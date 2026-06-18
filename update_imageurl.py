import os
import re

def update_file(path, func):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    new_content = func(content)
    if new_content != content:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Updated {path}")

# Event Entity
def update_entity(c):
    if 'public string? ImageUrl' not in c:
        c = c.replace('public string Location { get; set; } = string.Empty;', 'public string Location { get; set; } = string.Empty;\n    public string? ImageUrl { get; set; }')
    return c

update_file(r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Domain\Entities\Event.cs', update_entity)

# Endpoint requests & commands (Adding string? ImageUrl after Location)
def update_record(c):
    if 'string? ImageUrl' not in c:
        c = c.replace('string Location,', 'string Location,\n    string? ImageUrl,')
        c = c.replace('req.Location,', 'req.Location,\n                req.ImageUrl,')
    return c

for file in [
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\CreateEvent\Endpoint.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\CreateEvent\Command.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\UpdateEvent\Endpoint.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\UpdateEvent\Command.cs'
]:
    update_file(file, update_record)

# Response DTOs
def update_response(c):
    if 'string? ImageUrl' not in c:
        c = c.replace('string Location,', 'string Location,\n    string? ImageUrl,')
    return c

for file in [
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\CreateEvent\Response.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\UpdateEvent\Response.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetEvents\Response.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetEventById\Response.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetMapEvents\Response.cs'
]:
    update_file(file, update_response)

# Handlers mapping
def update_handler(c):
    # For Create/Update handlers, setting the property
    if 'ImageUrl = request.ImageUrl' not in c and 'Title = request.Title' in c:
        c = c.replace('Location = request.Location,', 'Location = request.Location,\n            ImageUrl = request.ImageUrl,')
    
    # For returning Responses, getting the property
    if 'e.Location,' in c and 'e.ImageUrl,' not in c:
        c = c.replace('e.Location,', 'e.Location,\n                e.ImageUrl,')
    if '@event.Location,' in c and '@event.ImageUrl,' not in c:
        c = c.replace('@event.Location,', '@event.Location,\n            @event.ImageUrl,')
    
    return c

for file in [
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\CreateEvent\Handler.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\UpdateEvent\Handler.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetEvents\Handler.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetEventById\Handler.cs',
    r'c:\Users\install-admin\Desktop\EventPlanner\EventPlanner.Server\Features\Events\GetMapEvents\Handler.cs'
]:
    update_file(file, update_handler)

print("Done")
