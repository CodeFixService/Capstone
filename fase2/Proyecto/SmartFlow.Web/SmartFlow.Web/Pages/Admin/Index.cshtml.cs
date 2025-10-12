using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rol) || rol != "Admin")
                return RedirectToPage("/Login/Login");

            return Page();
        }
    }
}
