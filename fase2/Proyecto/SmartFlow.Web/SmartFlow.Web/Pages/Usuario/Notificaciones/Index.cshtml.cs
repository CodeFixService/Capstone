using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public IActionResult OnGetPartial()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Content("<div class='alert alert-danger'>Sesión expirada. Inicia sesión nuevamente.</div>", "text/html");
            }

            var notificaciones = _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToList();

            // 🔹 Solo devuelve el bloque HTML de las notificaciones (sin layout)
            var html = "";
            foreach (var n in notificaciones)
            {
                html += $@"
            <div class='list-group-item {(n.Leida ? "text-muted" : "fw-bold")}'>
                <div class='d-flex justify-content-between'>
                    <strong>{n.Titulo}</strong>
                    <small class='text-muted'>{n.FechaCreacion:dd-MM-yyyy HH:mm}</small>
                </div>
                <div>{n.Mensaje}</div>
            </div>";
            }

            return Content(html, "text/html");
        }


    }
}
