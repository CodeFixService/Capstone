using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
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

        [BindProperty(SupportsGet = true)]
        public string? CarreraFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EstadoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .AsQueryable();

            if (!string.IsNullOrEmpty(CarreraFiltro))
                query = query.Where(s => s.Usuario.Carrera.Nombre == CarreraFiltro);

            if (!string.IsNullOrEmpty(EstadoFiltro))
                query = query.Where(s => s.Estado == EstadoFiltro);

            ListaSolicitudes = await query.ToListAsync();
        }
    }
}
