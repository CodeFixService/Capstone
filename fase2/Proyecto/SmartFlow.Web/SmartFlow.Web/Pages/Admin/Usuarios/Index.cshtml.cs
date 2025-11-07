using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

public IList<SmartFlow.Web.Models.Usuario> ListaUsuarios { get; set; } = new List<SmartFlow.Web.Models.Usuario>();
        // 🔹 Filtros (se cargan desde el formulario GET)
        [BindProperty(SupportsGet = true)]
        public string? RolFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CarreraFiltro { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Usuarios
                .Include(u => u.Carrera)
                .AsQueryable();

            // 🔹 Filtrado por Rol
            if (!string.IsNullOrEmpty(RolFiltro))
                query = query.Where(u => u.Rol == RolFiltro);

            // 🔹 Filtrado por Carrera
            if (!string.IsNullOrEmpty(CarreraFiltro))
                query = query.Where(u => u.Carrera.Nombre == CarreraFiltro);

            ListaUsuarios = await query.ToListAsync();
        }
    }
}
