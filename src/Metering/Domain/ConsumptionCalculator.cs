namespace Metering.Domain;

public sealed class ConsumptionCalculator
{
    public decimal Calculate(MeterReading current, MeterReading previous)
    {
        if (current.MeterId != previous.MeterId)
            throw new InvalidOperationException("Readings must belong to the same meter.");

        if (current.Period.Previous() != previous.Period)
            throw new InvalidOperationException("Readings must be for consecutive months.");

        var consumption = current.ValueKwh - previous.ValueKwh;

        if (consumption < 0)
            throw new InvalidOperationException("Reading cannot go backwards.");

        return consumption;
    }
}
