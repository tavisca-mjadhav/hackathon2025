namespace OrderAPI.Models
{
    public enum Status
    {
        Completed,
        Pending,
        Cancelled
    }

    public enum PaymentStatus
    {
        FullyPaid,
        PartiallyPaid,
        FullyRefunded,
        PartiallyRefunded,
        Failed
    }
}
