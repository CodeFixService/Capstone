using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlow.Web.Pages.Admin.Calendario
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public List<Servicio> Servicios { get; set; } = new();

        public IndexModel(SmartFlowContext context) => _context = context;

        public IActionResult OnGet()
        {
            Servicios = _context.Servicios
            .OrderBy(s => s.Nombre)
            .ToList();

            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin"  && rol != "Coordinador") return RedirectToPage("/Login/Login");
            return Page();
        }

        public JsonResult OnGetEventos(string estado, string servicioId)
        {
            // 🟢 1. Obtener el usuario actual desde la sesión
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                // Si no hay sesión, no devolvemos nada
                return new JsonResult(Enumerable.Empty<object>());
            }

            var usuarioActual = _context.Usuarios
                .Include(u => u.Carrera)
                .FirstOrDefault(u => u.Id == usuarioId.Value);

            if (usuarioActual == null)
            {
                return new JsonResult(Enumerable.Empty<object>());
            }

            //  Construir la consulta base
            var query = _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                    .ThenInclude(u => u.Carrera)
                .AsQueryable();

           
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

        
            if (!string.IsNullOrEmpty(servicioId))
            {
                if (int.TryParse(servicioId, out int sid))
                {
                    query = query.Where(r => r.ServicioId == sid);
                }
            }

            // Filtro por rol / carrera

            // Admin
            if (usuarioActual.Rol == "Admin")
            {
                if (usuarioActual.CarreraId != null)
                {
                    // Admin de carrera → solo reservas de estudiantes de su carrera
                    query = query.Where(r => r.Usuario.CarreraId == usuarioActual.CarreraId);
                }
                // Admin general (CarreraId == null) → NO filtramos, ve todo
            }
            // Coordinador o Director → solo su carrera
            else if (usuarioActual.Rol == "Coordinador" || usuarioActual.Rol == "Director")
            {
                if (usuarioActual.CarreraId != null)
                {
                    query = query.Where(r => r.Usuario.CarreraId == usuarioActual.CarreraId);
                }
            }

            //  Armar los eventos para el calendario
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
                    comentarioUsuario = r.ComentarioUsuario,
                    comentarioAdmin = r.ComentarioAdmin,
                })
                .ToList();

            return new JsonResult(eventos);
        }


        //  Actualizar estado de una reserva
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

                // Obtener estudiante
               
                var estudiante = reserva.Usuario;

                // Obtener actores según carrera
                var coordinador = _context.Usuarios
                    .FirstOrDefault(u => u.Rol == "Coordinador" && u.CarreraId == estudiante.CarreraId);

                var adminCarrera = _context.Usuarios
                    .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == estudiante.CarreraId);

                var adminGeneral = _context.Usuarios
                    .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == null);


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
                //  Coordinador
                if (coordinador != null)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = coordinador.Id,
                        Titulo = $"Reserva #{reserva.Id} actualizada",
                        Mensaje = $"La reserva de {estudiante.Nombre} para {reserva.Servicio.Nombre} el {reserva.FechaInicio:g} fue {estado.ToLower()}.",
                        Tipo = "Reserva",
                        FechaCreacion = DateTime.Now,
                        Leida = false
                    });
                }

                //  Admin de carrera
                if (adminCarrera != null)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = adminCarrera.Id,
                        Titulo = $"Reserva en tu carrera",
                        Mensaje = $"La reserva #{reserva.Id} de {estudiante.Nombre} fue {estado.ToLower()}.",
                        Tipo = "Reserva",
                        FechaCreacion = DateTime.Now,
                        Leida = false
                    });
                }

                //  Admin general
                if (adminGeneral != null)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = adminGeneral.Id,
                        Titulo = $"Actualización de reserva #{reserva.Id}",
                        Mensaje = $"{estudiante.Nombre} tiene una reserva {estado.ToLower()} en {reserva.Servicio.Nombre}.",
                        Tipo = "Reserva",
                        FechaCreacion = DateTime.Now,
                        Leida = false
                    });
                }


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
