using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SpadesBot.Logic;
using SpadesBot.Models;

namespace SpadesBot.Controllers
{
    public class SpadesController : ApiController
    {
        // POST /{gameId}/blind
        [HttpPost]
        public dynamic Blind(string gameId, BlindRequest request)
        {
            return new { blind = false };
        }

        // POST /{gameId}/deal
        [HttpPost]
        public dynamic Deal(string gameId, DealRequest request)
        {
            var cards = request.hand.Select(x => new Card(x)).ToList();
            HttpContext.Current.Cache[gameId + "_hand"] = cards;
            return new { bid = BidLogic.Bid(cards) };
        }

        // POST /{gameId}/play
        [HttpPost]
        public dynamic Play(string gameId, Book request)
        {
            var cards = (List<Card>)HttpContext.Current.Cache[gameId + "_hand"];
            
            //TODO: Replace this with actual logic.
            var card = cards[new Random().Next(0, (cards.Count - 1))];

            cards.Remove(card);
            HttpContext.Current.Cache[gameId + "_hand"] = cards;

            return new { card = card.ToString() };
        }

        // POST /{gameId}/book
        [HttpPost]
        public void Book(string gameId, Book request)
        {
            
        }
    }
}