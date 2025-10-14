using System;

namespace SmartFlow.Web.Models
{
    public class ChatMensaje
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }        // dueño del hilo (estudiante)
        public string EmisorRol { get; set; } = ""; // "Usuario" o "Admin"
        public string Texto { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool LeidoPorUsuario { get; set; } = false;
        public bool LeidoPorAdmin { get; set; } = false;
    }
}
