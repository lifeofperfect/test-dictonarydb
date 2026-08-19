namespace Metering.Domain;

public interface ICustomerMeterRegistry
{
    void Register(string customerId, string meterId);

    IReadOnlyCollection<string> GetMeterIds(string customerId);
}
