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

        // Cargar lista de servicios (combo del modal)
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
                ComentarioAdmin = "",
                FechaCreacion = DateTime.Now
            };

            _context.Reservas.Add(nueva);
            _context.SaveChanges();

            //  Notificar 
            //  Obtener estudiante con su carrera
            var estudiante = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId.Value);

            //  Obtener coordinador de su carrera
            var coordinador = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Coordinador" && u.CarreraId == estudiante.CarreraId);

            //  Obtener admin de su carrera
            var adminCarrera = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == estudiante.CarreraId);

            //  Obtener admin general (CarreraId == null)
            var adminGeneral = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == null);



            //  Notificar Coordinador
            if (coordinador != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = coordinador.Id,
                    Titulo = "Nueva reserva creada",
                    Mensaje = $"{estudiante.Nombre} reservó el servicio '{servicio.Nombre}' para el {request.FechaInicio:g}.",
                    Tipo = "Reserva",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            // 🔹 Notificar Admin de la carrera
            if (adminCarrera != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminCarrera.Id,
                    Titulo = "Nueva reserva en tu carrera",
                    Mensaje = $"{estudiante.Nombre} reservó el servicio '{servicio.Nombre}'.",
                    Tipo = "Reserva",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            // 🔹 Notificar Admin general (si existe)
            if (adminGeneral != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminGeneral.Id,
                    Titulo = "Nueva reserva registrada",
                    Mensaje = $"{estudiante.Nombre} creó una reserva.",
                    Tipo = "Reserva",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.SaveChanges();


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

            // Mostrar solo las reservas del usuario
            // NO mostrar "Ocupado"
            // No bloquear nada del calendario
            var eventos = reservas
                .Where(r => r.UsuarioId == usuarioId)  // ⬅ SOLO LAS DEL USUARIO
                .Select(r => new
                {
                    id = r.Id,
                    title = $"{r.Servicio?.Nombre} ({r.Estado})",
                    start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),

                    // 🎨 Colores por estado
                    color = r.Estado == "Aprobada" ? "#198754"
                         : r.Estado == "Rechazada" ? "#dc3545"
                         : "#ffc107",

                    textColor = "#000000",
                    rendering = "auto",  // ⬅ IMPORTANTE PARA QUE NO ROMPA LA SELECCIÓN

                    estado = r.Estado,
                    comentarioAdmin = r.ComentarioAdmin,
                    comentarioUsuario = r.ComentarioUsuario
                })
                .ToList();

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
