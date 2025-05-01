/**
 * StripeService.cs
 *
 * Manages Stripe-related operations for the StuMoov application.
 * Provides functionality for creating and managing Stripe Connect accounts, customer accounts,
 * checkout sessions, and invoices for bookings. Integrates with DAOs for data access and Stripe APIs.
 */

using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using StuMoov.Dao;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;
using StuMoov.Models.BookingModel;
using StuMoov.Models.PaymentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace StuMoov.Services.StripeService
{
    /// <summary>
    /// Service responsible for handling Stripe-related operations, including Connect account management,
    /// customer creation, checkout sessions, and invoice generation for bookings.
    /// </summary>
    public class StripeService
    {
        /// <summary>
        /// Configuration for accessing Stripe API keys and other settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Data access object for Stripe customer-related database operations.
        /// </summary>
        private readonly StripeCustomerDao _stripeCustomerDao;

        /// <summary>
        /// Data access object for Stripe Connect account-related database operations.
        /// </summary>
        private readonly StripeConnectAccountDao _stripeConnectAccountDao;

        /// <summary>
        /// Data access object for user-related database operations.
        /// </summary>
        private readonly UserDao _userDao;

        /// <summary>
        /// Data access object for payment-related database operations.
        /// </summary>
        private readonly PaymentDao _paymentDao;

        /// <summary>
        /// Data access object for booking-related database operations.
        /// </summary>
        private readonly BookingDao _bookingDao;

        /// <summary>
        /// Logger for recording Stripe-related events and errors.
        /// </summary>
        private readonly ILogger<StripeService> _logger;

        /// <summary>
        /// Initializes a new instance of the StripeService with required dependencies and configures Stripe API key.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="stripeCustomerDao">DAO for Stripe customer operations.</param>
        /// <param name="stripeConnectAccountDao">DAO for Stripe Connect account operations.</param>
        /// <param name="userDao">DAO for user operations.</param>
        /// <param name="paymentDao">DAO for payment operations.</param>
        /// <param name="bookingDao">DAO for booking operations.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public StripeService(
            IConfiguration configuration,
            StripeCustomerDao stripeCustomerDao,
            StripeConnectAccountDao stripeConnectAccountDao,
            UserDao userDao,
            PaymentDao paymentDao,
            BookingDao bookingDao,
            ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _stripeCustomerDao = stripeCustomerDao;
            _stripeConnectAccountDao = stripeConnectAccountDao;
            _userDao = userDao;
            _paymentDao = paymentDao;
            _bookingDao = bookingDao;
            _logger = logger;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        /// <summary>
        /// Creates a Stripe Connect account for a user.
        /// </summary>
        /// <param name="email">The email address for the account.</param>
        /// <param name="userId">The ID of the user in the application.</param>
        /// <returns>The created Stripe Connect account.</returns>
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

        /// <summary>
        /// Creates or retrieves a Stripe Connect account for a lender.
        /// </summary>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <returns>The Stripe Connect account, or null if creation fails.</returns>
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

            StripeConnectAccount connectAccount = new StripeConnectAccount(lender, account.Id);
            return await _stripeConnectAccountDao.AddAsync(connectAccount);
        }

        /// <summary>
        /// Creates an account link for onboarding a Stripe Connect account.
        /// </summary>
        /// <param name="accountId">The Stripe Connect account ID.</param>
        /// <param name="refreshUrl">URL to redirect if the link expires.</param>
        /// <param name="returnUrl">URL to redirect after onboarding.</param>
        /// <returns>The created account link.</returns>
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

        /// <summary>
        /// Creates an onboarding link for a lender's Stripe Connect account.
        /// </summary>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <param name="refreshUrl">URL to redirect if the link expires.</param>
        /// <param name="returnUrl">URL to redirect after onboarding.</param>
        /// <returns>The onboarding link URL, or null if creation fails.</returns>
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

            await _stripeConnectAccountDao.UpdateAccountLinkUrlAsync(connectAccount.Id, accountLink.Url);

            return accountLink.Url;
        }

        /// <summary>
        /// Retrieves a Stripe Connect account by its ID.
        /// </summary>
        /// <param name="accountId">The Stripe Connect account ID.</param>
        /// <returns>The Stripe Connect account.</returns>
        public async Task<Account> GetAccountAsync(string accountId)
        {
            AccountService service = new AccountService();
            return await service.GetAsync(accountId);
        }

        /// <summary>
        /// Updates the status of a lender's Stripe Connect account based on its current state.
        /// </summary>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <returns>The updated Stripe Connect account, or null if not found.</returns>
        public async Task<StripeConnectAccount?> UpdateAccountStatusAsync(Guid lenderId)
        {
            StripeConnectAccount? connectAccount = await _stripeConnectAccountDao.GetByUserIdAsync(lenderId);
            if (connectAccount == null)
            {
                return null;
            }

            // Get the latest account status from Stripe
            Account account = await GetAccountAsync(connectAccount.StripeConnectAccountId);

            StripeConnectAccountStatus status = DetermineAccountStatus(account);
            return await _stripeConnectAccountDao.UpdateStatusAsync(connectAccount.Id, status, account.PayoutsEnabled == true);
        }

        /// <summary>
        /// Determines the status of a Stripe Connect account based on its requirements and payout status.
        /// </summary>
        /// <param name="account">The Stripe Connect account.</param>
        /// <returns>The determined account status.</returns>
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

        /// <summary>
        /// Creates a Stripe Checkout session for a payment.
        /// </summary>
        /// <param name="customerId">The Stripe customer ID.</param>
        /// <param name="priceId">The Stripe price ID.</param>
        /// <param name="successUrl">URL to redirect after successful payment.</param>
        /// <param name="cancelUrl">URL to redirect if payment is canceled.</param>
        /// <param name="connectedAccountId">The Stripe Connect account ID.</param>
        /// <param name="applicationFeeAmount">The application fee amount in cents.</param>
        /// <returns>The created Stripe Checkout session.</returns>
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

        /// <summary>
        /// Creates a Stripe customer for a user.
        /// </summary>
        /// <param name="email">The customer's email address.</param>
        /// <param name="name">The customer's name.</param>
        /// <param name="userId">The ID of the user in the application.</param>
        /// <returns>The created Stripe customer.</returns>
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

        /// <summary>
        /// Creates or retrieves a Stripe customer for a renter.
        /// </summary>
        /// <param name="renterId">The ID of the renter.</param>
        /// <returns>The Stripe customer, or null if creation fails.</returns>
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
                if (string.IsNullOrEmpty(existingCustomer.StripeCustomerId))
                {
                    var customer = await CreateCustomerAsync(renter.Email, renter.DisplayName!, renter.Id.ToString());
                    return await _stripeCustomerDao.UpdateStripeDetailsAsync(renterId, customer.Id);
                }
                return existingCustomer;
            }

            StripeCustomer newCustomer = new StripeCustomer(renter);
            StripeCustomer? savedCustomer = await _stripeCustomerDao.AddAsync(newCustomer);

            if (savedCustomer != null)
            {
                // Create the Stripe customer
                Customer customer = await CreateCustomerAsync(renter.Email, renter.DisplayName!, renter.Id.ToString());

                return await _stripeCustomerDao.UpdateStripeDetailsAsync(renterId, customer.Id);
            }

            return null;
        }

        /// <summary>
        /// Updates a Stripe Connect account's status based on webhook data.
        /// </summary>
        /// <param name="account">The Stripe Connect account from the webhook.</param>
        /// <returns>The updated Stripe Connect account, or null if not found.</returns>
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

        /// <summary>
        /// Creates and sends an invoice for a booking, updating the associated payment record.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>The updated payment record, or null if invoice creation fails.</returns>
        public async Task<Payment?> CreateAndSendInvoiceForBookingAsync(Guid bookingId)
        {
            _logger.LogInformation($"Creating invoice for Booking ID: {bookingId}");

            try
            {
                Booking? booking = await _bookingDao.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogError($"Booking not found for ID: {bookingId}");
                    return null;
                }
                if (booking.Renter == null || booking.StorageLocation == null || booking.Payment == null)
                {
                    _logger.LogError($"Booking {bookingId} is missing required related data (Renter, StorageLocation, or Payment).");
                    return null;
                }
                if (booking.StorageLocation.Lender == null)
                {
                    _logger.LogError($"Lender information not found for StorageLocation {booking.StorageLocation.Id} in Booking {bookingId}");
                    return null;
                }

                Guid lenderId = booking.StorageLocation.LenderId;

                StripeCustomer? renterStripeInfo = await _stripeCustomerDao.GetByUserIdAsync(booking.RenterId);

                if (renterStripeInfo == null || string.IsNullOrEmpty(renterStripeInfo.StripeCustomerId))
                {
                    _logger.LogError($"Stripe Customer ID not found for Renter ID: {booking.RenterId}");
                    return null;
                }
                string renterStripeCustomerId = renterStripeInfo.StripeCustomerId;

                StripeConnectAccount? lenderConnectInfo = await _stripeConnectAccountDao.GetByUserIdAsync(lenderId);
                if (lenderConnectInfo == null || string.IsNullOrEmpty(lenderConnectInfo.StripeConnectAccountId))
                {
                    _logger.LogError($"Stripe Connect Account ID not found for Lender ID: {lenderId}");
                    return null;
                }
                string lenderConnectAccountId = lenderConnectInfo.StripeConnectAccountId;

                if (booking.Payment.Status != PaymentStatus.DRAFT)
                {
                    _logger.LogWarning($"Invoice creation skipped for Booking {bookingId}. Payment status is {booking.Payment.Status}, expected DRAFT.");
                    return booking.Payment;
                }

                long amountInCents = (long)booking.TotalPrice;
                decimal applicationFeePercent = _configuration.GetValue<decimal>("Stripe:ApplicationFeePercent", 3m);
                long applicationFeeInCents = (long)Math.Round(amountInCents * (applicationFeePercent / 100m));
                long amountTransferredInCents = amountInCents - applicationFeeInCents;

                decimal localAmountCharged = booking.TotalPrice;
                decimal localPlatformFee = (decimal)applicationFeeInCents;
                decimal localAmountTransferred = (decimal)amountTransferredInCents;

                _logger.LogInformation($"Calculated amounts for Booking {bookingId}: Total={amountInCents}c, Fee={applicationFeeInCents}c, Transferred={amountTransferredInCents}c");

                // 1. Create the DRAFT Invoice first to get its ID
                var invoiceOptions = new InvoiceCreateOptions
                {
                    Customer = renterStripeCustomerId,
                    CollectionMethod = "send_invoice",
                    DaysUntilDue = 5,
                    TransferData = new InvoiceTransferDataOptions
                    {
                        Destination = lenderConnectAccountId,
                    },
                    ApplicationFeeAmount = applicationFeeInCents,
                    Metadata = new Dictionary<string, string>
                    {
                        { "bookingId", booking.Id.ToString() },
                        { "paymentId", booking.PaymentId.ToString()! }
                    },
                    Description = $"Invoice for Booking {booking.Id}"
                };

                var invoiceService = new InvoiceService();
                Invoice invoice = await invoiceService.CreateAsync(invoiceOptions);
                _logger.LogInformation($"Created Draft Invoice {invoice.Id} for Booking {bookingId}");

                // 2. Create the InvoiceItem
                var invoiceItemOptions = new InvoiceItemCreateOptions
                {
                    Customer = renterStripeCustomerId,
                    Amount = amountInCents,
                    Currency = "usd",
                    Description = $"Storage Rental: {booking.StorageLocation.Name} ({booking.StartDate.ToShortDateString()} - {booking.EndDate.ToShortDateString()}) - Booking ID: {booking.Id}",
                    Invoice = invoice.Id
                };

                var invoiceItemService = new InvoiceItemService();
                InvoiceItem invoiceItem = await invoiceItemService.CreateAsync(invoiceItemOptions);
                _logger.LogInformation($"Created InvoiceItem {invoiceItem.Id} and linked it to Invoice {invoice.Id}");

                // 3. Finalize the invoice
                if (invoice.Status == "draft")
                {
                    _logger.LogInformation($"Attempting to finalize draft Invoice {invoice.Id}");
                    try
                    {
                        InvoiceFinalizeOptions finalizeOptions = new InvoiceFinalizeOptions { AutoAdvance = true };
                        invoice = await invoiceService.FinalizeInvoiceAsync(invoice.Id, finalizeOptions);
                        _logger.LogInformation($"Successfully finalized Invoice {invoice.Id}. Status: {invoice.Status}");
                    }
                    catch (StripeException finalizeEx)
                    {
                        _logger.LogError(finalizeEx, $"Stripe error finalizing invoice {invoice.Id}: {finalizeEx.StripeError?.Message ?? finalizeEx.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Invoice {invoice.Id} was not in draft status before finalization attempt (Status: {invoice.Status}).");
                }

                _logger.LogInformation($"Updating local payment record for Invoice {invoice.Id}");

                PaymentStatus finalPaymentStatus = invoice.Status switch
                {
                    "open" => PaymentStatus.OPEN,
                    "paid" => PaymentStatus.PAID,
                    "void" => PaymentStatus.VOID,
                    "uncollectible" => PaymentStatus.UNCOLLECTIBLE,
                    _ => PaymentStatus.DRAFT
                };
                _logger.LogInformation($"Mapping final Invoice status '{invoice.Status}' to local PaymentStatus '{finalPaymentStatus}'");

                Payment? updatedPayment = await _paymentDao.UpdatePaymentWithInvoiceDetailsAsync(
                    booking.Payment.Id,
                    invoice.Id,
                    finalPaymentStatus,
                    localAmountCharged,
                    localPlatformFee,
                    localAmountTransferred
                );

                if (updatedPayment == null)
                {
                    _logger.LogError($"Failed to update local Payment record {booking.PaymentId} after creating Stripe Invoice {invoice.Id}");
                }
                else
                {
                    _logger.LogInformation($"Successfully updated local Payment record {updatedPayment.Id} with Invoice details.");
                }

                return updatedPayment;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Stripe error creating invoice for Booking ID {bookingId}: {ex.StripeError?.Message ?? ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error creating invoice for Booking ID {bookingId}");
                return null;
            }
        }
    }
}