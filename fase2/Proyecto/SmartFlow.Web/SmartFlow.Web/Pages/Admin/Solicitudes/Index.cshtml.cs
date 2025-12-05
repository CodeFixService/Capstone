using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Solicitudes
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<Solicitud> ListaSolicitudes { get; set; } = new List<Solicitud>();

        [BindProperty(SupportsGet = true)]
        public string? CarreraFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EstadoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            //  1. Obtener usuario desde la sesión
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                Response.Redirect("/Login/Index");
                return;
            }

            var usuarioActual = await _context.Usuarios
                .Include(u => u.Carrera)
                .FirstOrDefaultAsync(u => u.Id == usuarioId.Value);

            if (usuarioActual == null)
            {
                Response.Redirect("/Login/Index");
                return;
            }

            //🟢 2. Construir la consulta base
            var query = _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .AsQueryable();

            // 3. Filtro automático según rol/carrera

            // ADMIN
            if (usuarioActual.Rol == "Admin")
            {
                if (usuarioActual.CarreraId != null)
                {
                    // Admin de carrera → solo su carrera
                    query = query.Where(s => s.Usuario.CarreraId == usuarioActual.CarreraId);
                }
                // Admin general: NO se filtra — ve todo
            }

            // COORDINADOR → solo su carrera
            else if (usuarioActual.Rol == "Coordinador")
            {
                query = query.Where(s => s.Usuario.CarreraId == usuarioActual.CarreraId);
            }

            // (Usuario/Estudiante nunca debería entrar aquí)
            // pero si entrara por error:
            else if (usuarioActual.Rol == "Usuario")
            {
                query = query.Where(s => s.UsuarioId == usuarioActual.Id);
            }

            //  4. Filtros adicionales manuales del admin (carrera/estado)
            if (!string.IsNullOrEmpty(CarreraFiltro))
                query = query.Where(s => s.Usuario.Carrera.Nombre == CarreraFiltro);

            if (!string.IsNullOrEmpty(EstadoFiltro))
                query = query.Where(s => s.Estado == EstadoFiltro);

            //  5. Ejecutar consulta
            ListaSolicitudes = await query
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }
        public async Task<JsonResult> OnGetEstadosAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return new JsonResult(new { data = new object[0] });

            var usuarioActual = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId.Value);

            var query = _context.Solicitudes
                .AsQueryable();

            // filtros de rol igual que en OnGetAsync
            if (usuarioActual.Rol == "Admin" && usuarioActual.CarreraId != null)
                query = query.Where(s => s.Usuario.CarreraId == usuarioActual.CarreraId);

            if (usuarioActual.Rol == "Coordinador")
                query = query.Where(s => s.Usuario.CarreraId == usuarioActual.CarreraId);

            var lista = await query
                .Select(s => new {
                    id = s.Id,
                    estado = s.Estado
                })
                .ToListAsync();

            return new JsonResult(new { data = lista });
        }



    }
}
