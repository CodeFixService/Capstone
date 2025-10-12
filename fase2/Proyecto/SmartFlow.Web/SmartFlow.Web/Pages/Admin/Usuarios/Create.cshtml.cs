using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SmartFlow.Web.Models.Usuario Usuario { get; set; } = new SmartFlow.Web.Models.Usuario();

        // ?? Lista desplegable de roles
        public SelectList RolesSelectList { get; set; }
        public SelectList CarrerasSelectList { get; set; }

        public void OnGet()
        {
            // Cargar roles desde la tabla Roles
            RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Si falla la validación, recarga los roles para que el select no se pierda
                RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre");
                return Page();
            }

            // Guardar auditoría
            Usuario.CreadoPor = HttpContext.Session.GetString("UsuarioNombre");
            Usuario.FechaCreacion = DateTime.Now;

            // Guardar usuario
            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
