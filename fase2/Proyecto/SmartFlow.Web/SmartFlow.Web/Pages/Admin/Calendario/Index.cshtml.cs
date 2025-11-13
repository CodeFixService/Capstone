using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Collections.Generic;
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
            if (rol != "Admin" && rol != "Director" && rol != "Coordinador") return RedirectToPage("/Login/Login");
            return Page();
        }

        // 🔹 Cargar eventos del calendario global (Admin)
        public JsonResult OnGetEventos(int? servicioId)
        {
            var query = _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                .AsQueryable();

            if (servicioId.HasValue)
                query = query.Where(r => r.ServicioId == servicioId.Value);

            var eventos = query
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
                    estado = r.Estado,
                    detalle = $"Estudiante: {r.Usuario.Nombre}\nServicio: {r.Servicio.Nombre}\nInicio: {r.FechaInicio:g}\nFin: {r.FechaFin:g}\nEstado: {r.Estado}"
                })
                .ToList();

            return new JsonResult(eventos);
        }

        // 🔹 Actualizar estado de una reserva
        [IgnoreAntiforgeryToken]
        public JsonResult OnPostActualizarEstado([FromForm] int id, [FromForm] string estado, [FromForm] string? comentarioAdmin)
        {
            try
            {
                var reserva = _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Servicio)
                    .FirstOrDefault(r => r.Id == id);

                if (reserva == null)
                    return new JsonResult(new { success = false, message = "Reserva no encontrada" });

                //  Actualizar estado y comentario
                reserva.Estado = estado;
                reserva.ComentarioAdmin = string.IsNullOrWhiteSpace(comentarioAdmin) ? null : comentarioAdmin;
                _context.SaveChanges();

                //  Crear notificación al usuario
                string mensaje = $"La reserva para {reserva.Servicio.Nombre} el {reserva.FechaInicio:g} fue {estado.ToLower()}.";
                if (!string.IsNullOrWhiteSpace(comentarioAdmin))
                    mensaje += $"\nComentario del administrador: {comentarioAdmin}";

                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = reserva.UsuarioId,
                    Titulo = $"Tu reserva fue {estado.ToLower()}",
                    Mensaje = mensaje,
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
