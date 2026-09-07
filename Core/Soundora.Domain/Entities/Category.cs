using Soundora.Domain.Common;

namespace Soundora.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<AudioContent> AudioContents { get; set; }
        = new List<AudioContent>();
}