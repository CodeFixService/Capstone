using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using SmartFlow.Web.Data;
using System.Linq;

namespace SmartFlow.Web.Pages.Usuario
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            // Tu lógica de carga actual
        }

        public JsonResult OnPostMarcarLeidas(int usuarioId)
        {
            var pendientes = _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToList();

            if (pendientes.Any())
            {
                foreach (var n in pendientes)
                    n.Leida = true;

                _context.SaveChanges();
            }

            return new JsonResult(new { success = true });
        }
    }
}
