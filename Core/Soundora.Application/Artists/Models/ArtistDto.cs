namespace Soundora.Application.Artists.Models;

public class ArtistDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}