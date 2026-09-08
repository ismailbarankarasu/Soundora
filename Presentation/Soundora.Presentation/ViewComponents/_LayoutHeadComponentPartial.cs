using Microsoft.AspNetCore.Mvc;

namespace Soundora.Presentation.ViewComponents
{
    public class _LayoutHeadComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
