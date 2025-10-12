using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Solicitudes
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Solicitud> ListaSolicitudes { get; set; } = new List<Solicitud>();

        public async Task OnGetAsync()
        {
            // Incluimos Usuario y Servicio para mostrar nombres
            ListaSolicitudes = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Servicio)
                .ToListAsync();
        }
    }
}
