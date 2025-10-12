using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Servicios
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Servicio Servicio { get; set; } = new Servicio();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Servicios.Add(Servicio);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
