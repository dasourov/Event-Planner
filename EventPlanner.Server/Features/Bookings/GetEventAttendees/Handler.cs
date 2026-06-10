using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Bookings.GetEventAttendees;

public class GetEventAttendeesHandler : IRequestHandler<GetEventAttendeesQuery, List<GetEventAttendeesResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;

    public GetEventAttendeesHandler(IBookingRepository bookingRepository, IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
    }

    public async Task<List<GetEventAttendeesResponse>> Handle(GetEventAttendeesQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.ListByEventAsync(request.EventId);
        var resultList = new List<GetEventAttendeesResponse>();

        foreach (var booking in bookings)
        {
            var user = await _userRepository.GetByIdAsync(booking.UserId);
            if (user != null)
            {
                resultList.Add(new GetEventAttendeesResponse(user.Id, user.Username, user.Email));
            }
        }

        return resultList;
    }
}
