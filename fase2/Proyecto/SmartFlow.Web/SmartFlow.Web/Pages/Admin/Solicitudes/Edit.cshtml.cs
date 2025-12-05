using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Solicitudes
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Estado { get; set; }

        [BindProperty]
        public string Motivo { get; set; }

        public Solicitud Solicitud { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Servicio)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (Solicitud == null)
                return NotFound();

            // precarga valores actuales
            Id = Solicitud.Id;
            Estado = Solicitud.Estado;
            Motivo = Solicitud.Motivo;

            return Page();
        }

     
        public async Task<IActionResult> OnPostAsync()
        {
            var solicitudDb = await _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .FirstOrDefaultAsync(s => s.Id == Id);

            if (solicitudDb == null)
                return NotFound();

            //  1. Actualizar datos
            solicitudDb.Estado = Estado;
            solicitudDb.Motivo = Motivo;
            await _context.SaveChangesAsync();

            //  2. Obtener estudiante
            var estudiante = solicitudDb.Usuario;

            //  3. Obtener Coordinador, Admin de carrera, Admin general
            var coordinador = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Coordinador" && u.CarreraId == estudiante.CarreraId);

            var adminCarrera = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == estudiante.CarreraId);

            var adminGeneral = _context.Usuarios
                .FirstOrDefault(u => u.Rol == "Admin" && u.CarreraId == null);

            // NOTIFICACIONES     

            // Estudiante
            _context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = estudiante.Id,
                Titulo = $"Solicitud #{solicitudDb.Id} {solicitudDb.Estado}",
                Mensaje = $"Tu solicitud '{solicitudDb.Asunto}' fue {solicitudDb.Estado.ToLower()}. Motivo: {solicitudDb.Motivo}",
                Tipo = "Solicitud",
                FechaCreacion = DateTime.Now
            });

            // Coordinador
            if (coordinador != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = coordinador.Id,
                    Titulo = $"Solicitud de {estudiante.Nombre} actualizada",
                    Mensaje = $"La solicitud #{solicitudDb.Id} fue {solicitudDb.Estado.ToLower()}.",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            // Admin de carrera (solo si NO es admin general quien opera)
            if (adminCarrera != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminCarrera.Id,
                    Titulo = $"Solicitud de {estudiante.Nombre} actualizada",
                    Mensaje = $"Estado: {solicitudDb.Estado}. Motivo: {solicitudDb.Motivo}",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            // 🟣dmin General (opcional, si quieres SIEMPRE notificarlos)
            if (adminGeneral != null)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    UsuarioId = adminGeneral.Id,
                    Titulo = $"Actualización en solicitud #{solicitudDb.Id}",
                    Mensaje = $"{estudiante.Nombre} tiene una solicitud {solicitudDb.Estado.ToLower()}.",
                    Tipo = "Solicitud",
                    FechaCreacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Solicitud #{Id} actualizada correctamente.";
            return RedirectToPage("Index");
        }

    }
}
