using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;

using UsuarioModel = SmartFlow.Web.Models.Usuario;

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
        public UsuarioModel Usuario { get; set; } = new UsuarioModel();
        public SelectList RolesSelectList { get; set; }
        public SelectList CarrerasSelectList { get; set; }

        public void OnGet()
        {
            //  Cargar roles desde la tabla Roles
            RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre");

            //  Cargar carreras desde la tabla Carreras
            CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                //  Si falla validación, recargar selects
                RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre");
                CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre");
                return Page();
            }
            Usuario.CreadoPor = HttpContext.Session.GetString("UsuarioNombre");
            Usuario.FechaCreacion = DateTime.Now;

            Usuario.Password = PasswordHelper.HashPassword(Usuario.Password);

            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
