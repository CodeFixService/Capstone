using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Pages.Usuario.Notificaciones
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Notificacion> ListaNotificaciones { get; set; }

        public async Task OnGetAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                Response.Redirect("/Login/Index");
                return;
            }

            ListaNotificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }
    }
}
