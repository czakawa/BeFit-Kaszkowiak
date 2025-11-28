using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeFit_Kaszkowiak.Models
{
    public class Cwiczenie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana.")]
        [StringLength(120, ErrorMessage = "Nazwa może mieć maks. 120 znaków.")]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; } = string.Empty;

        [Display(Name = "Opis")]
        [StringLength(1000)]
        public string? Opis { get; set; }

        // czas trwania w sekundach
        [Display(Name = "Czas trwania (s)")]
        [Range(0, 24 * 60 * 60, ErrorMessage = "Czas musi być dodatni i mniejszy niż doba.")]
        public int CzasTrwaniaSek { get; set; }

        // powtórzenia w serii
        [Display(Name = "Powtórzenia (na serię)")]
        [Range(0, 10000, ErrorMessage = "Powtórzenia muszą być nieujemne.")]
        public int Powtorzenia { get; set; }

        // liczba serii (nowe pole — widoki z niego korzystają)
        [Display(Name = "Liczba serii")]
        [Range(1, 1000, ErrorMessage = "Ilość serii musi być co najmniej 1.")]
        public int IloscSerii { get; set; } = 1;

        // obciążenie — nullable, bo może być ćwiczenie bez obciążenia
        [Display(Name = "Obciążenie (kg)")]
        [Range(0, 10000, ErrorMessage = "Obciążenie musi być nieujemne.")]
        public double? Obciazenie { get; set; }

        // Relacje / klucze obce
        [Display(Name = "Typ ćwiczenia")]
        [Required(ErrorMessage = "Musisz wybrać typ ćwiczenia.")]
        public int TypCwiczeniaId { get; set; }
        public TypCwiczenia? TypCwiczenia { get; set; }

        [Display(Name = "Sesja")]
        [Required(ErrorMessage = "Musisz wybrać sesję.")]
        public int SesjaId { get; set; }
        public Sesja? Sesja { get; set; }

        // właściciel wpisu — ustawiany automatycznie w kontrolerze
        public string? UserId { get; set; }
    }
}
