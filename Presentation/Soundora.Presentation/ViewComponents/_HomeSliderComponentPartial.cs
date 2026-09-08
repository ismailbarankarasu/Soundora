using Microsoft.AspNetCore.Mvc;

namespace Soundora.Presentation.ViewComponents
{
    public class _HomeSliderComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
