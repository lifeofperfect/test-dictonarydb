namespace Metering.Domain;

public sealed record MeterReading
{
    public MeterReading(string meterId, int month, decimal valueKwh)
        : this(meterId, BillingPeriod.FromMonthOnly(month), valueKwh)
    {
    }

    public MeterReading(string meterId, BillingPeriod period, decimal valueKwh)
    {
        MeterId = meterId;
        Period = period;
        ValueKwh = valueKwh;
    }

    public string MeterId { get; }

    public BillingPeriod Period { get; }

    public int Year => Period.Year;

    public int Month => Period.Month;

    public decimal ValueKwh { get; }
}
