using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Helpers;

namespace SmartFlow.Web.Pages.Coordinador
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            var sesion = HttpContext.Session;
            if (!AccesoHelper.TieneSesion(sesion) || !AccesoHelper.EsCoordinador(sesion))
            {
                Response.Redirect("/Login/Login");
            }
        }
    }
}
