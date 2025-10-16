using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using System.Linq;

namespace SmartFlow.Web.Pages.Usuario.Calendario
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public IndexModel(SmartFlowContext context) => _context = context;

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Usuario") return RedirectToPage("/Login/Login");
            return Page();
        }
        public JsonResult OnPostCrear(string servicio, DateTime fechaInicio, DateTime fechaFin)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { success = false, message = "Usuario no válido" });

            if (string.IsNullOrWhiteSpace(servicio))
                return new JsonResult(new { success = false, message = "Servicio requerido" });

            var nueva = new Models.Reserva
            {
                UsuarioId = usuarioId.Value,
                Servicio = servicio,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            _context.Reservas.Add(nueva);
            _context.SaveChanges();

            // 🔔 Notificación a todos los administradores
            var admins = _context.Usuarios.Where(u => u.Rol == "Admin").ToList();
            foreach (var admin in admins)
            {
                _context.Notificaciones.Add(new Models.Notificacion
                {
                    UsuarioId = admin.Id,
                    Titulo = "Nueva reserva creada",
                    Mensaje = $"Un estudiante realizó una reserva para el {fechaInicio:g} - Servicio: {servicio}.",
                    Tipo = "Alerta",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }
            _context.SaveChanges();



            return new JsonResult(new { success = true });
        }


        // 🔹 Handler que FullCalendar usa para cargar los eventos
        public JsonResult OnGetEventos()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(Enumerable.Empty<object>());

            var reservas = _context.Reservas
                .Select(r => new
                {
                    title = r.UsuarioId == usuarioId ? $"{r.Servicio} ({r.Estado})" : "Ocupado",
                    start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = r.UsuarioId == usuarioId
                        ? (r.Estado == "Aprobada" ? "#198754" :
                           r.Estado == "Rechazada" ? "#dc3545" : "#ffc107")
                        : "#6c757d" // gris si no es suya
                })
                .ToList();

            return new JsonResult(reservas);
        }



    }
}
