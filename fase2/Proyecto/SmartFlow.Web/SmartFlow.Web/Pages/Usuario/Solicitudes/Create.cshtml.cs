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

            // ==============================
            //  Obtener usuario actual
            // ==============================
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var estudiante = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);

            // ==============================
            //  Obtener personas involucradas
            // ==============================

            // Coordinador de la misma carrera
            var coordinador = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Coordinador" && u.CarreraId == estudiante.CarreraId);

            // Admin de la misma carrera
            var adminCarrera = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == estudiante.CarreraId);

            // Admin general (sin carrera asociada)
            var adminGeneral = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == null);

            // ==============================
            //  Notificar al Coordinador
            // ==============================
            if (coordinador != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = coordinador.Id,
                    Titulo = "Nueva solicitud registrada",
                    Mensaje = $"{estudiante.Nombre} creó una solicitud: '{Solicitud.Asunto}'",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            // ==============================
            //  Notificar Admin de la carrera
            // ==============================
            if (adminCarrera != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminCarrera.Id,
                    Titulo = "Solicitud en tu carrera",
                    Mensaje = $"{estudiante.Nombre} creó una solicitud.",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            // ==============================
            //  Notificar Admin general
            // ==============================
            if (adminGeneral != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminGeneral.Id,
                    Titulo = "Nueva solicitud registrada",
                    Mensaje = $"{estudiante.Nombre} creó una solicitud.",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();


            Solicitud.UsuarioId = usuarioId.Value;
            Solicitud.FechaCreacion = DateTime.Now;
            Solicitud.Estado = "Pendiente";

            _context.Solicitudes.Add(Solicitud);
            await _context.SaveChangesAsync();
           
            // Redirige al listado del usuario
            return RedirectToPage("/Usuario/Solicitudes/Index");
        }
    }
}
