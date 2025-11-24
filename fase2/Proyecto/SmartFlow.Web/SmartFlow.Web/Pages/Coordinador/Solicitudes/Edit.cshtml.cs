using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Coordinador.Solicitudes
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Solicitud Solicitud { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null)
                return RedirectToPage("/Login/Login");

            // 🔹 Obtener carrera del coordinador
            var carreraCoord = await _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefaultAsync();

            if (carreraCoord == null)
                return NotFound("El coordinador no tiene una carrera asignada.");

            // 🔹 Buscar solicitud de su carrera
            Solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .FirstOrDefaultAsync(s => s.Id == id && s.Usuario.CarreraId == carreraCoord);

            if (Solicitud == null)
                return NotFound("No se encontró la solicitud o no pertenece a su carrera.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null)
                return RedirectToPage("/Login/Login");

            // 🔹 Obtener carrera del coordinador
            var carreraCoord = await _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefaultAsync();

            if (carreraCoord == null)
                return NotFound("El coordinador no tiene una carrera asignada.");

            // 🔹 Buscar solicitud válida
            var solicitudDb = await _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .FirstOrDefaultAsync(s => s.Id == id && s.Usuario.CarreraId == carreraCoord);

            if (solicitudDb == null)
                return NotFound("No se encontró la solicitud o no pertenece a su carrera.");

            // 🔹 Actualizar datos
            solicitudDb.Estado = Solicitud.Estado;
            solicitudDb.Motivo = Solicitud.Motivo?.Trim();

            await _context.SaveChangesAsync();

            // 🔔 Crear notificación al usuario
            string mensaje = $"Tu solicitud \"{solicitudDb.Asunto}\" fue {solicitudDb.Estado.ToLower()}.";
            if (!string.IsNullOrWhiteSpace(solicitudDb.Motivo))
                mensaje += $"\nComentario del coordinador: {solicitudDb.Motivo}";

            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = solicitudDb.UsuarioId,
                Titulo = $"Solicitud {solicitudDb.Estado}",
                Mensaje = mensaje,
                Tipo = solicitudDb.Estado == "Aprobada" ? "Info" : "Alerta",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            ///  Registrar acción en bitácora (adaptado al modelo actual)
            _context.Bitacoras.Add(new Bitacora
            {
                Usuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Coordinador",
                Accion = "Actualización de Solicitud",
                Modulo = "Solicitudes",
                Detalle = $"El coordinador cambió la solicitud #{solicitudDb.Id} al estado: {solicitudDb.Estado}.",
                Fecha = DateTime.Now
            });



            await _context.SaveChangesAsync();

            TempData["Mensaje"] = " Solicitud actualizada correctamente.";
            return RedirectToPage("./Index");
        }
    }
}
