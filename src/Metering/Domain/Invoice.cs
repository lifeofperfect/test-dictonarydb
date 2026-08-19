namespace Metering.Domain;

public sealed record Invoice(
    string CustomerId,
    int Month,
    decimal ConsumptionKwh,
    decimal UnitPrice,
    decimal Total
);
