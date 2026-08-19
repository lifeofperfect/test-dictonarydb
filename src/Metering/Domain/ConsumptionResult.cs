namespace Metering.Domain;

public sealed record ConsumptionResult(
    ConsumptionStatus Status,
    decimal? ConsumptionKwh,
    string? ErrorMessage)
{
    public static ConsumptionResult Calculated(decimal consumptionKwh)
    {
        return new ConsumptionResult(ConsumptionStatus.Calculated, consumptionKwh, null);
    }

    public static ConsumptionResult Failed(ConsumptionStatus status, string errorMessage)
    {
        return new ConsumptionResult(status, null, errorMessage);
    }
}
