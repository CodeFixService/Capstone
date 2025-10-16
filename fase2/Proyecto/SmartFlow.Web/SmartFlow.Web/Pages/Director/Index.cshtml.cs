using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Helpers;

namespace SmartFlow.Web.Pages.Director
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            var sesion = HttpContext.Session;
            if (!AccesoHelper.TieneSesion(sesion) || !AccesoHelper.EsDirector(sesion))
            {
                Response.Redirect("/Login/Login");
            }
        }
    }
}
