using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Roles
{
    public class DeleteModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DeleteModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Rol Rol { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rol == null)
                return NotFound();

            Rol = rol;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
                return NotFound();

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
