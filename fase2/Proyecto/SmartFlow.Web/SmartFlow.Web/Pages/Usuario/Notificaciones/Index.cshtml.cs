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

            var html = "";
            foreach (var n in notificaciones)
            {
                html += $@"
    <div class='list-group-item {(n.Leida ? "text-muted" : "fw-bold")}'
         onclick=""marcarLeidaYRedirigir({n.Id}, '/Usuario/Notificaciones')"">
        <div class='d-flex justify-content-between'>
            <strong>{n.Titulo}</strong>
            <small class='text-muted'>{n.FechaCreacion:dd-MM-yyyy HH:mm}</small>
        </div>
        <div>{n.Mensaje}</div>
    </div>";
            }


            return Content(html, "text/html");
        }
        public IActionResult OnGetMarcarLeida(int id)
        {
            Console.WriteLine($"🔹 Handler MarcarLeida ejecutado. ID = {id}");

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
        public JsonResult OnGetReset()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId != null)
            {
                var notis = _context.Notificaciones
                    .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                    .ToList();

                foreach (var n in notis)
                    n.Leida = true;

                _context.SaveChanges();
            }

            return new JsonResult(new { ok = true });
        }
        public IActionResult OnGetListaParcial()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                return Content("<div class='alert alert-danger'>Sesión expirada. Inicia sesión nuevamente.</div>", "text/html");
            }

            var notificaciones = _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(10)
                .ToList();

            var html = "";

            foreach (var n in notificaciones)
            {
                html += $@"
        <div class='list-group-item mb-2 rounded-3 shadow-sm border-0 p-3 {(n.Leida ? "bg-light" : "bg-white")}'
             onclick=""marcarLeidaYRedirigir({n.Id})"">
            <div class='d-flex justify-content-between align-items-center'>
                <div>
                    <strong>{n.Titulo}</strong><br />
                    <small class='text-muted'>{n.FechaCreacion:dd/MM/yyyy HH:mm}</small>
                </div>
            </div>
            <div class='mt-2 text-secondary'>
                {(string.IsNullOrEmpty(n.Mensaje) ? "" : n.Mensaje.Length > 80 ? n.Mensaje.Substring(0, 80) + "..." : n.Mensaje)}
            </div>
        </div>";
            }

            if (!notificaciones.Any())
            {
                html = "<div class='alert alert-info text-center mt-3'>No tienes notificaciones por ahora.</div>";
            }

            return Content(html, "text/html");
        }





    }
}
