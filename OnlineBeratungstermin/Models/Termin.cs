namespace OnlineBeratungstermin.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DB Model Entity Termin
    /// </summary>
    public class Termin
    {
        public long ID { get; set; }

        [Required]
        public int TerminartID { get; set; }

        [Required]
        public long BeraterID { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime Ende { get; set; }

        [EmailAddress]
        [MaxLength(50)]
        public string KundenEmail { get; set; }

        [Required]
        [MaxLength(100)]
        public string KundenName { get; set; }

        [Required]
        [MaxLength(50)]
        public string KundenVertragsnummer { get; set; }

        public decimal KundenVertragsGesamtbeitrag { get; set; }

    }
}
