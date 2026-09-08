using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Subscriptions.Models;

namespace Soundora.Presentation.Models.Subscriptions;

public class AssignPackageViewModel
{
    public AssignPackageRequest Input { get; set; } = new();

    public List<SelectListItem> Users { get; set; } = new();

    public List<SelectListItem> Packages { get; set; } = new();
}