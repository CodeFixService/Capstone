using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Roles
{
    public class DetailsModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DetailsModel(SmartFlowContext context)
        {
            _context = context;
        }

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
    }
}
