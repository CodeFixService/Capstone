using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Roles
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Rol> ListaRoles { get; set; } = new List<Rol>();

        public async Task OnGetAsync()
        {
            ListaRoles = await _context.Roles.ToListAsync();
        }
    }
}
