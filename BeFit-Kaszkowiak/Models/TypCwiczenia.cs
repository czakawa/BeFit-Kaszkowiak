using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BeFit_Kaszkowiak.Models
{
    public class TypCwiczenia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa typu jest wymagana.")]
        [StringLength(100)]
        [Display(Name = "Nazwa ćwiczenia")]
        public string Nazwa { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Opis")]
        public string? Opis { get; set; }

        public ICollection<Cwiczenie>? Cwiczenia { get; set; }
    }
}
