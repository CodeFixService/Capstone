using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using SmartFlow.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartFlow.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;

        public IndexModel(SmartFlowContext context)
        {
            _context = context;
        }

        // 🔹 Propiedades públicas para mostrar en la vista
        public List<Carrera> Carreras { get; set; } = new();
        public List<Servicio> Servicios { get; set; } = new();

        public void OnGet()
        {
            // Cargar todas las carreras y servicios activos
            Carreras = _context.Carreras
                .OrderBy(c => c.Nombre)
                .ToList();

            Servicios = _context.Servicios
                .OrderBy(s => s.Nombre)
                .ToList();
        }
    }
}
    