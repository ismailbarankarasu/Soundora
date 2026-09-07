using Soundora.Domain.Common;
using Soundora.Domain.Enums;

namespace Soundora.Domain.Entities;

public class AudioContent : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }

    public int DurationInSeconds { get; set; }

    public ContentType ContentType { get; set; }

    public AccessLevel RequiredAccessLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public Guid? ArtistId { get; set; }

    public Artist? Artist { get; set; }
}