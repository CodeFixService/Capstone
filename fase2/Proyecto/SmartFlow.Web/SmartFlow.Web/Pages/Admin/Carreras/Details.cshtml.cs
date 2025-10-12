using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Carreras
{
    public class DetailsModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DetailsModel(SmartFlowContext context)
        {
            _context = context;
        }

        public Carrera Carrera { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var carrera = await _context.Carreras
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (carrera == null)
                return NotFound();

            Carrera = carrera;
            return Page();
        }
    }
}
