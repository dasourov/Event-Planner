using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public class CreateEventHandler : IRequestHandler<CreateEventCommand, CreateEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateEventHandler(IEventRepository eventRepository, ICategoryRepository categoryRepository)
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CreateEventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        var @event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Date = request.Date,
            CategoryId = request.CategoryId,
            MaxAttendees = request.MaxAttendees,
            OrganizerId = request.OrganizerId,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        await _eventRepository.CreateAsync(@event);

        return new CreateEventResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Location,
            @event.Latitude,
            @event.Longitude,
            @event.Date,
            @event.CategoryId,
            @event.MaxAttendees,
            @event.Status.ToString(),
            @event.OrganizerId
        );
    }
}
