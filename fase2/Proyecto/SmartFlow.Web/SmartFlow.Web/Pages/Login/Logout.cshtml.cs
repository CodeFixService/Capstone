using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Pages.Login
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear(); // Limpia toda la sesión
            return RedirectToPage("/Login/Login");
        }
    }
}
