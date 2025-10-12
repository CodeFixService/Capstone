using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Servicios
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Servicio Servicio { get; set; } = new Servicio();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
                return NotFound();

            Servicio = servicio;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Servicio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Servicios.Any(s => s.Id == Servicio.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
