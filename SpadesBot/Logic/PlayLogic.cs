using System;
using System.Collections.Generic;
using System.Linq;
using SpadesBot.Helpers;
using SpadesBot.Models;

namespace SpadesBot.Logic
{
    public class PlayLogic
    {
        private string GameId { get; set; }
        private LogicContainer Logic { get; set; }
        private Book Book { get; set; }
        private List<Card> BookCards { get; set; }

        private Card LeadCard { get; set; }
        private Card HighCard { get; set; }

        private bool CanFollowSuit { get; set; }

        private Card PartnerCard { get; set; }
        public bool PartnerWinning { get; set; }
        
        public PlayLogic(string gameId, LogicContainer logic, Book book)
        {
            GameId = gameId;
            Logic = logic;
            Book = book;

            BookCards = new List<Card>();

            if (!String.IsNullOrEmpty(Book.player1_card))
                BookCards.Add(new Card(Book.player1_card));

            if (!String.IsNullOrEmpty(Book.player2_card))
                BookCards.Add(new Card(Book.player2_card));

            if (!String.IsNullOrEmpty(Book.player3_card))
                BookCards.Add(new Card(Book.player3_card));

            if (!String.IsNullOrEmpty(Book.player4_card))
                BookCards.Add(new Card(Book.player4_card));

            Logic.PlayedCards.AddRange(
                BookCards.Where(x => !Logic.PlayedCards.Contains(x)));

            if (BookCards.Any())
            {
                if (Book.leader == 1)
                    LeadCard = new Card(Book.player1_card);
                else if (Book.leader == 2)
                    LeadCard = new Card(Book.player2_card);
                else if (Book.leader == 3)
                    LeadCard = new Card(Book.player3_card);
                else
                    LeadCard = new Card(Book.player4_card);    
            
                HighCard = BookCards
                    .Where(x => x.Suit == LeadCard.Suit)
                    .OrderByDescending(x => x.Rank)
                    .First();

                CanFollowSuit = Logic.HandCards.Any(x => x.Suit == LeadCard.Suit);
            }

            switch (Logic.PlayerId)
            {
                case 1:
                    PartnerCard = (String.IsNullOrEmpty(Book.player3_card)) 
                        ? null
                        : new Card(Book.player3_card);
                    break;
                case 2:
                    PartnerCard = (String.IsNullOrEmpty(Book.player4_card)) 
                        ? null
                        : new Card(Book.player4_card);
                    break;
                case 3:
                    PartnerCard = (String.IsNullOrEmpty(Book.player1_card)) 
                        ? null
                        : new Card(Book.player1_card);
                    break;
                case 4:
                    PartnerCard = (String.IsNullOrEmpty(Book.player2_card)) 
                        ? null
                        : new Card(Book.player2_card);
                    break;
            }

            if (PartnerCard != null)
            {
                PartnerWinning = BookCards.Any(x => x.Suit == "s")
                                     ? BookCards.Where(x => x.Suit == "s")
                                        .OrderByDescending(x => x.Rank)
                                        .First() == PartnerCard
                                     : HighCard == PartnerCard;
            }
        }

        public Card Play(out LogicContainer logic)
        {
            logic = this.Logic;
            return Logic.PlayerId == Book.leader ? Lead() : Follow();
        }

        private Card Lead()
        {
            if (!Logic.PlayerIsNil)
            {
                // Sure-things...
                var s1 = PlayHighestKnown();
                if (s1 != null)
                    return s1;
                
                if (Logic.PartnerIsNil)
                {
                    // Partner has not played, play highest card possible
                    if (Logic.SpadesBroken)
                    {
                        var s2 = PlayHighestKnown("s");
                        if (s2 != null)
                            return s2;
                    }

                    var highest = Logic.HandCards.OrderByDescending(x => x.Rank).First();
                    if (highest.Suit == "s" && !Logic.SpadesBroken && !Logic.SpadeTight)
                    {
                        return Logic.HandCards
                            .Where(x => x.Suit != "s")
                            .OrderByDescending(x => x.Rank)
                            .First();
                    }
                    
                    return Logic.HandCards
                        .OrderByDescending(x => x.Rank)
                        .First();
                }
            }

            //Play lowest card possible, favoring suits that partner has trumped, but opponent has not.
            if (Logic.PartnerTrumpedSuits.Any())
            {
                var t1 = Logic.HandCards
                    .Where(x => Logic.PartnerTrumpedSuits.Contains(x.Suit))
                    .Where(x => !Logic.OpponentTrumpedSuits.Contains(x.Suit))
                    .Where(x => x.Suit != "s")
                    .ToList();

                if (t1.Any())
                    return t1.OrderBy(x => x.Rank).First();

                var t2 = Logic.HandCards
                    .Where(x => Logic.PartnerTrumpedSuits.Contains(x.Suit))
                    .Where(x => x.Suit != "s")
                    .ToList();

                if (t2.Any())
                    return t2.OrderBy(x => x.Rank).First();
            }

            var t3 = Logic.HandCards
                .Where(x => x.Suit != "s")
                .ToList();

            if (t3.Any())
                return t3.OrderBy(x => x.Rank).First();

            return Logic.HandCards.OrderBy(x => x.Rank).First();
        }

        private Card Follow()
        {
            if (Logic.PlayerIsNil)
                return (CanFollowSuit)
                    ? Play_PlayerNil_OnSuit()
                    : Play_PlayerNil_OffSuit();

            if (Logic.PartnerIsNil)
                return (CanFollowSuit)
                    ? Play_PartnerNil_OnSuit()
                    : Play_PartnerNil_OffSuit();

            return (CanFollowSuit)
                ? Play_OnSuit()
                : Play_OffSuit();
        }

        private Card Play_PlayerNil_OnSuit()
        {
            if (BookCards.Any(x => x.Suit == "s"))
            {
                // Book has been trumped, play highest card in suit.
                return Logic.HandCards
                    .Where(x => x.Suit == LeadCard.Suit)
                    .OrderByDescending(x => x.Rank)
                    .First();
            }

            var highestOnSuit =
                BookCards.Where(x => x.Suit == LeadCard.Suit).OrderByDescending(x => x.Rank).First();

            if (Logic.HandCards.Any(x => x.Suit == LeadCard.Suit && x.Rank < highestOnSuit.Rank))
            {
                // Play highest card under the highest in the book.
                return Logic.HandCards
                    .Where(x => x.Suit == LeadCard.Suit)
                    .Where(x => x.Rank < highestOnSuit.Rank)
                    .OrderByDescending(x => x.Rank)
                    .First();
            }

            return Logic.HandCards
                .Where(x => x.Suit == LeadCard.Suit)
                .OrderBy(x => x.Rank)
                .First();
        }

        private Card Play_PlayerNil_OffSuit()
        {
            // Check non-trump suits for potential problems. (e.g. 2 cards left, 1 is the ace, etc.)
            var groups = Logic.HandCards.GroupBy(x => x.Suit, (key, cards) => new
            {
                suit = key,
                cards = cards.ToList()
            });

            var problems = groups.Where(x => x.suit != "s" &&
                                             x.cards.Count() <= 2 &&
                                             x.cards.Any(y => y.Rank > 10))
                                             .ToList();

            if (problems.Any())
            {
                var problem = problems.OrderBy(x => x.cards.Count()).First();
                return problem.cards.OrderByDescending(x => x.Rank).First();
            }

            if (BookCards.Any(x => x.Suit == "s"))
            {
                // Book has been trumped, dump the highest spade under 
                var trumpCard = BookCards.Where(x => x.Suit == "s").OrderByDescending(x => x.Rank).First();
                var t1 = Logic.HandCards.Where(x => x.Suit == "s" && x.Rank < trumpCard.Rank).ToList();

                if (t1.Any())
                    return t1.OrderByDescending(x => x.Rank).First();
            }

            if (Logic.HandCards.All(x => x.Suit == "s"))
            {
                //Play the lowest spade in your hand.
                return Logic.HandCards.OrderBy(x => x.Rank).First();
            }

            return Logic.HandCards.Where(x => x.Suit != "s").OrderByDescending(x => x.Rank).First();
        }

        private Card Play_PartnerNil_OnSuit()
        {
            // If partner has not yet played, play high.
            if (PartnerCard == null)
                return Logic.HandCards
                    .Where(x => x.Suit == LeadCard.Suit)
                    .OrderByDescending(x => x.Rank)
                    .First();

            // Partner has played, check if they are on suit.
            if (PartnerCard.Suit != LeadCard.Suit && PartnerCard.Suit != "s")
            {
                // Partner is off suit and safe, take book if sure.
                if (BookCards.All(x => x.Suit != "s"))
                {
                    var s0 = PlayHighestKnown(LeadCard.Suit);
                    if (s0 != null && HighCard.Rank < s0.Rank)
                        return s0;
                }
            }

            if (Logic.HandCards.Any(x => x.Suit == LeadCard.Suit && x.Rank > PartnerCard.Rank))
                return Logic.HandCards
                    .Where(x => x.Suit == LeadCard.Suit && 
                        x.Rank > PartnerCard.Rank)
                    .OrderBy(x => x.Rank)
                    .First();

            return Logic.HandCards
                .Where(x => x.Suit == LeadCard.Suit)
                .OrderBy(x => x.Rank).First();
        }

        private Card Play_PartnerNil_OffSuit()
        {
            if (Logic.HandCards.Any(x => x.Suit == "s"))
            {
                // Check if partner has trumped the lead suit, if so play highest trump.
                if (Logic.PartnerTrumpedSuits.Contains(LeadCard.Suit))
                    return Logic.HandCards
                        .Where(x => x.Suit == "s")
                        .OrderByDescending(x => x.Rank)
                        .First();

                // Partner has not trumped this suit yet, play low trump.
                return Logic.HandCards
                    .Where(x => x.Suit == "s")
                    .OrderBy(x => x.Rank)
                    .First();
            }

            return Logic.HandCards.OrderByDescending(x => x.Rank).First();
        }

        private Card Play_OnSuit()
        {
            if (BookCards.All(x => x.Suit != "s")) // Book has not been trumped
            {
                if (!PartnerWinning)
                {
                    if (BookCards.Count == 3 || (BookCards.Count == 2 && Logic.LeftBid <= 0))
                    {
                        // All other players have played a card, opposing team is winning.

                        var overCards =
                            Logic.HandCards.Where(x => x.Suit == LeadCard.Suit && x.Rank > HighCard.Rank).ToList();

                        // Play lowest card over the high card if possible.
                        if (overCards.Any())
                            return overCards.OrderBy(x => x.Rank).First();
                    }

                    var s1 = PlayHighestKnown(LeadCard.Suit);
                    if (s1 != null && s1.Rank > HighCard.Rank)
                    {
                        if (PartnerCard == null || HighCard.Rank != PartnerCard.Rank)
                            return s1;
                    }
                }

                if (BookCards.Count == 2) // Partner is winning and opposing team will play after you.
                {
                    // Check if your partner has the highest possible card (excluding your hand cards).
                    var s2 = PlayHighestKnown(LeadCard.Suit);
                    if (s2 != null && PartnerCard != null && s2.Rank > PartnerCard.Rank)
                    {
                        var partnerIsHighestKnown = true;

                        for (var i = PartnerCard.Rank; i < s2.Rank; i++)
                        {
                            var higherCard = new Card(i, LeadCard.Suit);
                            if (!Logic.PlayedCards.Contains(higherCard) &&
                                !Logic.HandCards.Contains(higherCard))
                                partnerIsHighestKnown = false;
                        }

                        // If a higher card exists than your partners play, but is not accounted for, play highest known.
                        if (!partnerIsHighestKnown)
                            return s2;
                    }
                }
            }

            // Can't play higher, play lowest.
            return Logic.HandCards.Where(x => x.Suit == LeadCard.Suit).OrderBy(x => x.Rank).First();
        }

        private Card Play_OffSuit()
        {
            if (PartnerWinning && (BookCards.Count == 3 || (BookCards.Count == 2 && Logic.LeftBid <= 0)))
            {
                // Partner is winning and no threats remain, sluff.
                var nonTrump = Logic.HandCards.Where(x => x.Suit != "s").OrderBy(x => x.Rank).ToList();
                if (nonTrump.Any())
                    return nonTrump.First();

                // You're spade tight, return the lowest one.
                return Logic.HandCards.OrderBy(x => x.Rank).First();
            }

            if (Logic.HandCards.Any(x => x.Suit == "s"))
            {
                if (BookCards.Any(x => x.Suit == "s"))
                {
                    var highTrump = BookCards.Where(x => x.Suit == "s").OrderByDescending(x => x.Rank).First();
                    if (PartnerCard != null &&
                    PartnerCard != highTrump)
                    {
                        var higherTrump = Logic.HandCards.Where(x => x.Suit == "s" && x.Rank > highTrump.Rank).ToList();
                        if (higherTrump.Any())
                            return higherTrump.OrderBy(x => x.Rank).First();
                    }
                }

                return Logic.HandCards
                    .Where(x => x.Suit == "s")
                    .OrderBy(x => x.Rank)
                    .First();
            }

            // Play lowest card.
            return Logic.HandCards.OrderBy(x => x.Rank).First();
        }

        private Card PlayHighestKnown(string suit = null)
        {
            var suits = new[] {"h", "c", "d"};
            if (!String.IsNullOrEmpty(suit))
                suits = new[] {suit};
            
            var randomized = suits.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var s in randomized)
            {
                if (HasHighestKnownInHand(s) && !SuitHasBeenTrumped(s))
                    return HighestInHand(s);
            }

            return null;
        }

        private bool HasHighestKnownInHand(string suit)
        {
            var cards = DeckFactory.CreateCardDeck()
                .Where(x => x.Suit == suit)
                .Where(x => !Logic.PlayedCards.Contains(x))
                .OrderByDescending(x => x.Rank)
                .ToList();

            return cards.Count != 0 && Logic.HandCards.Contains(cards.First());
        }

        private Card HighestInHand(string suit)
        {
            return Logic.HandCards
                        .Where(x => x.Suit == suit)
                        .OrderByDescending(x => x.Rank)
                        .First();
        }

        private bool SuitHasBeenTrumped(string suit)
        {
            return Logic.PartnerTrumpedSuits.Contains(suit) || Logic.OpponentTrumpedSuits.Contains(suit);
        }
    }
}