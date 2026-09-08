namespace Soundora.Application.Subscriptions.Models;

public class SubscriptionOperationResult
{
    public bool Succeeded { get; init; }

    public string? Error { get; init; }
}