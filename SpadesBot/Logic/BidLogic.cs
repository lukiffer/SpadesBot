using System;
using System.Collections.Generic;
using System.Linq;
using SpadesBot.Models;

namespace SpadesBot.Logic
{
    public class BidLogic
    {
        public int Bid(List<Card> hand)
        {
            var trump = hand.Count(x => x.Suit == "s");
            var marked = 0;

            var clubs = ProcessSuit(hand.Where(x => x.Suit == "c").ToList(), trump);
            marked += clubs.marked;

            var diamonds = ProcessSuit(hand.Where(x => x.Suit == "d").ToList(), (trump - marked));
            marked += diamonds.marked;

            var hearts = ProcessSuit(hand.Where(x => x.Suit == "h").ToList(), (trump - marked));
            marked += hearts.marked;

            var spades = ProcessSuit(
                hand.Where(x => x.Suit == "s")
                    .OrderBy(x => x.Rank)
                    .Skip(marked)
                    .Take(13)
                    .ToList(), 0);

            var bid = clubs.projected + diamonds.projected + hearts.projected + spades.projected;

            // Nil Guestimate...
            if (bid < 1.5m && !hand.Any(x => x.Rank > 11) && trump < 3)
                return 0;

            if (hand.Count(x => x.Rank > 9 && x.Suit == "s") > 2)
                bid += 0.33m;

            //Agressiveness factor.
            bid += 0.33m;
            return Convert.ToInt32(Math.Round(bid, MidpointRounding.AwayFromZero));
        }

        private dynamic ProcessSuit(List<Card> cards, int trump)
        {
            var projected = 0m;
            var marked = 0m;

            if (cards.Any(x => x.Rank == 14))
            {
                if (cards.Count < 6)
                    projected += 1.00m;
                else if (cards.Count < 7)
                    projected += 0.5m;
            }

            if (cards.Any(x => x.Rank == 13))
                if (cards.Count < 4 && cards.Count > 1)
                    projected += 1m;
                else if (cards.Count < 5 && cards.Count > 1)
                    projected += 0.5m;
                else if (cards.Count < 6 && cards.Count > 1)
                    projected += 0.33m;

            if (cards.Any(x => x.Rank == 12) && (cards.Count < 5 && cards.Count > 3))
                projected += 0.33m;

            if (cards.Count == 2)
                if (trump > 2)
                {
                    projected += 1m;
                    marked += 1m;
                }
                else if (trump > 1)
                {
                    projected += 0.66m;
                    marked += 0.66m;
                }

            if (cards.Count == 1)
                if (trump > 2)
                {
                    projected += 1.66m;
                    marked += 1.66m;
                }
                else if (trump > 1)
                {
                    projected += 1.00m;
                    marked += 1.00m;
                }

            if (cards.Count == 0)
                if (trump > 2)
                {
                    projected += 2.33m;
                    marked += 2.33m;
                }
                else if (trump > 1)
                {
                    projected += 1.66m;
                    marked += 1.66m;
                }
                else if (trump == 1)
                {
                    projected += 1.00m;
                    marked += 1.00m;
                }

            return new
            {
                projected,
                marked
            };
        }
    }
}