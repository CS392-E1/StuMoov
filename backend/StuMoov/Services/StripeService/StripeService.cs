using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using StuMoov.Dao;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Services.StripeService
{
    public class StripeService
    {
        private readonly IConfiguration _configuration;
        private readonly StripeCustomerDao _stripeCustomerDao;
        private readonly StripeConnectAccountDao _stripeConnectAccountDao;
        private readonly UserDao _userDao;

        public StripeService(
            IConfiguration configuration,
            StripeCustomerDao stripeCustomerDao,
            StripeConnectAccountDao stripeConnectAccountDao,
            UserDao userDao)
        {
            _configuration = configuration;
            _stripeCustomerDao = stripeCustomerDao;
            _stripeConnectAccountDao = stripeConnectAccountDao;
            _userDao = userDao;
        }

        public async Task<Account> CreateConnectAccountAsync(string email, string userId)
        {
            AccountCreateOptions options = new AccountCreateOptions
            {
                Type = "express",
                Email = email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                },
                BusinessType = "individual",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            };

            AccountService service = new AccountService();
            return await service.CreateAsync(options);
        }

        public async Task<StripeConnectAccount?> CreateConnectAccountForLenderAsync(Guid lenderId)
        {
            // Get the lender from the db
            Lender? lender = await _userDao.GetUserByIdAsync(lenderId) as Lender;
            if (lender == null)
            {
                return null;
            }

            // Check if this lender already has a Connect account
            StripeConnectAccount? existingAccount = await _stripeConnectAccountDao.GetByUserIdAsync(lenderId);
            if (existingAccount != null)
            {
                return existingAccount;
            }

            // Create the Stripe Connect account
            Account account = await CreateConnectAccountAsync(lender.Email, lender.Id.ToString());

            // Create and save a StripeConnectAccount record
            StripeConnectAccount connectAccount = new StripeConnectAccount(lender, account.Id);
            return await _stripeConnectAccountDao.AddAsync(connectAccount);
        }

        public async Task<AccountLink> CreateAccountLinkAsync(string accountId, string refreshUrl, string returnUrl)
        {
            AccountLinkCreateOptions options = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = refreshUrl,
                ReturnUrl = returnUrl,
                Type = "account_onboarding",
            };

            AccountLinkService service = new AccountLinkService();
            return await service.CreateAsync(options);
        }

        public async Task<string?> CreateOnboardingLinkForLenderAsync(Guid lenderId, string refreshUrl, string returnUrl)
        {
            // Get the Connect account from the db
            StripeConnectAccount? connectAccount = await _stripeConnectAccountDao.GetByUserIdAsync(lenderId);
            if (connectAccount == null)
            {
                return null;
            }

            // Create the account link
            AccountLink accountLink = await CreateAccountLinkAsync(connectAccount.StripeConnectAccountId, refreshUrl, returnUrl);

            // Update the account link URL in the db
            await _stripeConnectAccountDao.UpdateAccountLinkUrlAsync(connectAccount.Id, accountLink.Url);

            return accountLink.Url;
        }

        public async Task<Account> GetAccountAsync(string accountId)
        {
            AccountService service = new AccountService();
            return await service.GetAsync(accountId);
        }

        public async Task<StripeConnectAccount?> UpdateAccountStatusAsync(Guid lenderId)
        {
            StripeConnectAccount? connectAccount = await _stripeConnectAccountDao.GetByUserIdAsync(lenderId);
            if (connectAccount == null)
            {
                return null;
            }

            // Get the latest account status from Stripe
            Account account = await GetAccountAsync(connectAccount.StripeConnectAccountId);

            // Determine the account status based on Stripe's response
            StripeConnectAccountStatus status = DetermineAccountStatus(account);
            // Update the status in the database
            return await _stripeConnectAccountDao.UpdateStatusAsync(connectAccount.Id, status, account.PayoutsEnabled == true);
        }

        private StripeConnectAccountStatus DetermineAccountStatus(Account account)
        {
            // Check if there are any requirements due or past due
            if ((account.Requirements?.CurrentlyDue != null && account.Requirements.CurrentlyDue.Count > 0) ||
                (account.Requirements?.PastDue != null && account.Requirements.PastDue.Count > 0))
            {
                // If there are pending requirements, account is RESTRICTED
                return StripeConnectAccountStatus.RESTRICTED;
            }
            else if (account.PayoutsEnabled != true)
            {
                // If payouts are not enabled, account is PENDING
                return StripeConnectAccountStatus.PENDING;
            }
            else
            {
                // If payouts are enabled and no requirements are due, account is COMPLETED
                return StripeConnectAccountStatus.COMPLETED;
            }
        }

        public async Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(
            string customerId,
            string priceId,
            string successUrl,
            string cancelUrl,
            string connectedAccountId,
            long applicationFeeAmount)
        {
            SessionCreateOptions options = new SessionCreateOptions
            {
                Mode = "payment",
                Customer = customerId,
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                PaymentIntentData = new Stripe.Checkout.SessionPaymentIntentDataOptions
                {
                    ApplicationFeeAmount = applicationFeeAmount,
                    TransferData = new Stripe.Checkout.SessionPaymentIntentDataTransferDataOptions
                    {
                        Destination = connectedAccountId,
                    },
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            SessionService service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<Customer> CreateCustomerAsync(string email, string name, string userId)
        {
            CustomerCreateOptions options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId }
                }
            };

            CustomerService service = new CustomerService();
            return await service.CreateAsync(options);
        }


        public async Task<StripeCustomer?> CreateCustomerForRenterAsync(Guid renterId)
        {
            // Get the renter from the database
            Renter? renter = await _userDao.GetUserByIdAsync(renterId) as Renter;
            if (renter == null)
            {
                return null;
            }

            // Check if this renter exists in our db
            StripeCustomer? existingCustomer = await _stripeCustomerDao.GetByUserIdAsync(renterId);
            if (existingCustomer != null)
            {
                // If they exist but no Stripe ID, create one
                if (string.IsNullOrEmpty(existingCustomer.StripeCustomerId))
                {
                    var customer = await CreateCustomerAsync(renter.Email, renter.DisplayName!, renter.Id.ToString());
                    return await _stripeCustomerDao.UpdateStripeDetailsAsync(renterId, customer.Id);
                }
                return existingCustomer;
            }

            // Create a new db entry
            StripeCustomer newCustomer = new StripeCustomer(renter);
            StripeCustomer? savedCustomer = await _stripeCustomerDao.AddAsync(newCustomer);

            if (savedCustomer != null)
            {
                // Create the Stripe customer
                Customer customer = await CreateCustomerAsync(renter.Email, renter.DisplayName!, renter.Id.ToString());

                // Update the db entry with the Stripe customer ID
                return await _stripeCustomerDao.UpdateStripeDetailsAsync(renterId, customer.Id);
            }

            return null;
        }

        // Method to handle account updates from webhooks
        public async Task<StripeConnectAccount?> UpdateAccountFromWebhookAsync(Account account)
        {
            if (account == null) return null;

            // Find by Stripe Connect Account ID
            StripeConnectAccount? connectAccount = await _stripeConnectAccountDao.GetByStripeConnectAccountIdAsync(account.Id);
            if (connectAccount == null) return null;

            // Determine the new status based on the account details
            StripeConnectAccountStatus status = DetermineAccountStatus(account);

            // Update the account status in our db
            return await _stripeConnectAccountDao.UpdateStatusAsync(
                connectAccount.Id,
                status,
                account.PayoutsEnabled == true);
        }
    }
}