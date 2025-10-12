using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Aranceles
{
    public class DetailsModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DetailsModel(SmartFlowContext context)
        {
            _context = context;
        }

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
    }
}
