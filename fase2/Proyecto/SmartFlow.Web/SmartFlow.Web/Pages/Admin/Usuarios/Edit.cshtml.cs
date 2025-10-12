using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SmartFlow.Web.Models.Usuario Usuario { get; set; } = new SmartFlow.Web.Models.Usuario();

        // ?? Listas desplegables dinámicas
        public SelectList RolesSelectList { get; set; }
        public SelectList CarrerasSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            Usuario = usuario;

            // ?? Cargar roles y carreras desde la base de datos
            RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre", Usuario.Rol);
            CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Usuario.CarreraId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // ?? Si falla validación, recargar listas para no perder selects
                RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre", Usuario.Rol);
                CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Usuario.CarreraId);
                return Page();
            }

            // Mantener los campos de auditoría
            var original = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == Usuario.Id);

            if (original == null)
                return NotFound();

            Usuario.CreadoPor = original?.CreadoPor;
            Usuario.FechaCreacion = original?.FechaCreacion;

            _context.Attach(Usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(u => u.Id == Usuario.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
