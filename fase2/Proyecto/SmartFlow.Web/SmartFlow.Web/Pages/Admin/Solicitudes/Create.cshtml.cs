using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Solicitudes
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Solicitud Solicitud { get; set; } = new Solicitud();

        public SelectList UsuariosSelectList { get; set; }
        public SelectList ServiciosSelectList { get; set; }

        public void OnGet()
        {
            UsuariosSelectList = new SelectList(_context.Usuarios.ToList(), "Id", "Nombre");
            ServiciosSelectList = new SelectList(_context.Servicios.ToList(), "Id", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                UsuariosSelectList = new SelectList(_context.Usuarios.ToList(), "Id", "Nombre");
                ServiciosSelectList = new SelectList(_context.Servicios.ToList(), "Id", "Nombre");
                return Page();
            }

            Solicitud.Estado = "Pendiente"; // siempre inicia como pendiente
            Solicitud.FechaCreacion = DateTime.Now;

            _context.Solicitudes.Add(Solicitud);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
