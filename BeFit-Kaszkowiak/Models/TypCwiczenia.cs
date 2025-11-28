using System.ComponentModel.DataAnnotations;

namespace BeFit_Kaszkowiak.Models
{
    public class TypCwiczenia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(100, ErrorMessage = "Nazwa mo¿e mieæ maksymalnie 100 znaków")]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; }

        [StringLength(500)]
        [Display(Name = "Opis")]
        public string Opis { get; set; }
    }
}
