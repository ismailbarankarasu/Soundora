namespace Soundora.Application.Packages.Models;

public class UpdatePackageRequest : CreatePackageRequest
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }
}