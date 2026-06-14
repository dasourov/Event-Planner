using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Bookings.LeaveEvent;

public class LeaveEventHandler : IRequestHandler<LeaveEventCommand, LeaveEventResponse>
{
    private readonly IBookingRepository _bookingRepository;

    public LeaveEventHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<LeaveEventResponse> Handle(LeaveEventCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByUserAndEventAsync(request.UserId, request.EventId);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found");
        }

        await _bookingRepository.DeleteAsync(booking.Id);
        return new LeaveEventResponse(true);
    }
}
