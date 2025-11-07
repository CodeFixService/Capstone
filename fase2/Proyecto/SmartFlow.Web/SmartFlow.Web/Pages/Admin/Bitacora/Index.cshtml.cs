using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Bitacora
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<SmartFlow.Web.Models.Bitacora> ListaBitacora { get; set; } = new List<SmartFlow.Web.Models.Bitacora>();

        public async Task OnGetAsync()
        {
            ListaBitacora = await _context.Bitacoras
                .OrderByDescending(b => b.Fecha)
                .ToListAsync();
        }
    }
}
