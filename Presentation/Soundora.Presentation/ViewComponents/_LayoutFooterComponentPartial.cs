using Microsoft.AspNetCore.Mvc;

namespace Soundora.Presentation.ViewComponents
{
    public class _LayoutFooterComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
