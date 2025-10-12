using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class DeleteModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DeleteModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SmartFlow.Web.Models.Usuario Usuario { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            Usuario = usuario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            // Eliminación física actual (la cambiamos a lógica más adelante)
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
