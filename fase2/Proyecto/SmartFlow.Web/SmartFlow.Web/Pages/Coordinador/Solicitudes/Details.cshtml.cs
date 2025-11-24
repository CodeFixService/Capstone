using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Coordinador.Solicitudes
{
    public class DetailsModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public DetailsModel(SmartFlowContext context)
        {
            _context = context;
        }

        public Solicitud? Solicitud { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Coordinador")
                return RedirectToPage("/Login/Login");

            var coordId = HttpContext.Session.GetInt32("UsuarioId");
            if (coordId == null)
                return RedirectToPage("/Login/Login");

            // 🔹 Obtener la carrera del coordinador
            var carreraCoord = await _context.Usuarios
                .Where(u => u.Id == coordId)
                .Select(u => u.CarreraId)
                .FirstOrDefaultAsync();

            if (carreraCoord == null)
                return NotFound("El coordinador no tiene una carrera asignada.");

            // 🔹 Buscar la solicitud y validar que pertenezca a su carrera
            Solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                    .ThenInclude(u => u.Carrera)
                .Include(s => s.Servicio)
                .FirstOrDefaultAsync(s => s.Id == id && s.Usuario.CarreraId == carreraCoord);

            if (Solicitud == null)
                return NotFound("No se encontró la solicitud o no pertenece a su carrera.");

            return Page();
        }
    }
}
