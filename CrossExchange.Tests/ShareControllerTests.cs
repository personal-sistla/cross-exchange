using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace CrossExchange.Tests
{
    public class ShareControllerTests
    {
        private readonly Mock<IShareRepository> _shareRepositoryMock = new Mock<IShareRepository>();

        private readonly ShareController _shareController;

        public ShareControllerTests()
        {
            _shareController = new ShareController(_shareRepositoryMock.Object);
            var mockResultSet = (new List<HourlyShareRate>() {
                    new HourlyShareRate()
                    {
                        Id = 1,
                        Rate = 1.5m,
                        Symbol = "A",
                        TimeStamp = DateTime.Now.AddHours(1)
                    },new HourlyShareRate()
                    {
                        Id = 2,
                        Rate = 1,
                        Symbol = "A",
                        TimeStamp = DateTime.Now
                    }
                }).AsQueryable();

            _shareRepositoryMock.Setup(x => x.Query()).Returns(mockResultSet);
        }

        [Test]
        public async Task Post_ShouldInsertHourlySharePrice()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };
            var result = await _shareController.Post(hourRate);
            Assert.NotNull(result);
            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        [Test]
        public async Task Post_ShouldThrowError()
        {
            var hourRate = new HourlyShareRate
            {
                Symbol = "CBI",
                Rate = 330.0M,
                TimeStamp = new DateTime(2018, 08, 17, 5, 0, 0)
            };

            _shareController.ModelState.AddModelError("", "Error");
            var result = await _shareController.Post(hourRate);

            Assert.NotNull(result);

            var createdResult = result as BadRequestObjectResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(400, createdResult.StatusCode);
        }

        [Test]
        public async Task Get_ShouldGetSharePrices()
        {
            // Act
            var result = await _shareController.Get("A");
            Assert.NotNull(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            Assert.NotNull((okResult.Value as IEnumerable<HourlyShareRate>));
            Assert.AreEqual(2, (okResult.Value as IEnumerable<HourlyShareRate>).Count());
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task Get_ShouldGetSharePricesLatest()
        {
            var result = await _shareController.GetLatestPrice("A");
            Assert.NotNull(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            Assert.AreEqual(1.5m, okResult.Value);
            Assert.AreEqual(200, okResult.StatusCode);
        }
    }
}
