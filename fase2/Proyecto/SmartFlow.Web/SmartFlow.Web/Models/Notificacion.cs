using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFlow.Web.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; }

        [StringLength(500)]
        public string Mensaje { get; set; }

        public bool Leida { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // 🔹 Relación con usuario receptor
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // 🔹 Tipo de notificación (Aprobación, Calendario, Mensaje)
        [StringLength(50)]
        public string Tipo { get; set; }
    }
}
