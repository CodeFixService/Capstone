using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using System.Linq;

namespace SmartFlow.Web.Pages.Admin.Calendario
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public List<string> Servicios { get; set; } = new();

        public IndexModel(SmartFlowContext context) => _context = context;

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin") return RedirectToPage("/Login/Login");
            return Page();
        }

        // Handler que el calendario usa para cargar eventos
        public JsonResult OnGetEventos(string? servicio)
        {
            var query = _context.Reservas.AsQueryable();

            if (!string.IsNullOrEmpty(servicio))
                query = query.Where(r => r.Servicio == servicio);

            var eventos = query
                .Join(_context.Usuarios,
                      r => r.UsuarioId,
                      u => u.Id,
                      (r, u) => new
                      {
                          id = r.Id,
                          title = $"{u.Nombre} - {r.Servicio} ({r.Estado})",
                          start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                          end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                          color = r.Estado == "Aprobada" ? "#198754"
                                : r.Estado == "Rechazada" ? "#dc3545"
                                : "#ffc107",
                          usuario = u.Nombre,
                          servicio = r.Servicio,
                          estado = r.Estado,
                          detalle = $"Estudiante: {u.Nombre}\nServicio: {r.Servicio}\nInicio: {r.FechaInicio:g}\nFin: {r.FechaFin:g}\nEstado: {r.Estado}"
                      })
                .ToList();

            return new JsonResult(eventos);
        }

        [IgnoreAntiforgeryToken]
        public JsonResult OnPostActualizarEstado([FromForm] int id, [FromForm] string estado)
        {
            try
            {
                var reserva = _context.Reservas.FirstOrDefault(r => r.Id == id);
                if (reserva == null)
                    return new JsonResult(new { success = false, message = "Reserva no encontrada" });

                reserva.Estado = estado;
                _context.SaveChanges();

                // 🔔 Notificación al estudiante
                _context.Notificaciones.Add(new Models.Notificacion
                {
                    UsuarioId = reserva.UsuarioId,
                    Titulo = $"Tu reserva fue {estado.ToLower()}",
                    Mensaje = $"La reserva para {reserva.Servicio} el {reserva.FechaInicio:g} fue {estado.ToLower()}.",
                    Tipo = estado == "Aprobada" ? "Info" : "Alerta",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });

                _context.SaveChanges();

                return new JsonResult(new
                {
                    success = true,
                    nuevoEstado = reserva.Estado,
                    id = reserva.Id
                });

            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }


    }
}
