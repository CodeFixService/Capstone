using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;
using SmartFlow.Web.Models;
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
            var sesion = HttpContext.Session;

            if (!AccesoHelper.TieneSesion(sesion))
            {
                Response.Redirect("/Login/Login");
                return;
            }

            if (!AccesoHelper.EsUsuario(sesion))
            {
                Response.Redirect("/Admin/Index");
                return;
            }


        }


        // 🔔 ACCIÓN: Marcar notificaciones como leídas (se mantiene igual)
     
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
