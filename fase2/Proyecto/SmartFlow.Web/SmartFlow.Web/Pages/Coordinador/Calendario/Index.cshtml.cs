using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Linq;

namespace SmartFlow.Web.Pages.Coordinador.Calendario
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            return Page();
        }

        public JsonResult OnGetEventos()
        {
            // Obtener el ID del coordinador actual
            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null)
                return new JsonResult(new { success = false, message = "Sesión expirada" });

            // Obtener su carrera asignada
            var carreraCoord = _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefault();

            // Si no tiene carrera asignada, no mostrar nada
            if (carreraCoord == null)
                return new JsonResult(new { success = false, message = "El coordinador no tiene carrera asignada" });

            // Filtrar solo reservas de usuarios que pertenezcan a esa carrera
            var eventos = _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                .Where(r => r.Usuario.CarreraId == carreraCoord)
                .Select(r => new
                {
                    id = r.Id,
                    title = $"{r.Usuario.Nombre} - {r.Servicio.Nombre} ({r.Estado})",
                    start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = r.Estado == "Aprobada" ? "#198754"
                            : r.Estado == "Rechazada" ? "#dc3545"
                            : "#ffc107",
                    usuario = r.Usuario.Nombre,
                    servicio = r.Servicio.Nombre,
                    estado = r.Estado
                })
                .ToList();

            return new JsonResult(eventos);
        }


        [IgnoreAntiforgeryToken]
        public JsonResult OnPostActualizarEstado([FromForm] int id, [FromForm] string estado, [FromForm] string? comentarioCoord)
        {
            try
            {
                var reserva = _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Servicio)
                    .FirstOrDefault(r => r.Id == id);

                if (reserva == null)
                    return new JsonResult(new { success = false, message = "Reserva no encontrada" });

                // Actualizar estado y comentario
                reserva.Estado = estado;
                reserva.ComentarioAdmin = string.IsNullOrWhiteSpace(comentarioCoord) ? null : comentarioCoord;
                _context.SaveChanges();

                // Notificar al usuario
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = reserva.UsuarioId,
                    Titulo = $"Tu reserva fue {estado.ToLower()}",
                    Mensaje = $"El coordinador ha {estado.ToLower()} tu reserva para {reserva.Servicio.Nombre}.",
                    Tipo = estado == "Aprobada" ? "Info" : "Alerta",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
                _context.SaveChanges();
                // 🧾 Registrar acción en bitácora
                _context.Bitacoras.Add(new Bitacora
                {
                    Usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Coordinador",
                    Accion = "Cambio de estado de reserva",
                    Modulo = "Calendario - Coordinador",
                    Detalle = $"El coordinador cambió la reserva #{reserva.Id} a estado: {reserva.Estado}.",
                    Fecha = DateTime.Now
                });
                _context.SaveChanges();


                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
