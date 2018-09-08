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
    public class PortfolioControllerTests
    {
        private readonly Mock<IPortfolioRepository> _repoMock = new Mock<IPortfolioRepository>();
        private readonly PortfolioController _ctrl;

        public PortfolioControllerTests()
        {
            _ctrl = new PortfolioController(_repoMock.Object);
            var mockResultSet = (new List<Portfolio>() {
                    new Portfolio()
                    {
                        Id = 1,
                        Name = "A",
                        Trade =new List<Trade>()
                    }}).AsQueryable();

            _repoMock.Setup(x => x.Query()).Returns(mockResultSet);
            _repoMock.Setup(x => x.GetAll()).Returns(mockResultSet);
        }

        [Test]
        public async Task Post_ShouldInsertNewPortfolio()
        {
            var model = new Portfolio
            {
                Name = "C",
                Trade = new List<Trade>() { }
            };
            var result = await _ctrl.Post(model);
            Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
        }

        [Test]
        public async Task Post_ShouldThrowError()
        {
            var model = new Portfolio
            {
                Trade = new List<Trade>() { }
            };
            _ctrl.ModelState.AddModelError("", "Error");

            var result = await _ctrl.Post(model);
            Assert.NotNull(result);

            var createdResult = result as BadRequestObjectResult;
            Assert.NotNull(createdResult);
            Assert.AreEqual(400, createdResult.StatusCode);
        }

        [Test]
        public async Task Get_Portfolio()
        {
            // Act
            var result = await _ctrl.GetPortfolioInfo(1);
            Assert.NotNull(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            Assert.AreEqual(1, (okResult.Value as Portfolio).Id);
            Assert.AreEqual(200, okResult.StatusCode);
        }
    }
}
