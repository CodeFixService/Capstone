using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using System.Linq;

namespace SmartFlow.Web.Pages
{
    [IgnoreAntiforgeryToken] // permite POST desde fetch sin token
    public class NotificacionesModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public NotificacionesModel(SmartFlowContext context) => _context = context;

        // POST /Notificaciones?handler=MarcarLeidas
        public IActionResult OnPostMarcarLeidas(int usuarioId)
        {
            if (usuarioId <= 0)
                return new JsonResult(new { success = false, message = "Usuario inválido" });

            var pendientes = _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToList();

            if (pendientes.Count > 0)
            {
                foreach (var n in pendientes)
                    n.Leida = true;

                _context.SaveChanges();
            }

            return new JsonResult(new { success = true });
        }

        // GET /Notificaciones?handler=Count&usuarioId=123
        public IActionResult OnGetCount(int usuarioId)
        {
            if (usuarioId <= 0)
                return new JsonResult(new { count = 0 });

            var count = _context.Notificaciones
                .Count(n => n.UsuarioId == usuarioId && !n.Leida);

            return new JsonResult(new { count });
        }
    }
}
