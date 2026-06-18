using MediatR;

namespace EventPlanner.Server.Features.Auth.CheckAvailability;

public record CheckAvailabilityQuery(string? Username, string? Email) : IRequest<CheckAvailabilityResponse>;

public record CheckAvailabilityResponse(bool UsernameExists, bool EmailExists);
