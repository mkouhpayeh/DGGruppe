namespace OnlineBeratungstermin.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// DB Model Entity to show Berater data
    /// </summary>
    public class Berater
    {
        public long ID { get; set; }    

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<Termin> Termine { get; set; }
    }
}
