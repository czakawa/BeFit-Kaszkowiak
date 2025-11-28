using System.ComponentModel.DataAnnotations;

namespace BeFit_Kaszkowiak.Models
{
    public class Sesja
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytu³ jest wymagany")]
        [Display(Name = "Tytu³")]
        public string Tytul { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime Data { get; set; }

        [Display(Name = "Opis")]
        public string Opis { get; set; }

        // w³aœciciel
        public string UserId { get; set; }

        public ICollection<Cwiczenie> Cwiczenia { get; set; }
    }
}
