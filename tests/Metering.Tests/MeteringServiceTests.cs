using Metering.Application;
using Metering.Domain;
using Metering.Infrastructure;

namespace Metering.Tests;

public sealed class MeteringServiceTests
{
    [Fact]
    public void CanStoreAndGetReading()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 1, 100m);

        var reading = service.GetReading("meter-1", 1);

        Assert.NotNull(reading);
        Assert.Equal("meter-1", reading.MeterId);
        Assert.Equal(1, reading.Month);
        Assert.Equal(100m, reading.ValueKwh);
    }

    [Fact]
    public void CanGetReadingByMeterIdYearAndMonth()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2025, 1, 100m);
        service.SubmitReading("meter-1", 2026, 1, 250m);

        var reading = service.GetReading("meter-1", 2026, 1);

        Assert.NotNull(reading);
        Assert.Equal("meter-1", reading.MeterId);
        Assert.Equal(2026, reading.Year);
        Assert.Equal(1, reading.Month);
        Assert.Equal(250m, reading.ValueKwh);
    }

    [Fact]
    public void CalculatesConsumptionAcrossYearBoundary()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2025, 12, 500m);
        service.SubmitReading("meter-1", 2026, 1, 560m);

        var consumption = service.GetMeterConsumption("meter-1", 2026, 1);

        Assert.Equal(60m, consumption);
    }

    [Fact]
    public void RejectsNegativeReading()
    {
        var service = CreateService();

        var exception = Assert.Throws<ArgumentException>(() =>
            service.SubmitReading("meter-1", 1, -10m));

        Assert.Equal("Reading cannot be negative. (Parameter 'valueKwh')", exception.Message);
    }

    [Fact]
    public void RejectsDuplicateReadingForSameMeterAndPeriod()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2026, 1, 100m);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.SubmitReading("meter-1", 2026, 1, 120m));

        Assert.Equal("Reading already exists for this meter and period.", exception.Message);
    }

    [Fact]
    public void CanCorrectExistingReading()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2026, 1, 100m);
        service.SubmitReading("meter-1", 2026, 2, 160m);

        service.CorrectReading("meter-1", 2026, 2, 155m);

        var consumption = service.GetMeterConsumption("meter-1", 2026, 2);

        Assert.Equal(55m, consumption);
    }

    [Fact]
    public void CannotCorrectMissingReading()
    {
        var service = CreateService();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.CorrectReading("meter-1", 2026, 1, 100m));

        Assert.Equal("Cannot correct a reading that does not exist.", exception.Message);
    }

    [Fact]
    public void CalculatesMeterConsumptionFromCurrentAndPreviousReadings()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 1, 100m);
        service.SubmitReading("meter-1", 2, 160m);

        var consumption = service.GetMeterConsumption("meter-1", 2);

        Assert.Equal(60m, consumption);
    }

    [Fact]
    public void CalculatesMeterConsumptionForPeriod()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2026, 1, 100m);
        service.SubmitReading("meter-1", 2026, 4, 250m);

        var consumption = service.GetMeterConsumptionForPeriod("meter-1", 2026, 1, 2026, 4);

        Assert.Equal(150m, consumption);
    }

    [Fact]
    public void PeriodConsumptionRequiresEndPeriodAfterStartPeriod()
    {
        var service = CreateService();

        var exception = Assert.Throws<ArgumentException>(() =>
            service.GetMeterConsumptionForPeriod("meter-1", 2026, 4, 2026, 1));

        Assert.Equal("To period must be after from period.", exception.Message);
    }

    [Fact]
    public void CannotCalculateConsumptionWithoutPreviousReading()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2, 160m);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GetMeterConsumption("meter-1", 2));

        Assert.Equal("Previous reading is missing.", exception.Message);
    }

    [Fact]
    public void CanReturnConsumptionResultWhenPreviousReadingIsMissing()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2026, 2, 160m);

        var result = service.GetMeterConsumptionResult("meter-1", 2026, 2);

        Assert.Equal(ConsumptionStatus.PreviousReadingMissing, result.Status);
        Assert.Null(result.ConsumptionKwh);
        Assert.Equal("Previous reading is missing.", result.ErrorMessage);
    }

    [Fact]
    public void ReadingCannotGoBackwards()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 1, 200m);
        service.SubmitReading("meter-1", 2, 150m);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            service.GetMeterConsumption("meter-1", 2));

        Assert.Equal("Reading cannot go backwards.", exception.Message);
    }

    [Fact]
    public void CanReturnConsumptionResultWhenReadingGoesBackwards()
    {
        var service = CreateService();

        service.SubmitReading("meter-1", 2026, 1, 200m);
        service.SubmitReading("meter-1", 2026, 2, 150m);

        var result = service.GetMeterConsumptionResult("meter-1", 2026, 2);

        Assert.Equal(ConsumptionStatus.ReadingWentBackwards, result.Status);
        Assert.Null(result.ConsumptionKwh);
        Assert.Equal("Reading cannot go backwards.", result.ErrorMessage);
    }

    [Fact]
    public void CalculatesCustomerConsumptionAcrossMultipleMeters()
    {
        var service = CreateService();

        service.RegisterMeter("customer-1", "meter-1");
        service.RegisterMeter("customer-1", "meter-2");

        service.SubmitReading("meter-1", 1, 100m);
        service.SubmitReading("meter-1", 2, 160m);

        service.SubmitReading("meter-2", 1, 50m);
        service.SubmitReading("meter-2", 2, 90m);

        var consumption = service.GetCustomerConsumption("customer-1", 2);

        Assert.Equal(100m, consumption);
    }

    [Fact]
    public void RegisteringSameMeterTwiceDoesNotDoubleCountConsumption()
    {
        var service = CreateService();

        service.RegisterMeter("customer-1", "meter-1");
        service.RegisterMeter("customer-1", "meter-1");

        service.SubmitReading("meter-1", 1, 100m);
        service.SubmitReading("meter-1", 2, 160m);

        var consumption = service.GetCustomerConsumption("customer-1", 2);

        Assert.Equal(60m, consumption);
    }

    [Fact]
    public void CreatesInvoiceForCustomer()
    {
        var service = CreateService();

        service.RegisterMeter("customer-1", "meter-1");
        service.SubmitReading("meter-1", 1, 100m);
        service.SubmitReading("meter-1", 2, 160m);

        var invoice = service.CreateInvoice("customer-1", 2, 0.30m);

        Assert.Equal("customer-1", invoice.CustomerId);
        Assert.Equal(2, invoice.Month);
        Assert.Equal(60m, invoice.ConsumptionKwh);
        Assert.Equal(0.30m, invoice.UnitPrice);
        Assert.Equal(18.00m, invoice.Total);
    }

    private static MeteringApplicationService CreateService()
    {
        return new MeteringApplicationService(
            new InMemoryMeterReadingRepository(),
            new InMemoryCustomerMeterRegistry(),
            new ConsumptionCalculator());
    }
}
