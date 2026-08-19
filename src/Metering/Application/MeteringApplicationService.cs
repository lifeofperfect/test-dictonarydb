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

    public void CorrectReading(string meterId, int month, decimal correctedValueKwh)
    {
        CorrectReading(meterId, BillingPeriod.FromMonthOnly(month), correctedValueKwh);
    }

    public void CorrectReading(string meterId, int year, int month, decimal correctedValueKwh)
    {
        CorrectReading(meterId, new BillingPeriod(year, month), correctedValueKwh);
    }

    public void CorrectReading(string meterId, BillingPeriod period, decimal correctedValueKwh)
    {
        ValidateRequired(meterId, nameof(meterId));

        if (correctedValueKwh < 0)
            throw new ArgumentException("Reading cannot be negative.", nameof(correctedValueKwh));

        _readings.Replace(new MeterReading(meterId, period, correctedValueKwh));
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
        var result = GetMeterConsumptionResult(meterId, period);

        if (result.Status != ConsumptionStatus.Calculated)
            throw new InvalidOperationException(result.ErrorMessage);

        return result.ConsumptionKwh!.Value;
    }

    public ConsumptionResult GetMeterConsumptionResult(string meterId, int month)
    {
        return GetMeterConsumptionResult(meterId, BillingPeriod.FromMonthOnly(month));
    }

    public ConsumptionResult GetMeterConsumptionResult(string meterId, int year, int month)
    {
        return GetMeterConsumptionResult(meterId, new BillingPeriod(year, month));
    }

    public ConsumptionResult GetMeterConsumptionResult(string meterId, BillingPeriod period)
    {
        ValidateRequired(meterId, nameof(meterId));

        var current = _readings.Get(meterId, period);
        var previous = _readings.Get(meterId, period.Previous());

        if (current is null)
            return ConsumptionResult.Failed(
                ConsumptionStatus.CurrentReadingMissing,
                "Current reading is missing.");

        if (previous is null)
            return ConsumptionResult.Failed(
                ConsumptionStatus.PreviousReadingMissing,
                "Previous reading is missing.");

        try
        {
            return ConsumptionResult.Calculated(_consumptionCalculator.Calculate(current, previous));
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message == "Reading cannot go backwards.")
            {
                return ConsumptionResult.Failed(
                    ConsumptionStatus.ReadingWentBackwards,
                    exception.Message);
            }

            throw;
        }
    }

    public decimal GetMeterConsumptionForPeriod(string meterId, int fromYear, int fromMonth, int toYear, int toMonth)
    {
        return GetMeterConsumptionForPeriod(
            meterId,
            new BillingPeriod(fromYear, fromMonth),
            new BillingPeriod(toYear, toMonth));
    }

    public decimal GetMeterConsumptionForPeriod(string meterId, BillingPeriod fromPeriod, BillingPeriod toPeriod)
    {
        ValidateRequired(meterId, nameof(meterId));

        if (toPeriod.CompareTo(fromPeriod) <= 0)
            throw new ArgumentException("To period must be after from period.");

        var start = _readings.Get(meterId, fromPeriod);
        var end = _readings.Get(meterId, toPeriod);

        if (start is null)
            throw new InvalidOperationException("Start reading is missing.");

        if (end is null)
            throw new InvalidOperationException("End reading is missing.");

        var consumption = end.ValueKwh - start.ValueKwh;

        if (consumption < 0)
            throw new InvalidOperationException("Reading cannot go backwards.");

        return consumption;
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
