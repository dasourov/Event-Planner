using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Bson;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public class CreateEventHandler : IRequestHandler<CreateEventCommand, CreateEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateEventHandler(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository
    )
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CreateEventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var category = await ResolveCategoryAsync(request.CategoryId);

        var @event = new Event
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Date = request.Date,
            CategoryId = category.Id,
            MaxAttendees = request.MaxAttendees,
            ImageUrl = request.ImageUrl,
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
            @event.ImageUrl,
            @event.Latitude,
            @event.Longitude,
            @event.Date,
            @event.CategoryId,
            @event.MaxAttendees,
            @event.Status.ToString(),
            @event.OrganizerId
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