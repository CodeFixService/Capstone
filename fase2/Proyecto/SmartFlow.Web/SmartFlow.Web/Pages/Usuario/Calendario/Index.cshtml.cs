using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            // 🔹 Verifica sesión activa y rol correcto
            if (usuarioId == null || string.IsNullOrEmpty(rol) || rol != "Usuario")
                return RedirectToPage("/Login/Index");

            return Page();
        }

        // 🟢 Cargar lista de servicios (combo del modal)
        public async Task<JsonResult> OnGetServicios()
        {
            try
            {
                var servicios = await _context.Servicios
                    .Select(s => new
                    {
                        id = s.Id,
                        nombre = s.Nombre
                    })
                    .ToListAsync();

                return new JsonResult(servicios);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = true, message = ex.Message });
            }
        }

        // 🟢 Crear nueva reserva
        public JsonResult OnPostCrear([FromBody] ReservaRequest request)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { success = false, message = "Sesión expirada. Inicia sesión nuevamente." });

            var servicio = _context.Servicios.FirstOrDefault(s => s.Id == request.ServicioId);
            if (servicio == null)
                return new JsonResult(new { success = false, message = "Servicio no encontrado." });

            // 🔹 Validar solapamiento de horarios
            bool existe = _context.Reservas.Any(r =>
                r.ServicioId == request.ServicioId &&
                ((request.FechaInicio >= r.FechaInicio && request.FechaInicio < r.FechaFin) ||
                 (request.FechaFin > r.FechaInicio && request.FechaFin <= r.FechaFin))
            );

            if (existe)
                return new JsonResult(new { success = false, message = "Ese horario ya está ocupado." });

            var nueva = new Reserva
            {
                UsuarioId = usuarioId.Value,
                ServicioId = request.ServicioId,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                Estado = "Pendiente",
                ComentarioUsuario = string.IsNullOrWhiteSpace(request.Comentario) ? null : request.Comentario,
                FechaCreacion = DateTime.Now
            };

            _context.Reservas.Add(nueva);
            _context.SaveChanges();

            // 🔹 Notificar a administradores
            var admins = _context.Usuarios.Where(u => u.Rol == "Admin").ToList();
            foreach (var admin in admins)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = admin.Id,
                    Titulo = "Nueva reserva creada",
                    Mensaje = $"Un usuario reservó el servicio '{servicio.Nombre}' para el {request.FechaInicio:g}.",
                    Tipo = "Reserva",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }

        // 🟢 Cargar eventos del calendario
        public async Task<IActionResult> OnPostVerificarDisponibilidadAsync([FromBody] Reserva reserva)
        {
            var ocupado = await _context.Reservas
                .AnyAsync(r =>
                    r.FechaInicio < reserva.FechaFin &&
                    r.FechaFin > reserva.FechaInicio);

            return new JsonResult(new { disponible = !ocupado });
        }

        public JsonResult OnGetEventos()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(Enumerable.Empty<object>());

            var reservas = _context.Reservas
                .Include(r => r.Servicio)
                .ToList();

            // 🔹 Armar lista de eventos con colores según propietario
            var eventos = reservas.Select(r => new
            {
                id = r.Id,
                title = r.UsuarioId == usuarioId
                    ? $"{r.Servicio?.Nombre} ({r.Estado})"
                    : "Ocupado",
                start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = r.UsuarioId == usuarioId
                    ? (r.Estado == "Aprobada" ? "#198754" :
                       r.Estado == "Rechazada" ? "#dc3545" : "#ffc107")
                    : "#b0b0b0", // gris para otros usuarios
                textColor = "#000000",
                overlap = true,
                rendering = r.UsuarioId == usuarioId ? "auto" : "background",
                estado = r.Estado
            });

            return new JsonResult(eventos);
        }
    }

    // 🔹 Modelo auxiliar para recibir datos del frontend
    public class ReservaRequest
    {
        public int ServicioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Comentario { get; set; }
    }
}
