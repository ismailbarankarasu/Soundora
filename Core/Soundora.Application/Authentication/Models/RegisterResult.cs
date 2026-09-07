namespace Soundora.Application.Authentication.Models;

public sealed class RegisterResult
{
    public bool Succeeded { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; }
        = Array.Empty<string>();

    public static RegisterResult Success()
    {
        return new RegisterResult
        {
            Succeeded = true
        };
    }

    public static RegisterResult Failure(
        IEnumerable<string> errors)
    {
        return new RegisterResult
        {
            Errors = errors.ToArray()
        };
    }
}