using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        public async Task<JsonResult> OnGetEstadosAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { data = Array.Empty<object>() });

            var lista = await _context.Solicitudes
                .Where(s => s.UsuarioId == usuarioId)
                .Select(s => new {
                    id = s.Id,
                    estado = s.Estado,
                    comentario = s.Motivo
                })
                .ToListAsync();

            return new JsonResult(new { data = lista });
        }

        public async Task<JsonResult> OnGetDetalleAsync(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { success = false });

            var sol = await _context.Solicitudes
                .Include(s => s.Servicio)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id && s.UsuarioId == usuarioId);

            if (sol == null)
                return new JsonResult(new { success = false });

            return new JsonResult(new
            {
                id = sol.Id,
                asunto = sol.Asunto,
                descripcion = sol.Descripcion,
                fecha = sol.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                servicio = sol.Servicio?.Nombre ?? "No asignado",
                estado = sol.Estado,
                comentario = sol.Motivo ?? "Sin comentarios"
            });
        }

    }
}
