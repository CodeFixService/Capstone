using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFlow.Web.Helpers; // arriba del archivo


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

        public async Task OnGetAsync()
        {
            ListaUsuarios = await _context.Usuarios
                .Include(u => u.Carrera) // ?? importante para mostrar el nombre
                .ToListAsync();
        }

    }
}
