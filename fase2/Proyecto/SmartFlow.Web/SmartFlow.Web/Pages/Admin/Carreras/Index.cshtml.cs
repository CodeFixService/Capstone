using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Carreras
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Carrera> ListaCarreras { get; set; } = new List<Carrera>();

        public async Task OnGetAsync()
        {
            ListaCarreras = await _context.Carreras.ToListAsync();
        }
    }
}
