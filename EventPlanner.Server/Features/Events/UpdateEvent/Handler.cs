using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.UpdateEvent;

public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public UpdateEventHandler(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository
    )
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<UpdateEventResponse> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.Id);
        if (@event == null)
        {
            throw new Exception("Event not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Only organizer or Admin can update
        if (@event.OrganizerId != request.UserId && user.Role != UserRole.Admin)
        {
            throw new Exception("Not authorized to update this event");
        }

        var category = await ResolveCategoryAsync(request.CategoryId);

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.Location = request.Location;
        @event.ImageUrl = request.ImageUrl;
        @event.Latitude = request.Latitude;
        @event.Longitude = request.Longitude;
        @event.Date = request.Date;
        @event.CategoryId = category.Id;
        @event.MaxAttendees = request.MaxAttendees;

        await _eventRepository.UpdateAsync(@event);

        return new UpdateEventResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Location,
            @event.ImageUrl,
            @event.Latitude,
            @event.Longitude,
            @event.Date,
            @event.CategoryId,
            @event.MaxAttendees,
            @event.Status.ToString()
        );
    }

    private async Task<Category> ResolveCategoryAsync(string categoryId)
    {
        Category? category = null;

        if (!string.IsNullOrWhiteSpace(categoryId) && ObjectId.TryParse(categoryId, out _))
        {
            category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category != null)
            {
                return category;
            }
        }

        var categoryName = GetCategoryName(categoryId);
        category = await _categoryRepository.GetByNameAsync(categoryName);
        if (category != null)
        {
            return category;
        }

        category = new Category
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = categoryName,
            Description = $"{categoryName} events"
        };

        await _categoryRepository.CreateAsync(category);
        return category;
    }

    private static string GetCategoryName(string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return "General";
        }

        return categoryId.Trim().ToLowerInvariant() switch
        {
            "dummy-cat-tech" => "Technology",
            "dummy-cat-arts" => "Art",
            "dummy-cat-wellness" => "Wellness",
            "dummy-cat-net" => "Networking",
            "dummy-cat-work" => "Workshop",
            "tech" => "Technology",
            "technology" => "Technology",
            "arts" => "Art",
            "art" => "Art",
            "wellness" => "Wellness",
            "networking" => "Networking",
            "workshop" => "Workshop",
            "music" => "Music",
            "sports" => "Sports",
            "food" => "Food",
            _ => "General"
        };
    }
}
