using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Pages.Usuario
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rol) || rol != "Usuario")
                return RedirectToPage("/Login/Login");

            return Page();
        }
    }
}
