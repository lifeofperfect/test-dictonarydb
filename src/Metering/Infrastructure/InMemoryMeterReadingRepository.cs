using Metering.Domain;

namespace Metering.Infrastructure;

public sealed class InMemoryMeterReadingRepository : IMeterReadingRepository
{
    private readonly Dictionary<(string MeterId, BillingPeriod Period), MeterReading> _readings = new();

    public void Save(MeterReading reading)
    {
        _readings[(reading.MeterId, reading.Period)] = reading;
    }

    public MeterReading? Get(string meterId, int month)
    {
        return Get(meterId, BillingPeriod.FromMonthOnly(month));
    }

    public MeterReading? Get(string meterId, BillingPeriod period)
    {
        _readings.TryGetValue((meterId, period), out var reading);
        return reading;
    }
}
