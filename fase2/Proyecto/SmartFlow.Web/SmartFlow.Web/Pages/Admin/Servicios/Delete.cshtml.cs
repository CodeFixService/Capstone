using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Servicios
{
    public class DeleteModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DeleteModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Servicio Servicio { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio = await _context.Servicios
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
                return NotFound();

            Servicio = servicio;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio = await _context.Servicios.FindAsync(id);

            if (servicio == null)
                return NotFound();

            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
