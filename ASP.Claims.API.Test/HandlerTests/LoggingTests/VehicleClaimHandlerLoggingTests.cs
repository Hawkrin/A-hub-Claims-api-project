using ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using AutoMapper;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace ASP.Claims.API.Test.HandlerTests.LoggingTests;

public class VehicleClaimHandlerLoggingTests
{
    private readonly Mock<IClaimRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IClaimStatusEvaluator> _mockStatusEvaluator;

    public VehicleClaimHandlerLoggingTests()
    {
        _mockRepository = new Mock<IClaimRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockStatusEvaluator = new Mock<IClaimStatusEvaluator>();
    }

    [Fact]
    public async Task CreateHandler_LogsInformation_WhenClaimCreated()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreateVehicleClaimHandler>();
        var command = new CreateVehicleClaimCommand(
            RegistrationNumber: "ABC123",
            PlaceOfAccident: "Highway 101",
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new VehicleClaim
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Highway 101",
            Status = ClaimStatus.None
        };

        _mockMapper.Setup(m => m.Map<VehicleClaim>(command)).Returns(claim);
        _mockStatusEvaluator.Setup(e => e.Evaluate(claim, null)).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Ok());

        var handler = new CreateVehicleClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Vehicle claim created") &&
            log.Message.Contains("ABC123") &&
            log.Message.Contains("Highway 101"));
    }

    [Fact]
    public async Task CreateHandler_LogsError_WhenSaveFails()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreateVehicleClaimHandler>();
        var command = new CreateVehicleClaimCommand(
            RegistrationNumber: "ABC123",
            PlaceOfAccident: "Highway 101",
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new VehicleClaim { Id = Guid.NewGuid() };

        _mockMapper.Setup(m => m.Map<VehicleClaim>(command)).Returns(claim);
        _mockStatusEvaluator.Setup(e => e.Evaluate(claim, null)).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Fail("Database error"));

        var handler = new CreateVehicleClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Error && 
            log.Message.Contains("Failed to create vehicle claim"));
    }

    [Fact]
    public async Task UpdateHandler_LogsInformation_WhenStatusChanges()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdateVehicleClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdateVehicleClaimCommand(
            Id: claimId,
            RegistrationNumber: "ABC123",
            PlaceOfAccident: "Highway 101",
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        var existingClaim = new VehicleClaim
        {
            Id = claimId,
            Status = ClaimStatus.RequiresManualReview
        };

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync(existingClaim);
        _mockMapper.Setup(m => m.Map(command, existingClaim)).Returns(existingClaim);
        _mockStatusEvaluator.Setup(e => e.Evaluate(existingClaim, null)).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.UpdateClaim(existingClaim)).ReturnsAsync(Result.Ok());

        var handler = new UpdateVehicleClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Vehicle claim status changed") &&
            log.Message.Contains("RequiresManualReview") &&
            log.Message.Contains("None"));
    }

    [Fact]
    public async Task UpdateHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdateVehicleClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdateVehicleClaimCommand(
            Id: claimId,
            RegistrationNumber: "ABC123",
            PlaceOfAccident: "Highway 101",
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new UpdateVehicleClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Attempted to update non-existent vehicle claim"));
    }

    [Fact]
    public async Task DeleteHandler_LogsInformation_WhenDeleteSucceeds()
    {
        // Arrange
        var fakeLogger = new FakeLogger<DeleteVehicleClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new DeleteVehicleClaimCommand(claimId);

        _mockRepository.Setup(r => r.DeleteClaim(claimId)).ReturnsAsync(Result.Ok());

        var handler = new DeleteVehicleClaimHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Vehicle claim deleted"));
    }

    [Fact]
    public async Task QueryHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<VehicleClaimQueryHandler>();
        var claimId = Guid.NewGuid();
        var query = new GetVehicleClaimByIdQuery(claimId);

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new VehicleClaimQueryHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Vehicle claim not found"));
    }
}
