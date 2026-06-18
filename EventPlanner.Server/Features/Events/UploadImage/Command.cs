using MediatR;
using Microsoft.AspNetCore.Http;

namespace EventPlanner.Server.Features.Events.UploadImage;

public record UploadImageCommand(IFormFile File) : IRequest<UploadImageResponse>;
