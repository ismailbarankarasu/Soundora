using Microsoft.AspNetCore.Mvc;

namespace Soundora.Presentation.ViewComponents
{
    public class _LayoutScriptComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
