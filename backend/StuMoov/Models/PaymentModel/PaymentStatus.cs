namespace StuMoov.Models.PaymentModel
{
    public enum PaymentStatus
    {
        CANCELLED,
        PROCESSING,
        REQUIRES_ACTION,
        REQUIRES_CAPTURE,
        REQUIRES_CONFIRMATION,
        REQUIRES_PAYMENT_METHOD,
        SUCCEEDED
    }
}
