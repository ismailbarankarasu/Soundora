using Soundora.Domain.Enums;

namespace Soundora.Application.Packages.Models;

public class PackageDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public int DurationInDays { get; init; }

    public AccessLevel AccessLevel { get; init; }

    public bool IsActive { get; init; }
}