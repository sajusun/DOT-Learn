namespace OrderPulse.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    Submitted = 2,
    Processing = 3,
    Paid = 4,
    Fulfilled = 5,
    Cancelled = 6
}
