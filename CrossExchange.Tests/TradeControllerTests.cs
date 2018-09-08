using System;
using System.Threading.Tasks;
using CrossExchange.Controller;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CrossExchange.Tests
{
    [TestFixture]
    public class TradeControllerTests
    {
        private Mock<IShareRepository> _shareRepoMock = new Mock<IShareRepository>();
        private Mock<ITradeRepository> _tradeRepoMock = new Mock<ITradeRepository>();
        private Mock<IPortfolioRepository> _pfRepoMock = new Mock<IPortfolioRepository>();
        private TradeController _tradeCtrl;

        [SetUp]
        public void Setup()
        {
            _tradeCtrl = new TradeController(_shareRepoMock.Object,
                _tradeRepoMock.Object,
                _pfRepoMock.Object);

            var mockShareRepo = (new List<HourlyShareRate>() {
                    new HourlyShareRate()
                    {
                        Id = 1,
                        Rate = 1m,
                        Symbol = "A",
                        TimeStamp = DateTime.Now.AddHours(1)
                    }}).AsQueryable();

            var mockPfRepo = (new List<Portfolio>() {
                    new Portfolio()
                    {
                        Id = 1,
                        Name = "A",
                        Trade =new List<Trade>()
                    }}).AsQueryable();

            var mockTrRepo = (new List<Trade>() {
                    new Trade()
                    {
                        Id = 1,
                        PortfolioId = 1,
                        Symbol = "A",
                        Action = "BUY",
                        NoOfShares = 100,
                        Price = 100
                    }}).AsQueryable();

            _shareRepoMock.Setup(x => x.Query()).Returns(mockShareRepo);
            _pfRepoMock.Setup(x => x.Query()).Returns(mockPfRepo);
            _tradeRepoMock.Setup(x => x.Query()).Returns(mockTrRepo);
        }

        [Test]
        public async Task Get_Portfolio()
        {
            var result = await _tradeCtrl.GetAllTradings(1);
            Assert.NotNull(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value as IEnumerable<Trade>);
            Assert.NotNull((okResult.Value as IEnumerable<Trade>).Count());
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [Test]
        public async Task Post_InvalidTradeModel()
        {
            _tradeCtrl.ModelState.AddModelError("", "Error");
            var result = await _tradeCtrl.Post(new TradeModel());
            Assert.NotNull(result);

            var badResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badResult.StatusCode);
        }

        [Test]
        public async Task Post_InvalidPortfolio()
        {
            var result = await _tradeCtrl.Post(new TradeModel()
            {
                Action = "BUY",
                PortfolioId = 2,
                Symbol = "A"
            });
            Assert.NotNull(result);

            var badResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badResult.StatusCode);
            Assert.AreEqual("no portfolio", badResult.Value);
        }

        [Test]
        public async Task Post_InvalidSymbol()
        {
            var result = await _tradeCtrl.Post(new TradeModel()
            {
                Action = "BUY",
                PortfolioId = 1,
                Symbol = "C"
            });
            Assert.NotNull(result);

            var badResult = result as BadRequestObjectResult;
            Assert.AreEqual(400, badResult.StatusCode);
            Assert.AreEqual("no symbol", badResult.Value);
        }

        [Test]
        public async Task Post_SellMoreThanHaving()
        {
            var result = await _tradeCtrl.Post(new TradeModel()
            {
                Action = "SELL",
                PortfolioId = 1,
                Symbol = "A",
                NoOfShares = 1000,
            });

            Assert.NotNull(result);

            var okResult = result as NotFoundObjectResult;
            Assert.AreEqual(404, okResult.StatusCode);
            Assert.AreEqual("not enough shares", okResult.Value);
        }

        [Test]
        public async Task Post_SellMin()
        {
            var result = await _tradeCtrl.Post(new TradeModel()
            {
                Action = "SELL",
                PortfolioId = 1,
                Symbol = "A",
                NoOfShares = 1,
            });

            Assert.NotNull(result);

            var okResult = result as CreatedResult;
            Assert.AreEqual(201, okResult.StatusCode);
        }

        [Test]
        public async Task Post_Buy()
        {
            var result = await _tradeCtrl.Post(new TradeModel()
            {
                Action = "BUY",
                PortfolioId = 1,
                Symbol = "A",
                NoOfShares = 10,
            });

            Assert.NotNull(result);

            var okResult = result as CreatedResult;
            Assert.AreEqual(201, okResult.StatusCode);
        }
    }
}