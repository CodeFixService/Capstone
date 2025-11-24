using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Coordinador.Notificaciones
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Notificacion> ListaNotificaciones { get; set; } = new List<Notificacion>();

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return RedirectToPage("/Login/Login");

            ListaNotificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            return Page();
        }

        // 🔹 Versión parcial para el ícono de notificaciones
        public IActionResult OnGetPartial()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Content("<div class='alert alert-danger'>Sesión expirada.</div>", "text/html");

            var notificaciones = _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToList();

            var html = string.Join("", notificaciones.Select(n => $@"
                <div class='list-group-item {(n.Leida ? "text-muted" : "fw-bold")}'
                     onclick=""marcarLeidaYRedirigir({n.Id}, '/Coordinador/Notificaciones')"">
                    <div class='d-flex justify-content-between'>
                        <strong>{n.Titulo}</strong>
                        <small class='text-muted'>{n.FechaCreacion:dd-MM-yyyy HH:mm}</small>
                    </div>
                    <div>{n.Mensaje}</div>
                </div>"));

            return Content(html, "text/html");
        }

        public IActionResult OnGetMarcarLeida(int id)
        {
            var notificacion = _context.Notificaciones.FirstOrDefault(n => n.Id == id);
            if (notificacion != null)
            {
                notificacion.Leida = true;
                _context.SaveChanges();
            }
            return new JsonResult(new { ok = true });
        }

        public JsonResult OnGetCount()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { count = 0 });

            var count = _context.Notificaciones.Count(n => n.UsuarioId == usuarioId && !n.Leida);
            return new JsonResult(new { count });
        }
    }
}
