using Metering.Domain;

namespace Metering.Application;

public sealed class MeteringApplicationService
{
    private readonly IMeterReadingRepository _readings;
    private readonly ICustomerMeterRegistry _customerMeters;
    private readonly ConsumptionCalculator _consumptionCalculator;

    public MeteringApplicationService(
        IMeterReadingRepository readings,
        ICustomerMeterRegistry customerMeters,
        ConsumptionCalculator consumptionCalculator)
    {
        _readings = readings;
        _customerMeters = customerMeters;
        _consumptionCalculator = consumptionCalculator;
    }

    public void RegisterMeter(string customerId, string meterId)
    {
        ValidateRequired(customerId, nameof(customerId));
        ValidateRequired(meterId, nameof(meterId));

        _customerMeters.Register(customerId, meterId);
    }

    public void SubmitReading(string meterId, int month, decimal valueKwh)
    {
        SubmitReading(meterId, BillingPeriod.FromMonthOnly(month), valueKwh);
    }

    public void SubmitReading(string meterId, int year, int month, decimal valueKwh)
    {
        SubmitReading(meterId, new BillingPeriod(year, month), valueKwh);
    }

    public void SubmitReading(string meterId, BillingPeriod period, decimal valueKwh)
    {
        ValidateRequired(meterId, nameof(meterId));

        if (valueKwh < 0)
            throw new ArgumentException("Reading cannot be negative.", nameof(valueKwh));

        _readings.Save(new MeterReading(meterId, period, valueKwh));
    }

    public MeterReading? GetReading(string meterId, int month)
    {
        return GetReading(meterId, BillingPeriod.FromMonthOnly(month));
    }

    public MeterReading? GetReading(string meterId, int year, int month)
    {
        return GetReading(meterId, new BillingPeriod(year, month));
    }

    public MeterReading? GetReading(string meterId, BillingPeriod period)
    {
        ValidateRequired(meterId, nameof(meterId));

        return _readings.Get(meterId, period);
    }

    public decimal GetMeterConsumption(string meterId, int month)
    {
        return GetMeterConsumption(meterId, BillingPeriod.FromMonthOnly(month));
    }

    public decimal GetMeterConsumption(string meterId, int year, int month)
    {
        return GetMeterConsumption(meterId, new BillingPeriod(year, month));
    }

    public decimal GetMeterConsumption(string meterId, BillingPeriod period)
    {
        ValidateRequired(meterId, nameof(meterId));

        var current = _readings.Get(meterId, period);
        var previous = _readings.Get(meterId, period.Previous());

        if (current is null)
            throw new InvalidOperationException("Current reading is missing.");

        if (previous is null)
            throw new InvalidOperationException("Previous reading is missing.");

        return _consumptionCalculator.Calculate(current, previous);
    }

    public decimal GetCustomerConsumption(string customerId, int month)
    {
        return GetCustomerConsumption(customerId, BillingPeriod.FromMonthOnly(month));
    }

    public decimal GetCustomerConsumption(string customerId, int year, int month)
    {
        return GetCustomerConsumption(customerId, new BillingPeriod(year, month));
    }

    public decimal GetCustomerConsumption(string customerId, BillingPeriod period)
    {
        ValidateRequired(customerId, nameof(customerId));

        var meterIds = _customerMeters.GetMeterIds(customerId);

        decimal total = 0m;

        foreach (var meterId in meterIds)
        {
            total += GetMeterConsumption(meterId, period);
        }

        return total;
    }

    public Invoice CreateInvoice(string customerId, int month, decimal unitPrice)
    {
        ValidateRequired(customerId, nameof(customerId));
        ValidateMonth(month);

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        var consumption = GetCustomerConsumption(customerId, month);
        var total = consumption * unitPrice;

        return new Invoice(customerId, month, consumption, unitPrice, total);
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} is required.", parameterName);
    }

    private static void ValidateMonth(int month)
    {
        if (month <= 0)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be positive.");
    }
}
