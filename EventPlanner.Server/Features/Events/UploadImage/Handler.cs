using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EventPlanner.Server.Features.Events.UploadImage;

public class UploadImageHandler : IRequestHandler<UploadImageCommand, UploadImageResponse>
{
    private readonly IWebHostEnvironment _env;

    public UploadImageHandler(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<UploadImageResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new Exception("No file was uploaded.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, ext) < 0)
        {
            throw new Exception("Invalid file type. Only images are allowed.");
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "images");
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString("N") + ext;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        // Return a relative URL that can be requested from the frontend
        var url = $"/uploads/images/{uniqueFileName}";
        return new UploadImageResponse(url);
    }
}
