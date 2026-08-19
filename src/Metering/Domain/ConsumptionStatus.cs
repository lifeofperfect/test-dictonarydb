namespace Metering.Domain;

public enum ConsumptionStatus
{
    Calculated,
    CurrentReadingMissing,
    PreviousReadingMissing,
    ReadingWentBackwards
}
