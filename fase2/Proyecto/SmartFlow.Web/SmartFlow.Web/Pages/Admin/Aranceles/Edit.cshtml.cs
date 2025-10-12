using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Aranceles
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Arancel Arancel { get; set; } = new Arancel();

        public SelectList CarrerasSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var arancel = await _context.Aranceles
                .Include(a => a.Carrera)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (arancel == null)
                return NotFound();

            Arancel = arancel;

            CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Arancel.CarreraId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Arancel.CarreraId);
                return Page();
            }

            _context.Attach(Arancel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Aranceles.Any(a => a.Id == Arancel.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
