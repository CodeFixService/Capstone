using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;
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
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == Correo);

            if (usuario == null || !PasswordHelper.VerifyPassword(Password, usuario.Password))
            {
                Mensaje = "⚠️ Usuario o contraseña incorrectos.";
                return Page();
            }

            if (usuario != null)
            {
                HttpContext.Session.SetString("UsuarioCorreo", usuario.Correo);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);


                HttpContext.Session.SetString("Rol", usuario.Rol);


                return usuario.Rol switch
                {
                    "Admin" => RedirectToPage("/Admin/Index"),
                    "Director" => RedirectToPage("/Director/Index"),
                    "Coordinador" => RedirectToPage("/Coordinador/Index"),
                    _ => RedirectToPage("/Usuario/Index")
                };

            }

            Mensaje = "Correo o contraseña incorrectos.";
            return Page();
        }
    }
}
