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
            if (rol != "Usuario") return RedirectToPage("/Login/Login");
            return Page();
        }

        // 🟢 CARGAR LISTA DE SERVICIOS (para llenar el <select>)
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

        // 🟢 CREAR UNA NUEVA RESERVA
        public JsonResult OnPostCrear(int servicioId, DateTime fechaInicio, DateTime fechaFin, string comentario)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { success = false, message = "Usuario no válido" });

            var servicio = _context.Servicios.FirstOrDefault(s => s.Id == servicioId);
            if (servicio == null)
                return new JsonResult(new { success = false, message = "Servicio no encontrado" });

            // Validar solapamiento de horarios
            bool existe = _context.Reservas.Any(r =>
                r.ServicioId == servicioId &&
                r.Estado == "Aprobada" &&
                ((fechaInicio >= r.FechaInicio && fechaInicio < r.FechaFin) ||
                 (fechaFin > r.FechaInicio && fechaFin <= r.FechaFin))
            );

            if (existe)
                return new JsonResult(new { success = false, message = "Ese horario ya está ocupado." });

            var nueva = new Reserva
            {
                UsuarioId = usuarioId.Value,
                ServicioId = servicioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Estado = "Pendiente",
                ComentarioUsuario = string.IsNullOrWhiteSpace(comentario) ? null : comentario,
                FechaCreacion = DateTime.Now
            };

            _context.Reservas.Add(nueva);
            _context.SaveChanges();

            // Notificar a administradores
            var admins = _context.Usuarios.Where(u => u.Rol == "Admin").ToList();
            foreach (var admin in admins)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = admin.Id,
                    Titulo = "Nueva reserva creada",
                    Mensaje = $"Un usuario reservó para el {fechaInicio:g} - Servicio: {servicio.Nombre}.",
                    Tipo = "Reserva",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }

        // 🟢 CARGAR EVENTOS EN EL CALENDARIO
        public JsonResult OnGetEventos()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(Enumerable.Empty<object>());

            // 🔹 Cargar todas las reservas (no solo las del usuario)
            var reservas = _context.Reservas
                .Include(r => r.Servicio)
                .ToList();

            var eventos = reservas.Select(r => new
            {
                id = r.Id,
                // 🔸 Si la reserva es del usuario actual, muestra su servicio y estado
                // 🔸 Si la reserva es de otro usuario, muestra "Ocupado"
                title = r.UsuarioId == usuarioId
                    ? $"{r.Servicio?.Nombre} ({r.Estado})"
                    : "Ocupado",
                start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = r.UsuarioId == usuarioId
                    ? (r.Estado == "Aprobada" ? "#198754" :
                       r.Estado == "Rechazada" ? "#dc3545" : "#ffc107")
                    : "#6c757d", // 🔹 gris para reservas de otros usuarios
                servicio = r.Servicio?.Nombre,
                comentarioAdmin = r.ComentarioAdmin
            });

            return new JsonResult(eventos);
        }


    }
}
