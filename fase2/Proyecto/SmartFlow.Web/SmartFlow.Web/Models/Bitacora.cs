using System;

namespace SmartFlow.Web.Models
{
    public class Bitacora
    {
        public int Id { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Modulo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}
