using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Etiquetas
    {

        public class EtiquetaDto
        {

            public int IdEtiqueta { get; set; }    

            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El color es obligatorio.")]
            [StringLength(20)]
            [Display(Name = "Color")]
            public string Color { get; set; } = string.Empty;
        }

        public class EtiquetaCrearVm
        {
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(50)]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El color es obligatorio.")]
            [StringLength(20)]
            public string Color { get; set; } = string.Empty;
        }

        public class EtiquetaCrearDto
        {
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(50)]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El color es obligatorio.")]
            [StringLength(20)]
            public string Color { get; set; } = string.Empty;
        }

        public class EtiquetaVm
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
        }

    }
}
