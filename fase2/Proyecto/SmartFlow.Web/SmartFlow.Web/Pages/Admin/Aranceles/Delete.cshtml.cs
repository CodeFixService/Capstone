using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Aranceles
{
    public class DeleteModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DeleteModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Arancel Arancel { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var arancel = await _context.Aranceles
                .Include(a => a.Carrera)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (arancel == null)
                return NotFound();

            Arancel = arancel;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var arancel = await _context.Aranceles.FindAsync(id);

            if (arancel == null)
                return NotFound();

            _context.Aranceles.Remove(arancel);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
