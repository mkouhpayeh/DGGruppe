using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBeratungstermin.Helpers;
using OnlineBeratungstermin.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace OnlineBeratungstermin.Controllers
{
    //======================================================================================================
    //Kalenderwoche refers to a week within a calendar year. In Germany and some other European countries,
    //calendar weeks are commonly used to reference specific weeks throughout the year.
    //Each calendar week is identified by a number, starting from 1 to 52 or 53, depending on the year.
    //The first calendar week of the year is the one that includes the first Thursday of January.
    //For example, "Kalenderwoche 23" refers to the 23rd week of the calendar year. 
    //Assumbtion 1 =>  All "Berater" work fulltime during all working days and hours.
    //======================================================================================================


    [Route("api/[controller]")]
    [ApiController]
    public class TermineController : ControllerBase
    {
        private readonly ILogger<TermineController> _logger;

        private readonly OnlineTermineDbContext _dbContext;

        public TermineController(OnlineTermineDbContext dbContext, ILogger<TermineController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all free termin based on available Termine during working time
        /// </summary>
        /// <param name="kalenderWoche">The number of week in a year should between 1, 52</param>
        /// <param name="terminartID">Id of Termin plan, for example 15 min or 30 min.</param>
        /// <returns>The list of available termins with Berater Id to select one</returns>
        [HttpGet("GetTermine")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AvailableTerminModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AvailableTerminModel>>> GetTermine(int kalenderWoche, int terminartID)
        {
            {
                try
                {
                    // first of all check terminartID condition to prevent unnecessary DB query if the first term be true, doesn't check the second condition   
                    if (terminartID < 1 || !_dbContext.Terminarten.Any(i => i.ID == terminartID))
                    {
                        _logger.LogError($"The TerminartID= {terminartID} is not valid.");
                        return StatusCode(400, "Please enter a valid Terminart ID.");
                    }

                    if (!IsKalendarWocheValid(kalenderWoche))
                    {
                        _logger.LogError($"The kalenderWoche= {kalenderWoche} is not valid.");
                        return StatusCode(400, "Please enter a valid KalenderWoche number.");
                    }

                    //Calculate Start and End of each Week
                    DateTime startOfWeek = GetStartWorkingDateOfWoche(kalenderWoche);
                    DateTime endOfWeek = GetEndWorkingDateOfWoche(kalenderWoche).AddDays(1);

                    //Retreive Termin duration time based on input value
                    var terminDuration = await _dbContext.Terminarten
                        .Where(i => i.ID == terminartID)
                        .Select(i => i.Dauer)
                        .FirstOrDefaultAsync();

                    //We can filter available Berater based on availability options if exists
                    //Assumption: All Brater are available
                    var availableBeraters = await _dbContext.Beraters
                        .Select(i => i.ID)
                        .ToListAsync();

                    //Returns all future Termin + current Termin
                    var appointments = await _dbContext.Termine
                        .Where(t => (t.Start >= startOfWeek && t.Ende < endOfWeek) || (t.Ende > startOfWeek))
                        .ToListAsync();


                    // Generate the available time slots for each Berater
                    List<TimeSlotModel> availableTimeslots = GetAvailableTimeSlots(TrimGivenTime(startOfWeek), endOfWeek, TimeSpan.FromMinutes(terminDuration), appointments);

                    var availableTermine = new List<AvailableTerminModel>();

                    //check availibity of each timeslot
                    foreach (var beraterID in availableBeraters)
                    {
                        var appointment = appointments.Where(i => i.BeraterID == beraterID).ToList();

                        availableTermine.AddRange(
                            from timeslot in availableTimeslots
                            let app = appointment.Where(t =>
                                (t.Start > timeslot.StartTime && t.Ende < timeslot.StartTime) ||
                                (t.Start >= timeslot.StartTime && t.Ende <= timeslot.EndTime) ||
                                (t.Ende > timeslot.StartTime && t.Ende < timeslot.EndTime) ||
                                (timeslot.StartTime >= t.Start && timeslot.EndTime <= t.Ende)
                            ).ToList()
                            where app.Count == 0
                            select new AvailableTerminModel
                            {
                                BeraterID = beraterID,
                                StartDate = timeslot.StartTime,
                                EndDate = timeslot.EndTime,
                                TeminartID = terminartID
                            }
                        );
                    }

                    return Ok(availableTermine);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"While getting termin for KalenderWoche={kalenderWoche} and TerminartID={terminartID} got this Error=> {ex.Message}");
                    return StatusCode(500, $"An error occurred while retrieving data. {ex.Message}");
                }
            }
        }


        /// <summary>
        /// Stores selected termin into DB based on Termin Object. Before storying check the DB to sure about available Termin
        /// </summary>
        /// <param name="termin">The Termin Object based on Termine Table in DB</param>
        /// <returns>The status of stored data</returns>
        [HttpPost("PostTermin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Termin))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Termin>> PostTermin(Termin termin)
        {
            try
            {
                if (termin == null || !ModelState.IsValid)
                    return StatusCode(400, $"Invalid input value.");

                // Check if the requested appointment is available
                if (!IsTerminAvailable(termin))
                    return BadRequest("This appointment is taken before, please choose another time slot.");

                _dbContext.Termine.Add(termin);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(PostTermin), new { id = termin.ID }, termin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while saving new appointment. {ex.Message}");
            }
        }


        #region Herlper Methods

        /// <summary>
        /// Check the availability of specified Termin. Before storing Termin into the DB should check redundant values.
        /// </summary>
        /// <param name="termin">Termin object to check redundancy</param>
        /// <returns>Returns bool value to show availability</returns>
        private bool IsTerminAvailable(Termin termin)
        {
            try
            {
                if (_dbContext.Database == null)
                    return false;

                // Check if the requested appointment overlaps with existing appointments
                bool isOverlapping = _dbContext.Termine.Any(t =>
                    t.Start < termin.Ende &&
                    t.Ende > termin.Start &&
                    t.BeraterID == termin.BeraterID);

                return !isOverlapping;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Generates all available time slots based on required time and duration. As we have at least 15 min for each Termin, we check each 15 min
        /// </summary>
        /// <param name="startTime">Start time of Slot</param>
        /// <param name="endTime">End time of Slot</param>
        /// <param name="duration">Termin duration</param>
        /// <param name="currentAppointments">List of all available appointments</param>
        /// <returns>Returns a list of Time slots contains start time and end time</returns>
        private List<TimeSlotModel> GetAvailableTimeSlots(DateTime startTime, DateTime endTime, TimeSpan duration, List<Termin> currentAppointments)
        {
            List<TimeSlotModel> availableTimeSlots = new List<TimeSlotModel>();

            DateTime nextTime = startTime.Add(duration);

            // Iterate over the time range
            while (nextTime < endTime)
            {
                // Move to the next time slot, check each 15 minutes
                if (IsWithinBusinessHours(startTime, nextTime))
                    availableTimeSlots.Add(new TimeSlotModel(startTime, nextTime));

                startTime = startTime.AddMinutes(15);
                nextTime = startTime.Add(duration);
            }

            return availableTimeSlots.OrderBy(t => t.StartTime).ToList();
        }


        /// <summary>
        /// Check valid date and time that should be between specified value.
        /// </summary>
        /// <param name="datetime">Given Time value to check validity.</param>
        /// <returns>Returns bool value to show tha given time is in the working hours or not</returns>
        private bool IsWithinBusinessHours(DateTime startDate, DateTime endDate)
        {
            try
            {
                TimeSpan startTimeWithoutMilliseconds = new TimeSpan(startDate.TimeOfDay.Hours, startDate.TimeOfDay.Minutes, startDate.TimeOfDay.Seconds);
                TimeSpan endTimeWithoutMilliseconds = new TimeSpan(endDate.TimeOfDay.Hours, endDate.TimeOfDay.Minutes, endDate.TimeOfDay.Seconds);

                if (startDate.DayOfWeek != endDate.DayOfWeek)
                    return false;

                if (startDate.DayOfWeek == DayOfWeek.Friday)
                {
                    return startTimeWithoutMilliseconds >= new TimeSpan(8, 0, 0) && endTimeWithoutMilliseconds <= new TimeSpan(12, 0, 0);
                }
                else if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                    return false;
                else
                {
                    return (startTimeWithoutMilliseconds >= new TimeSpan(8, 0, 0) && endTimeWithoutMilliseconds <= new TimeSpan(12, 0, 0)) ||
                           (startTimeWithoutMilliseconds >= new TimeSpan(13, 0, 0) && endTimeWithoutMilliseconds <= new TimeSpan(16, 0, 0));
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// It used just for appointments of today, we should set time > now and at the beginig of each quarter. For example 08:41 => 08:45
        /// </summary>
        /// <param name="currentTime">Given Time value to decorate.</param>
        /// <returns>Returns decorated time</returns>
        private DateTime TrimGivenTime(DateTime currentTime)
        {
            if (currentTime.Minute % 15 == 0)
                return currentTime.AddSeconds(-currentTime.Second);
            return currentTime.AddMinutes(15 - (currentTime.Minute % 15)).AddSeconds(-currentTime.Second);
        }


        /// <summary>
        /// Calculates the end date and time value of given number of week 
        /// </summary>
        /// <param name="kalenderwoche">The number of week in a year must be between 1, 52</param>
        /// <returns>Returns Date and time value of last day of working week based on given week number.</returns>
        private DateTime GetEndWorkingDateOfWoche(int kalenderwoche)
        {
            DateTime today = DateTime.Today;
            int currentWoche = GetCurrentWoche();

            if (kalenderwoche == currentWoche)
            {
                DayOfWeek startOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                int diff = (7 + (today.DayOfWeek - startOfWeek)) % 7;
                DateTime startDayOfWeek = today.AddDays(-1 * diff);
                DateTime endDayOfWeek = startDayOfWeek.AddDays(6);
                return endDayOfWeek;
            }
            else
            {
                DateTime startDayOfWeek = GetStartDayOfWeekOfYear(kalenderwoche, today.Year);
                DateTime endDayOfWeek = startDayOfWeek.AddDays(6);
                return endDayOfWeek;
            }
        }


        /// <summary>
        /// Calculates the start date and time value of given number of week 
        /// </summary>
        /// <param name="kalenderwoche">The number of week in a year must be between 1, 52</param>
        /// <returns>Returns Date and time value of first day of working week based on given week number.</returns>
        private DateTime GetStartWorkingDateOfWoche(int kalenderwoche)
        {
            DateTime today = DateTime.Now;
            int currentWoche = GetCurrentWoche();
            DateTime startDayOfWeek = today;

            if (kalenderwoche == currentWoche)
                startDayOfWeek = today;
            else
                startDayOfWeek = GetStartDayOfWeekOfYear(kalenderwoche, today.Year);

            return startDayOfWeek;
        }


        /// <summary>
        /// Helper method to determine each week of year
        /// </summary>
        /// <param name="kalenderwoche">The number of week</param>
        /// <param name="year">The number of year</param>
        /// <returns>Returns Date and time value of first day of given week.</returns>
        private DateTime GetStartDayOfWeekOfYear(int kalenderwoche, int year)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)DayOfWeek.Monday - (int)jan1.DayOfWeek;

            DateTime startDayOfWeek = jan1.AddDays(daysOffset + 7 * (kalenderwoche - 1));
            return startDayOfWeek;
        }


        /// <summary>
        /// Helper method to return the current week number
        /// </summary>
        /// <returns>Returns current week number</returns>
        private int GetCurrentWoche()
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            return calendar.GetWeekOfYear(DateTime.Now, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
        }


        /// <summary>
        /// Check validity of number of week. The value should be between current week number and 52
        /// </summary>
        /// <param name="kalenderWoche">The specified number of week</param>
        /// <returns>Returns a bool value to show the validity. true=> is valid number</returns>
        private bool IsKalendarWocheValid(int kalendarwoche)
        {
            try
            {
                // Check if the given calendarwoche is equal to or greater than the current week
                return kalendarwoche >= GetCurrentWoche() && kalendarwoche <= 52;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
