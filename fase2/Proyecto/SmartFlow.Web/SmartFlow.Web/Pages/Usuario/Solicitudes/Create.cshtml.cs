using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Usuario.Solicitudes
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Solicitud Solicitud { get; set; } = new Solicitud();

        public SelectList ServiciosSelectList { get; set; }

        public void OnGet()
        {
            // Cargar los servicios disponibles
            ServiciosSelectList = new SelectList(_context.Servicios
                .Where(s => s.Activo)
                .ToList(), "Id", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ServiciosSelectList = new SelectList(_context.Servicios
                    .Where(s => s.Activo)
                    .ToList(), "Id", "Nombre");
                return Page();
            }

            // ?? Tomamos el ID del usuario actual desde la sesión
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                // Si no hay sesión activa, redirige al login
                return RedirectToPage("/Login/Index");
            }

            Solicitud.UsuarioId = usuarioId.Value;
            Solicitud.FechaCreacion = DateTime.Now;
            Solicitud.Estado = "Pendiente";

            _context.Solicitudes.Add(Solicitud);
            await _context.SaveChangesAsync();
            // ?? Crear notificación para todos los administradores
            var admins = _context.Usuarios
                .Where(u => u.Rol == "Admin")
                .ToList();

            foreach (var admin in admins)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = admin.Id,
                    Titulo = $"Nueva solicitud de {HttpContext.Session.GetString("UsuarioNombre")}",
                    Mensaje = $"El usuario {HttpContext.Session.GetString("UsuarioNombre")} ha enviado una nueva solicitud: '{Solicitud.Asunto}'",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                };
                _context.Notificaciones.Add(notificacion);
            }

            await _context.SaveChangesAsync();


            // Redirige al listado del usuario
            return RedirectToPage("/Usuario/Solicitudes/Index");
        }
    }
}
