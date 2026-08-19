using Metering.Domain;

namespace Metering.Infrastructure;

public sealed class InMemoryCustomerMeterRegistry : ICustomerMeterRegistry
{
    private readonly Dictionary<string, HashSet<string>> _customerMeters = new();

    public void Register(string customerId, string meterId)
    {
        if (!_customerMeters.TryGetValue(customerId, out var meterIds))
        {
            meterIds = new HashSet<string>();
            _customerMeters[customerId] = meterIds;
        }

        meterIds.Add(meterId);
    }

    public IReadOnlyCollection<string> GetMeterIds(string customerId)
    {
        if (!_customerMeters.TryGetValue(customerId, out var meterIds))
            return Array.Empty<string>();

        return meterIds.ToArray();
    }
}
