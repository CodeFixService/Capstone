using System;

namespace SmartFlow.Web.Models
{
    public class ChatMensaje
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }        // dueño del hilo (estudiante)


        // Nuevos campos para relaciones y filtrado
        public int? CoordinadorId { get; set; }   // coordinador de la carrera del estudiante
        public int? AdminId { get; set; }         // admin de la misma carrera
        public int? CarreraId { get; set; }       // carrera asociada
        public string DestinatarioRol { get; set; } = ""; // a quién va dirigido
        public bool LeidoPorCoordinador { get; set; } = false;

        public string EmisorRol { get; set; } = ""; // "Usuario" o "Admin"
        public string Texto { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool LeidoPorUsuario { get; set; } = false;
        public bool LeidoPorAdmin { get; set; } = false;
    }
}
