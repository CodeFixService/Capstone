using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartFlow.Web.Data;
using SmartFlow.Web.Helpers;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartFlow.Web.Pages.Admin.Usuarios
{
    public class EditModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public EditModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SmartFlow.Web.Models.Usuario Usuario { get; set; } = new SmartFlow.Web.Models.Usuario();

        public SelectList RolesSelectList { get; set; }
        public SelectList CarrerasSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            Usuario = usuario;

            RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre", Usuario.Rol);
            CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Usuario.CarreraId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 🔹 Desactiva validación de contraseña vacía
            ModelState.Remove("Usuario.Password");
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                RolesSelectList = new SelectList(_context.Roles.ToList(), "Nombre", "Nombre", Usuario.Rol);
                CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre", Usuario.CarreraId);
                return Page();
            }

            // 🔹 Buscar el usuario original
            var original = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == Usuario.Id);

            if (original == null)
                return NotFound();

            // 🔹 Mantener auditoría
            Usuario.CreadoPor = original.CreadoPor;
            Usuario.FechaCreacion = original.FechaCreacion;

            // 🔹 Si la contraseña fue modificada → cifrarla
            if (!string.IsNullOrWhiteSpace(Usuario.Password))
            {
                Usuario.Password = PasswordHelper.HashPassword(Usuario.Password);
            }
            else
            {
                // Mantener la anterior si no se ingresó nueva
                Usuario.Password = original.Password;
            }

            try
            {
                _context.Update(Usuario);
                await _context.SaveChangesAsync();

                var usuarioActivo = HttpContext.Session.GetString("UsuarioNombre") ?? "Administrador";
                await BitacoraHelper.RegistrarAsync(_context, usuarioActivo,
                    "Usuarios", "Edición", $"Se editó el usuario con ID {Usuario.Id}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(u => u.Id == Usuario.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
