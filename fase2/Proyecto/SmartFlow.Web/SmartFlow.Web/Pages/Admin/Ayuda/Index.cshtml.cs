using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartFlow.Web.Data;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SmartFlow.Web.Pages.Admin.Ayuda
{
    public class IndexModel : PageModel
    {
        private readonly SmartFlowContext _context;
        public IndexModel(SmartFlowContext context) => _context = context;

        public List<Item> Conversaciones { get; set; } = new();

        public class Item
        {
            public int UsuarioId { get; set; }
            public string Nombre { get; set; } = "";
            public string UltimoTexto { get; set; } = "";
            public DateTime FechaUltimo { get; set; }
            public int NoLeidos { get; set; }
        }

        public void OnGet()
        {
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != "Admin") { Response.Redirect("/Login/Login"); return; }

            Conversaciones = _context.Usuarios
                .Select(u => new Item
                {
                    UsuarioId = u.Id,
                    Nombre = u.Nombre,
                    UltimoTexto = _context.ChatMensajes.Where(c => c.UsuarioId == u.Id)
                                   .OrderByDescending(c => c.Fecha).Select(c => c.Texto).FirstOrDefault() ?? "-",
                    FechaUltimo = _context.ChatMensajes.Where(c => c.UsuarioId == u.Id)
                                   .OrderByDescending(c => c.Fecha).Select(c => c.Fecha).FirstOrDefault(),
                    NoLeidos = _context.ChatMensajes.Count(c => c.UsuarioId == u.Id && !c.LeidoPorAdmin && c.EmisorRol == "Usuario")
                })
                .OrderByDescending(x => x.FechaUltimo)
                .ToList();
        }
    }
}
