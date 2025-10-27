using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Usuario.Notificaciones
{
    public class VerTodasModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public VerTodasModel(SmartFlowContext context)
        {
            _context = context;
        }

        public List<Notificacion> Notificaciones { get; set; } = new();

        public async Task OnGetAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                Response.Redirect("/Login/Index");
                return;
            }

            Notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            // 🔹 Marcar como leídas si aún no lo están
            foreach (var n in Notificaciones.Where(x => !x.Leida))
                n.Leida = true;

            await _context.SaveChangesAsync();
        }
    }
}
