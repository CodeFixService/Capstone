using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        public IList<SmartFlow.Web.Models.Usuario> ListaUsuarios { get; set; } = new List<SmartFlow.Web.Models.Usuario>();
        public bool PuedeCrearUsuarios { get; set; }

        //  Filtros (se cargan desde el formulario GET)
        [BindProperty(SupportsGet = true)]
        public string? RolFiltro { get; set; }

        public string? CarreraFiltro { get; set; }

        public async Task OnGetAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuarioActual = _context.Usuarios
                .Include(u => u.Carrera)
                .FirstOrDefault(u => u.Id == usuarioId);

            var rol = HttpContext.Session.GetString("Rol");

            // Solo Admin puede usar esta vista
            if (rol == "Admin" && usuarioActual?.CarreraId == null)
            {
                PuedeCrearUsuarios = true;
                
            }
            else {   
                PuedeCrearUsuarios = false;
            
            }

            //  1) Query base
          
            var query = _context.Usuarios
                .Include(u => u.Carrera)
                .AsQueryable();

            //  2) Admin general → ve TODOS

            if (usuarioActual.CarreraId == null)
            {
                // No se filtra por carrera
            }
            else
            {
                
                //  3) Admin de carrera → SOLO su carrera
                
                query = query.Where(u => u.CarreraId == usuarioActual.CarreraId);
            }

            
            //  4) Filtros manuales del usuario
            

            // Filtrar por Rol
            if (!string.IsNullOrEmpty(RolFiltro))
                query = query.Where(u => u.Rol == RolFiltro);

            // Filtrar por Carrera
            if (!string.IsNullOrEmpty(CarreraFiltro))
                query = query.Where(u => u.Carrera.Nombre == CarreraFiltro);

            
            //  5) Cargar lista final
            
            ListaUsuarios = await query.ToListAsync();

        }
    }
}
