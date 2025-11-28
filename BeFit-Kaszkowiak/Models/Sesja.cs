using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeFit_Kaszkowiak.Models
{
    public class Sesja
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        [StringLength(150)]
        [Display(Name = "Tytuł sesji")]
        public string Tytul { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Opis")]
        public string? Opis { get; set; }

        // <-- mapujemy property DataRozpoczecia do kolumny 'Data' w bazie
        [Column("Data")]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime DataRozpoczecia { get; set; }

        [Display(Name = "Data zakończenia")]
        public DateTime? DataZakonczenia { get; set; }

        // właściciel
        public string? UserId { get; set; }
    }
}
