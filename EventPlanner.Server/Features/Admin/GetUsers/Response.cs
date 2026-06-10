using System;

namespace EventPlanner.Server.Features.Admin.GetUsers;

public record GetUsersResponse(
    string Id,
    string Username,
    string Email,
    string Role,
    bool IsBanned,
    DateTime CreatedAt
);
