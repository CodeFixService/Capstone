namespace SmartFlow.Web.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // "Admin" o "Usuario"

        public string? CreadoPor { get; set; } // ← ahora opcional
        public DateTime? FechaCreacion { get; set; } = DateTime.Now; // ← opcional
        public int? CarreraId { get; set; }   // Guarda el ID de la carrera
        public Carrera? Carrera { get; set; } // Propiedad de navegación

    }
}
