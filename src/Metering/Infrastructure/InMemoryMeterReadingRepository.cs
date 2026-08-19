using Metering.Domain;

namespace Metering.Infrastructure;

public sealed class InMemoryMeterReadingRepository : IMeterReadingRepository
{
    private readonly Dictionary<(string MeterId, BillingPeriod Period), MeterReading> _readings = new();

    public void Save(MeterReading reading)
    {
        if (_readings.ContainsKey((reading.MeterId, reading.Period)))
            throw new InvalidOperationException("Reading already exists for this meter and period.");

        _readings[(reading.MeterId, reading.Period)] = reading;
    }

    public void Replace(MeterReading reading)
    {
        if (!_readings.ContainsKey((reading.MeterId, reading.Period)))
            throw new InvalidOperationException("Cannot correct a reading that does not exist.");

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
