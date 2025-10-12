using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SmartFlow.Web.Models
{
    public class Carrera
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        // 🔹 Relación uno a muchos (una carrera → varios usuarios)
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
