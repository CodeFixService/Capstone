using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
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

        public IList<Arancel> ListaAranceles { get; set; } = new List<Arancel>();

        public async Task OnGetAsync()
        {
            // Incluimos la relación con Carrera para mostrar el nombre
            ListaAranceles = await _context.Aranceles
                .Include(a => a.Carrera)
                .ToListAsync();
        }
    }
}
