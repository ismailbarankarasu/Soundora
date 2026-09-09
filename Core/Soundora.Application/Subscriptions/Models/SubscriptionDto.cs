namespace Soundora.Application.Subscriptions.Models;

public class SubscriptionDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public string Status { get; init; } = string.Empty;
}