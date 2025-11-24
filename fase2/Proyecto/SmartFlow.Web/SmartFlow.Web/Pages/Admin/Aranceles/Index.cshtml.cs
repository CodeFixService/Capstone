using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Aranceles
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        // ✅ Filtros
        [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
        [BindProperty(SupportsGet = true)] public int? CarreraId { get; set; }
        [BindProperty(SupportsGet = true)] public string? TipoArancel { get; set; }

        // ✅ Datos para los selects
        public SelectList Carreras { get; set; }
        public List<string> Tipos { get; set; } = new() { "Matrícula", "Arancel", "Certificado"};

        // ✅ Resultado final
        public IList<Arancel> ListaAranceles { get; set; } = new List<Arancel>();

        public async Task OnGetAsync()
        {
            // 🔹 Cargar carreras para el filtro
            Carreras = new SelectList(await _context.Carreras.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");

            // 🔹 Base de consulta
            var query = _context.Aranceles
                .Include(a => a.Carrera)
                .AsQueryable();

            // 🔹 Filtro por texto
            if (!string.IsNullOrWhiteSpace(Busqueda))
                query = query.Where(a => a.Nombre.Contains(Busqueda) || (a.Descripcion ?? "").Contains(Busqueda));

            // 🔹 Filtro por carrera
            if (CarreraId.HasValue && CarreraId > 0)
                query = query.Where(a => a.CarreraId == CarreraId);

            // 🔹 Filtro por tipo
            if (!string.IsNullOrWhiteSpace(TipoArancel))
                query = query.Where(a => a.Nombre.Contains(TipoArancel));

            // 🔹 Ejecutar consulta
            ListaAranceles = await query.OrderBy(a => a.Nombre).ToListAsync();
        }
    }
}
