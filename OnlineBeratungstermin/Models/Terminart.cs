namespace OnlineBeratungstermin.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DB Model Entity Terminart
    /// </summary>
    public class Terminart
    {
        public int ID { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        //Dauer ist in Minuten
        [Required]
        [MaxLength(3)]
        public int Dauer { get; set; }

        public ICollection<Termin> Termine { get; set; }
    }
}
