using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Servicios
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Servicio> ListaServicios { get; set; } = new List<Servicio>();

        public async Task OnGetAsync()
        {
            ListaServicios = await _context.Servicios.ToListAsync();
        }
    }
}
