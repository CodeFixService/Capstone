using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFlow.Web.Pages.Admin.Aranceles
{
    public class CreateModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public CreateModel(SmartFlowContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Arancel Arancel { get; set; } = new Arancel();

        // ?? Lista desplegable de carreras
        public SelectList CarrerasSelectList { get; set; }

        public void OnGet()
        {
            CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarrerasSelectList = new SelectList(_context.Carreras.ToList(), "Id", "Nombre");
                return Page();
            }

            _context.Aranceles.Add(Arancel);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
