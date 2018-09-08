using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossExchange.Controller
{
    [Route("api/Trade")]
    public class TradeController : ControllerBase
    {
        private IShareRepository _shareRepository { get; set; }
        private ITradeRepository _tradeRepository { get; set; }
        private IPortfolioRepository _portfolioRepository { get; set; }

        public TradeController(IShareRepository shareRepository,
            ITradeRepository tradeRepository,
            IPortfolioRepository portfolioRepository)
        {
            _shareRepository = shareRepository;
            _tradeRepository = tradeRepository;
            _portfolioRepository = portfolioRepository;
        }


        [HttpGet("{portfolioid}")]
        public async Task<IActionResult> GetAllTradings([FromRoute]int portFolioid)
        {
            var trade = _tradeRepository.Query().Where(x => x.PortfolioId.Equals(portFolioid));
            return Ok(trade);
        }

        /*************************************************************************************************************************************
        For a given portfolio, with all the registered shares you need to do a trade which could be either a BUY or SELL trade. For a particular trade keep following conditions in mind:
		BUY:
        a) The rate at which the shares will be bought will be the latest price in the database.
		b) The share specified should be a registered one otherwise it should be considered a bad request. 
		c) The Portfolio of the user should also be registered otherwise it should be considered a bad request. 
                
        SELL:
        a) The share should be there in the portfolio of the customer.
		b) The Portfolio of the user should be registered otherwise it should be considered a bad request. 
		c) The rate at which the shares will be sold will be the latest price in the database.
        d) The number of shares should be sufficient so that it can be sold. 
        Hint: You need to group the total shares bought and sold of a particular share and see the difference to figure out if there are sufficient quantities available for SELL. 

        *************************************************************************************************************************************/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TradeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            HourlyShareRate r = null;
            Portfolio pf = null;

            Parallel.Invoke(
                () => { r = GetLatestRate(model.Symbol); },
                () => { pf = GetPortfolio(model.PortfolioId); });

            if (r == null)
                return BadRequest("no symbol");

            if (pf == null)
                return BadRequest("no portfolio");

            if (model.Action.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                return await Sell(model, r, pf);
            else
                return await Buy(model, r, pf);
        }

        private async Task<IActionResult> Sell(TradeModel model, HourlyShareRate r, Portfolio pf)
        {
            IEnumerable<Trade> t = GetTrades(model.PortfolioId, model.Symbol);

            var aShares = 0;
            for (int i = 0; i < t.Count(); i++)
            {
                aShares += (t.ElementAt(i).Action.Equals("SELL", StringComparison.OrdinalIgnoreCase) ? -1 : 1) *
                    t.ElementAt(i).NoOfShares;
            }

            if (aShares < model.NoOfShares)
                return NotFound("not enough shares");

            var result = new Trade()
            {
                Price = model.NoOfShares * r.Rate,
                NoOfShares = model.NoOfShares,
                Action = "SELL",
                PortfolioId = model.PortfolioId,
                Symbol = model.Symbol
            };
            await _tradeRepository.InsertAsync(result);

            return Created("Trade", result);
        }

        private async Task<IActionResult> Buy(TradeModel model, HourlyShareRate r, Portfolio pf)
        {
            var result = new Trade()
            {
                Price = model.NoOfShares * r.Rate,
                NoOfShares = model.NoOfShares,
                Action = "BUY",
                PortfolioId = model.PortfolioId,
                Symbol = model.Symbol
            };
            await _tradeRepository.InsertAsync(result);

            return Created("Trade", result);
        }

        HourlyShareRate GetLatestRate(string symbol)
        {
            var share = _shareRepository.Query()
                .Where(x => x.Symbol.Equals(symbol))
                .OrderByDescending(x => x.TimeStamp)
                .FirstOrDefault();
            return share;
        }

        Portfolio GetPortfolio(int id)
        {
            var pf = _portfolioRepository.Query()
                .Where(x => x.Id.Equals(id))
                .FirstOrDefault();
            return pf;
        }
        
        IEnumerable<Trade> GetTrades(int id, string symbol)
        {
            var pf = _tradeRepository.Query()
                .Where(x => x.PortfolioId.Equals(id) && x.Symbol.Equals(symbol))
                .ToList();
            return pf;
        }
    }
}
