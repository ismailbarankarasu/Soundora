using Microsoft.AspNetCore.Mvc;

namespace Soundora.Presentation.ViewComponents
{
    public class _LayoutHeaderComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
