using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SmartFlow.Web.Helpers;


namespace SmartFlow.Web.Pages.Usuario.Solicitudes
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Solicitud> ListaSolicitudes { get; set; } = new List<Solicitud>();

        public async Task OnGetAsync()
        {
            // ?? Tomamos el ID del usuario actual desde la sesión
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                // Si no hay sesión, redirige al login
                Response.Redirect("/Login/Index");
                return;
            }

            // ?? Cargamos solo las solicitudes de ese usuario
            ListaSolicitudes = await _context.Solicitudes
                .Include(s => s.Servicio)
                .Where(s => s.UsuarioId == usuarioId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }
    }
}
