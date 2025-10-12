using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Servicios
{
    public class DetailsModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DetailsModel(SmartFlowContext context)
        {
            _context = context;
        }

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
    }
}
