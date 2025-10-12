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
            var solicitudDb = await _context.Solicitudes.FindAsync(Id);
            if (solicitudDb == null)
                return NotFound();

            solicitudDb.Estado = Estado;
            solicitudDb.Motivo = Motivo;

            await _context.SaveChangesAsync();

            // ?? Crear notificación para el usuario
            var notificacion = new Notificacion
            {
                UsuarioId = solicitudDb.UsuarioId,
                Titulo = $"Solicitud #{solicitudDb.Id} {solicitudDb.Estado}",
                Mensaje = $"Tu solicitud '{solicitudDb.Asunto}' fue {solicitudDb.Estado.ToLower()} por el administrador. Motivo: {solicitudDb.Motivo}",
                Tipo = "Solicitud",
                FechaCreacion = DateTime.Now
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();


            TempData["Mensaje"] = $"Solicitud #{Id} actualizada correctamente.";
            return RedirectToPage("Index");
        }
    }
}
