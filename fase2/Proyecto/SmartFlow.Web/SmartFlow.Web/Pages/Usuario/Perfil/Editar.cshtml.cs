using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartFlow.Web.Pages.Usuario.Perfil
{
    public class EditarModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public EditarModel(SmartFlowContext context) => _context = context;

        [BindProperty] public PerfilInput Input { get; set; } = new();
        public string Mensaje { get; set; } = "";

        public class PerfilInput
        {
            [Required, StringLength(120)]
            public string Nombre { get; set; } = "";

            [Required, EmailAddress, StringLength(150)]
            public string Correo { get; set; } = "";

            // Passwords (opcionales en este paso)
            public string? PassActual { get; set; }
            public string? PassNueva { get; set; }
            public string? PassConfirm { get; set; }
        }

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Usuario") return RedirectToPage("/Login/Login");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToPage("/Login/Login");

            var u = _context.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
            if (u == null) return RedirectToPage("/Login/Login");

            Input.Nombre = u.Nombre;
            Input.Correo = u.Correo;
            return Page();
        }

        public IActionResult OnPost()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Usuario") return RedirectToPage("/Login/Login");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToPage("/Login/Login");

            if (!ModelState.IsValid) return Page();

            var u = _context.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
            if (u == null) return RedirectToPage("/Login/Login");

            // Actualiza nombre/correo
            u.Nombre = Input.Nombre.Trim();
            u.Correo = Input.Correo.Trim();

            // Cambio de contraseña (opcional)
            if (!string.IsNullOrWhiteSpace(Input.PassNueva) ||
                !string.IsNullOrWhiteSpace(Input.PassConfirm) ||
                !string.IsNullOrWhiteSpace(Input.PassActual))
            {
                // Validar campos
                if (string.IsNullOrWhiteSpace(Input.PassActual) ||
                    string.IsNullOrWhiteSpace(Input.PassNueva) ||
                    string.IsNullOrWhiteSpace(Input.PassConfirm))
                {
                    ModelState.AddModelError(string.Empty, "Para cambiar la contraseña completa los tres campos.");
                    return Page();
                }

                if (Input.PassNueva != Input.PassConfirm)
                {
                    ModelState.AddModelError(string.Empty, "La nueva contraseña y su confirmación no coinciden.");
                    return Page();
                }

                // POR AHORA: comparación directa.
                // (en la etapa final ciframos y comparamos hash)
                if (u.Password != Input.PassActual)
                {
                    ModelState.AddModelError(string.Empty, "La contraseña actual no es correcta.");
                    return Page();
                }

                u.Password = Input.PassNueva; // luego se reemplaza por hash
            }

            _context.SaveChanges();

            // Actualiza nombre en sesión para el saludo/campanita
            HttpContext.Session.SetString("UsuarioNombre", u.Nombre);

            Mensaje = "✅ Cambios guardados correctamente.";
            return Page();
        }
    }
}
