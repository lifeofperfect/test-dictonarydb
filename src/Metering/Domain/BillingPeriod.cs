namespace Metering.Domain;

public sealed record BillingPeriod : IComparable<BillingPeriod>
{
    public const int DefaultYear = 1;

    public int Year { get; }
    public int Month { get; }

    public BillingPeriod(int year, int month)
    {
        if (year <= 0)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be positive.");

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

        Year = year;
        Month = month;
    }

    public static BillingPeriod FromMonthOnly(int month)
    {
        return new BillingPeriod(DefaultYear, month);
    }

    public BillingPeriod Previous()
    {
        if (Month == 1)
            return new BillingPeriod(Year - 1, 12);

        return new BillingPeriod(Year, Month - 1);
    }

    public int CompareTo(BillingPeriod? other)
    {
        if (other is null)
            return 1;

        var yearComparison = Year.CompareTo(other.Year);

        if (yearComparison != 0)
            return yearComparison;

        return Month.CompareTo(other.Month);
    }
}
