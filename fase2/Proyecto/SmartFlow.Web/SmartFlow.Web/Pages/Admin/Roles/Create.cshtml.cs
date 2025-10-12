using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Roles
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Rol Rol { get; set; } = new Rol();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Roles.Add(Rol);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
