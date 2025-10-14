using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;              // tu DbContext
using SmartFlow.Web.Models;            // Notificacion, Usuario
using System;
using System.Collections.Generic;
using System.Linq;



namespace SmartFlow.Web.Pages.Admin.Notificaciones
{
    public class NuevaModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public NuevaModel(SmartFlowContext context) => _context = context;

        // Campos del formulario
        [BindProperty] public int? UsuarioId { get; set; }
        [BindProperty] public bool EnviarATodos { get; set; }
        [BindProperty] public string Titulo { get; set; } = "";
        [BindProperty] public string Mensaje { get; set; } = "";

        // Para llenar el combo
        public List<SmartFlow.Web.Models.Usuario> Usuarios { get; set; } = new();

        // Mensaje informativo post-envío
        public string MensajeSistema { get; set; } = "";

        public IActionResult OnGet()
        {
            // Guard de rol (simple con sesión)
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin") return RedirectToPage("/Login/Login");

            Usuarios = _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ToList();

            return Page();
        }

        public IActionResult OnPost()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin") return RedirectToPage("/Login/Login");

            if (string.IsNullOrWhiteSpace(Titulo) || string.IsNullOrWhiteSpace(Mensaje))
            {
                MensajeSistema = "Debe ingresar Título y Mensaje.";
                OnGet(); // recargar combo
                return Page();
            }

            var ahora = DateTime.Now;

            if (EnviarATodos)
            {
                var ids = _context.Usuarios.Select(u => u.Id).ToList();
                foreach (var id in ids)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = id,
                        Titulo = Titulo,
                        Mensaje = Mensaje,
                        Tipo = "Info",
                        Leida = false,
                        FechaCreacion = ahora
                    });
                }
            }
            else
            {
                if (UsuarioId is null || UsuarioId <= 0)
                {
                    MensajeSistema = "Seleccione un usuario o marque 'Enviar a todos'.";
                    OnGet();
                    return Page();
                }

                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = UsuarioId.Value,
                    Titulo = Titulo,
                    Mensaje = Mensaje,
                    Tipo = "Info",
                    Leida = false,
                    FechaCreacion = ahora
                });
            }

            _context.SaveChanges();

            // Recargar combo y limpiar
            Usuarios = _context.Usuarios.OrderBy(u => u.Nombre).ToList();
            UsuarioId = null; EnviarATodos = false; Titulo = ""; Mensaje = "";
            MensajeSistema = "? Notificación enviada correctamente.";
            return Page();
        }
    }
}
