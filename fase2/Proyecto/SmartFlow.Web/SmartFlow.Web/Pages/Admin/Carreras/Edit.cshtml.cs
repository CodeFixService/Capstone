using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SmartFlow.Web.Pages.Admin.Carreras
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Carrera Carrera { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Carrera = await _context.Carreras.FirstOrDefaultAsync(c => c.Id == id);

            if (Carrera == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Carrera).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Carreras.Any(c => c.Id == Carrera.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
