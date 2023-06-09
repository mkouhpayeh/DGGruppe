namespace OnlineBeratungstermin.Models
{
    public class TimeSlotModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSlotModel(DateTime start, DateTime end)
        {
            this.StartTime = start;
            this.EndTime = end;
        }
    }
}
