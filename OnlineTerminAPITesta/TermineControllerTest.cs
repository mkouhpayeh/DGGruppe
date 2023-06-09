namespace OnlineTerminAPITests
{
    using Microsoft.AspNetCore.Mvc;
    using OnlineBeratungstermin.Controllers;
    using OnlineBeratungstermin.Helpers;
    using OnlineBeratungstermin.Models;
    using System.Threading.Tasks;
    using Xunit;
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    
    [Collection("Database Collection")]
    public class TermineControllerTest
    {
        private DatabaseFixture _fixture;
        private DbContext _dbContext;

        private readonly ILogger<TermineController> _logger;

        public TermineControllerTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _dbContext = fixture._dbContext;
        }

        #region GetTermine
        [Fact]
        public async Task GetTermine_ReturnsOkResultWithAvailableTimeSlots()
        {
            var controller = new TermineController((OnlineTermineDbContext)_dbContext, _logger);

            // Arrange
            int kalenderWoche = 23;
            int terminartID = 1;

            // Act
            var result = await controller.GetTermine(kalenderWoche, terminartID);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsType<List<AvailableTerminModel>>(okResult.Value);

            var availableTimeSlots = okResult.Value as List<AvailableTerminModel>;
            Assert.NotEmpty(availableTimeSlots);
        }

        #endregion

        #region PostTermin

        [Fact]
        public async Task PostTermin_ValidTermin_ReturnsCreatedAction()
        {
            // Arrange
            var termin = new Termin
            {
                Start = new DateTime(2023, 6, 12, 10, 30, 0),
                Ende = new DateTime(2023, 6, 12, 11, 0, 0),
                BeraterID = 1,
                KundenEmail = "Test@gmai.com",
                KundenName = "Peter.K",
                TerminartID = 2,
                KundenVertragsnummer = "77",
                KundenVertragsGesamtbeitrag = 34.6M
            };

            var controller = new TermineController((OnlineTermineDbContext)_dbContext, _logger);

            // Act
            var result = await controller.PostTermin(termin);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdTermin = Assert.IsType<Termin>(createdAtActionResult.Value);
            Assert.Equal(termin.ID, createdTermin.ID);
        }

        [Fact]
        public async Task PostTermin_UnavailableTermin_ReturnsBadRequest()
        {
            // Arrange
            var termin = new Termin
            {
                Start = new DateTime(2023, 6, 12, 8, 0, 0),
                Ende = new DateTime(2023, 6, 12, 8, 15, 0),
                BeraterID = 1,
                KundenEmail = "Test@gmail.com",
                KundenName = "Peter.K",
                TerminartID = 1,
                KundenVertragsnummer = "77",
                KundenVertragsGesamtbeitrag = 34.6M
            };
            var controller = new TermineController((OnlineTermineDbContext)_dbContext, _logger);

            // Act
            var result = await controller.PostTermin(termin);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("This appointment is taken before, please choose another time slot.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTermin_ExceptionThrown_Null400()
        {
            // Arrange


            var controller = new TermineController((OnlineTermineDbContext)_dbContext, _logger);

            // Act
            var result = await controller.PostTermin(null);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, statusCodeResult.StatusCode);
            Assert.Contains("Invalid input value.", "Invalid input value.");
        }


        #endregion
    }
}