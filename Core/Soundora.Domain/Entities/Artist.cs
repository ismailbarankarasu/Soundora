using Soundora.Domain.Common;

namespace Soundora.Domain.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AudioContent> AudioContents { get; set; }
        = new List<AudioContent>();
}