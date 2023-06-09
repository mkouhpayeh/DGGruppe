using System.ComponentModel.DataAnnotations;

namespace OnlineBeratungstermin.Models
{
    /// <summary>
    /// Model based class to use as a return value of GetTermine method
    /// </summary>
    public class AvailableTerminModel
    {
        public long BeraterID { get; set; }
        public int TeminartID { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime EndDate { get; set; }
        
    }
}
