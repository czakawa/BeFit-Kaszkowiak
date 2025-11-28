using System.ComponentModel.DataAnnotations;

namespace BeFit_Kaszkowiak.Models
{
    public class Cwiczenie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; }

        [Display(Name = "Opis")]
        public string Opis { get; set; }

        [Display(Name = "Czas trwania (s)")]
        public int CzasTrwaniaSek { get; set; }

        [Display(Name = "Powtórzenia")]
        public int Powtorzenia { get; set; }

        [Display(Name = "Typ æwiczenia")]
        public int TypCwiczeniaId { get; set; }
        public TypCwiczenia TypCwiczenia { get; set; }

        [Display(Name = "Sesja")]
        public int SesjaId { get; set; }
        public Sesja Sesja { get; set; }
    }
}
