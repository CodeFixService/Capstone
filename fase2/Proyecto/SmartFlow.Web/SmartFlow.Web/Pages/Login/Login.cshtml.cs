using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using Microsoft.AspNetCore.Http;
using SmartFlow.Web.Models;
using System.Linq;

namespace SmartFlow.Web.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public LoginModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Correo { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Mensaje { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == Correo && u.Password == Password);

            if (usuario != null)
            {
                HttpContext.Session.SetString("UsuarioCorreo", usuario.Correo);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);


                HttpContext.Session.SetString("Rol", usuario.Rol);


                if (usuario.Rol == "Admin")
                    return RedirectToPage("/Admin/Index"); // lo crearemos después
                else
                    return RedirectToPage("/Usuario/Index"); // lo crearemos después
            }

            Mensaje = "Correo o contraseña incorrectos.";
            return Page();
        }
    }
}
