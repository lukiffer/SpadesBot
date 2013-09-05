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
        // POST /{gameId}/{playerId}/blind
        [HttpPost]
        public BlindResponse Blind(string gameId, int playerId, BlindRequest request)
        {
            return new BlindResponse { blind = false };
        }

        // POST /{gameId}/{playerId}/deal
        [HttpPost]
        public DealResponse Deal(string gameId, int playerId, DealRequest request)
        {
            var cards = request.hand.Select(x => new Card(x)).ToList();
            var logic = new LogicContainer(cards) {PlayerId = playerId};
            HttpContext.Current.Cache[gameId + playerId] = logic;

            return new DealResponse { bid = new BidLogic().Bid(cards) };
        }

        // POST /{gameId}/{playerId}/bid
        [HttpPost]
        public void Bid(string gameId, int playerId, Round request)
        {
            var logic = (LogicContainer)HttpContext.Current.Cache[gameId + playerId];

            if (playerId == 1)
            {
                logic.PlayerBid = (request.player1_bid.HasValue) ? request.player1_bid.Value : 0;
                logic.PartnerBid = (request.player3_bid.HasValue) ? request.player3_bid.Value : 0;
                logic.LeftBid = (request.player2_bid.HasValue) ? request.player2_bid.Value : 0;
                logic.RightBid = (request.player4_bid.HasValue) ? request.player4_bid.Value : 0;
            }

            if (playerId == 2)
            {
                logic.PlayerBid = (request.player2_bid.HasValue) ? request.player2_bid.Value : 0;
                logic.PartnerBid = (request.player4_bid.HasValue) ? request.player4_bid.Value : 0;
                logic.LeftBid = (request.player3_bid.HasValue) ? request.player3_bid.Value : 0;
                logic.RightBid = (request.player1_bid.HasValue) ? request.player1_bid.Value : 0;
            }

            if (playerId == 3)
            {
                logic.PlayerBid = (request.player3_bid.HasValue) ? request.player3_bid.Value : 0;
                logic.PartnerBid = (request.player1_bid.HasValue) ? request.player1_bid.Value : 0;
                logic.LeftBid = (request.player4_bid.HasValue) ? request.player4_bid.Value : 0;
                logic.RightBid = (request.player2_bid.HasValue) ? request.player2_bid.Value : 0;
            }

            if (playerId == 4)
            {
                logic.PlayerBid = (request.player4_bid.HasValue) ? request.player4_bid.Value : 0;
                logic.PartnerBid = (request.player2_bid.HasValue) ? request.player2_bid.Value : 0;
                logic.LeftBid = (request.player1_bid.HasValue) ? request.player1_bid.Value : 0;
                logic.RightBid = (request.player3_bid.HasValue) ? request.player3_bid.Value : 0;
            }

            HttpContext.Current.Cache[gameId + playerId] = logic;
        }

        // POST /{gameId}/{playerId}/play
        [HttpPost]
        public PlayResponse Play(string gameId, int playerId, Book request)
        {
            var logic = (LogicContainer)HttpContext.Current.Cache[gameId + playerId];
            var play = new PlayLogic(gameId, logic, request).Play(out logic);
            
            logic.HandCards.Remove(play);
            HttpContext.Current.Application[gameId + playerId] = logic;

            return new PlayResponse { card = play.ToString() };
        }

        // POST /{gameId}/{playerId}/book
        [HttpPost]
        public void Book(string gameId, int playerId, Book request)
        {
            var logic = (LogicContainer)HttpContext.Current.Cache[gameId + playerId];
            var bookCards = new List<Card>
                                {
                                    new Card(request.player1_card),
                                    new Card(request.player2_card),
                                    new Card(request.player3_card),
                                    new Card(request.player4_card)
                                };


            Card leadCard;
            if (request.leader == 1)
                leadCard = new Card(request.player1_card);
            else if (request.leader == 2)
                leadCard = new Card(request.player2_card);
            else if (request.leader == 3)
                leadCard = new Card(request.player3_card);
            else
                leadCard = new Card(request.player4_card);

            if (leadCard.Suit != "s")
            {
                if (logic.TeamId == 1)
                {
                    if (new Card(request.player2_card).Suit == "s" || new Card(request.player4_card).Suit == "s")
                    {
                        if (!logic.OpponentTrumpedSuits.Contains(leadCard.Suit))
                            logic.OpponentTrumpedSuits.Add(leadCard.Suit);
                    }

                    if (logic.PlayerId == 1)
                    {
                        if (new Card(request.player3_card).Suit == "s")
                            if (!logic.PartnerTrumpedSuits.Contains(leadCard.Suit))
                                logic.PartnerTrumpedSuits.Add(leadCard.Suit);
                    }
                    else
                    {
                        if (new Card(request.player1_card).Suit == "s")
                            if (!logic.PartnerTrumpedSuits.Contains(leadCard.Suit))
                                logic.PartnerTrumpedSuits.Add(leadCard.Suit);
                    }
                }
                else
                {
                    if (new Card(request.player1_card).Suit == "s" || new Card(request.player3_card).Suit == "s")
                    {
                        if (!logic.OpponentTrumpedSuits.Contains(leadCard.Suit))
                            logic.OpponentTrumpedSuits.Add(leadCard.Suit);
                    }

                    if (logic.PlayerId == 2)
                    {
                        if (new Card(request.player4_card).Suit == "s")
                            if (!logic.PartnerTrumpedSuits.Contains(leadCard.Suit))
                                logic.PartnerTrumpedSuits.Add(leadCard.Suit);
                    }
                    else
                    {
                        if (new Card(request.player2_card).Suit == "s")
                            if (!logic.PartnerTrumpedSuits.Contains(leadCard.Suit))
                                logic.PartnerTrumpedSuits.Add(leadCard.Suit);
                    }
                }
            }

            logic.PlayedCards.AddRange(bookCards.Where(x => !logic.PlayedCards.Contains(x)));
            HttpContext.Current.Application[gameId + playerId] = logic;
        }
    }
}