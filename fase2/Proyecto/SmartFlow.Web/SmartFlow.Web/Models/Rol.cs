using System.ComponentModel.DataAnnotations;

namespace SmartFlow.Web.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }
    }
}
