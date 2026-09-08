namespace Soundora.Application.Subscriptions.Models;

public class SubscriptionPackageDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int DurationInDays { get; init; }
}