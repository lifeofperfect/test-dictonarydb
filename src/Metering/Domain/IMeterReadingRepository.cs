namespace Metering.Domain;

public interface IMeterReadingRepository
{
    void Save(MeterReading reading);

    MeterReading? Get(string meterId, int month);

    MeterReading? Get(string meterId, BillingPeriod period);
}
