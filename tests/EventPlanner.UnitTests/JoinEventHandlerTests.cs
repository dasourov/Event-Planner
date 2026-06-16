using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MongoDB.Bson;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Features.Bookings.JoinEvent;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class JoinEventHandlerTests
{
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly JoinEventHandler _handler;

    private const string EventId = "65f000000000000000000001";
    private const string UserId = "65f000000000000000000002";

    public JoinEventHandlerTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _handler = new JoinEventHandler(_bookingRepositoryMock.Object, _eventRepositoryMock.Object);

        // Simulate MongoDB assigning an ObjectId on insert
        _bookingRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => b.Id = ObjectId.GenerateNewId().ToString())
            .Returns(Task.CompletedTask);
    }

    private Event PublishedEvent(int? maxAttendees = null) => new Event
    {
        Id = EventId,
        Title = "Test Event",
        Status = EventStatus.Published,
        MaxAttendees = maxAttendees
    };

    [Fact]
    public async Task Handle_ShouldCreateBooking_WhenEventIsPublishedAndHasNoCapacityLimit()
    {
        // Arrange
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(PublishedEvent());
        _bookingRepositoryMock.Setup(repo => repo.GetByUserAndEventAsync(UserId, EventId)).ReturnsAsync((Booking?)null);

        // Act
        var response = await _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.NotEmpty(response.BookingId);
        _bookingRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync((Event?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None));
        _bookingRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEventIsNotPublished()
    {
        // Arrange
        var draftEvent = PublishedEvent();
        draftEvent.Status = EventStatus.Draft;
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(draftEvent);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None));
        _bookingRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUserIsAlreadyBooked()
    {
        // Arrange
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(PublishedEvent());
        _bookingRepositoryMock.Setup(repo => repo.GetByUserAndEventAsync(UserId, EventId)).ReturnsAsync(new Booking { Id = "existing" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None));
        _bookingRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEventCapacityIsReached()
    {
        // Arrange
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(PublishedEvent(maxAttendees: 2));
        _bookingRepositoryMock.Setup(repo => repo.GetByUserAndEventAsync(UserId, EventId)).ReturnsAsync((Booking?)null);
        _bookingRepositoryMock.Setup(repo => repo.CountByEventAsync(EventId)).ReturnsAsync(2);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None));
        _bookingRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateBooking_WhenCapacityHasRoom()
    {
        // Arrange
        Booking? created = null;
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(PublishedEvent(maxAttendees: 2));
        _bookingRepositoryMock.Setup(repo => repo.GetByUserAndEventAsync(UserId, EventId)).ReturnsAsync((Booking?)null);
        _bookingRepositoryMock.Setup(repo => repo.CountByEventAsync(EventId)).ReturnsAsync(1);
        _bookingRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => { b.Id = "65f0000000000000000000ff"; created = b; })
            .Returns(Task.CompletedTask);
        _bookingRepositoryMock
            .Setup(repo => repo.ListByEventAsync(EventId))
            .ReturnsAsync(() => new List<Booking>
            {
                new Booking { Id = "65f0000000000000000000aa", EventId = EventId },
                created!
            });

        // Act
        var response = await _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        _bookingRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollBackBooking_WhenConcurrentJoinOversellsCapacity()
    {
        // Arrange: pre-insert count passes (1 of 2 seats taken), but by the time the
        // booking lands, two other bookings already exist and ours ranks last (rank 3 of 2)
        Booking? created = null;
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(PublishedEvent(maxAttendees: 2));
        _bookingRepositoryMock.Setup(repo => repo.GetByUserAndEventAsync(UserId, EventId)).ReturnsAsync((Booking?)null);
        _bookingRepositoryMock.Setup(repo => repo.CountByEventAsync(EventId)).ReturnsAsync(1);
        _bookingRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Booking>()))
            .Callback<Booking>(b => { b.Id = "65f0000000000000000000ff"; created = b; })
            .Returns(Task.CompletedTask);
        _bookingRepositoryMock
            .Setup(repo => repo.ListByEventAsync(EventId))
            .ReturnsAsync(() => new List<Booking>
            {
                new Booking { Id = "65f0000000000000000000aa", EventId = EventId },
                new Booking { Id = "65f0000000000000000000bb", EventId = EventId },
                created!
            });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(new JoinEventCommand(EventId, UserId), CancellationToken.None));
        _bookingRepositoryMock.Verify(repo => repo.DeleteAsync("65f0000000000000000000ff"), Times.Once);
    }
}
