using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Coordinador.Solicitudes
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
        public string? EstadoFiltro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador") return RedirectToPage("/Login/Login");

            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null) return RedirectToPage("/Login/Login");

            // 🔹 Carrera del coordinador
            var carreraCoord = await _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefaultAsync();

            if (carreraCoord == null)
            {
                ListaSolicitudes = new List<Solicitud>();
                return Page();
            }

            // 🔹 Solo solicitudes de su carrera
            var query = _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .Where(s => s.Usuario.CarreraId == carreraCoord)
                .AsQueryable();

            if (!string.IsNullOrEmpty(EstadoFiltro))
                query = query.Where(s => s.Estado == EstadoFiltro);

            ListaSolicitudes = await query.ToListAsync();
            return Page();
        }
    }
}
