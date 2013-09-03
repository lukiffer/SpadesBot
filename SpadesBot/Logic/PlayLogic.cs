using System;
using System.Collections.Generic;
using System.Linq;
using SpadesBot.Helpers;
using SpadesBot.Models;

namespace SpadesBot.Logic
{
    public class PlayLogic
    {
        public Card Play(string gameId, LogicContainer logic, Book book)
        {
            var bookCards = new List<Card>();

            if (!String.IsNullOrEmpty(book.player1_card))
                bookCards.Add(new Card(book.player1_card));

            if (!String.IsNullOrEmpty(book.player2_card))
                bookCards.Add(new Card(book.player2_card));

            if (!String.IsNullOrEmpty(book.player3_card))
                bookCards.Add(new Card(book.player3_card));

            if (!String.IsNullOrEmpty(book.player4_card))
                bookCards.Add(new Card(book.player4_card));

            logic.PlayedCards.AddRange(bookCards.Where(x => !logic.PlayedCards.Contains(x)));

            if (logic.PlayerId == book.leader)
                return Lead(logic);

            return Follow(logic, book, bookCards);
        }

        private Card Lead(LogicContainer logic)
        {
            if (!logic.PlayerIsNil)
            {
                // Sure-things...
                var s1 = PlayHighestKnown(logic);
                if (s1 != null)
                    return s1;
                
                if (logic.PartnerIsNil)
                {
                    // Partner has not played, play highest card possible
                    if (logic.SpadesBroken)
                    {
                        var s2 = PlayHighestKnown(logic, "s");
                        if (s2 != null)
                            return s2;
                    }

                    var highest = logic.HandCards.OrderByDescending(x => x.Rank).First();
                    if (highest.Suit == "s" && !logic.SpadesBroken && !logic.SpadeTight)
                    {
                        return logic.HandCards
                            .Where(x => x.Suit != "s")
                            .OrderByDescending(x => x.Rank)
                            .First();
                    }
                    
                    return logic.HandCards
                        .OrderByDescending(x => x.Rank)
                        .First();
                }
            }

            //Play lowest card possible, favoring suits that partner has trumped, but opponent has not.
            if (logic.PartnerTrumpedSuits.Any())
            {
                var t1 = logic.HandCards
                    .Where(x => logic.PartnerTrumpedSuits.Contains(x.Suit))
                    .Where(x => !logic.OpponentTrumpedSuits.Contains(x.Suit))
                    .Where(x => x.Suit != "s")
                    .ToList();

                if (t1.Any())
                    return t1.OrderBy(x => x.Rank).First();

                var t2 = logic.HandCards
                    .Where(x => logic.PartnerTrumpedSuits.Contains(x.Suit))
                    .Where(x => x.Suit != "s")
                    .ToList();

                if (t2.Any())
                    return t2.OrderBy(x => x.Rank).First();
            }

            var t3 = logic.HandCards
                .Where(x => x.Suit != "s")
                .ToList();

            if (t3.Any())
                return t3.OrderBy(x => x.Rank).First();

            return logic.HandCards.OrderBy(x => x.Rank).First();
        }

        private Card Follow(LogicContainer logic, Book book, List<Card> bookCards)
        {
            Card partnerCard;
            if (logic.TeamId == 1)
            {
                if (logic.PlayerId == 1)
                    partnerCard = (String.IsNullOrEmpty(book.player3_card)) ? null : new Card(book.player3_card);
                else
                    partnerCard = (String.IsNullOrEmpty(book.player1_card)) ? null : new Card(book.player1_card);
            }
            else
            {
                if (logic.PlayerId == 2)
                    partnerCard = (String.IsNullOrEmpty(book.player4_card)) ? null : new Card(book.player4_card);
                else
                    partnerCard = (String.IsNullOrEmpty(book.player2_card)) ? null : new Card(book.player2_card);
            }

            Card leadCard;
            if (book.leader == 1)
                leadCard = new Card(book.player1_card);
            else if (book.leader == 2)
                leadCard = new Card(book.player2_card);
            else if (book.leader == 3)
                leadCard = new Card(book.player3_card);
            else
                leadCard = new Card(book.player4_card);
            
            var highCard = bookCards.Where(x => x.Suit == leadCard.Suit).OrderByDescending(x => x.Rank).First();

            if (logic.PlayerIsNil)
            {
                if (logic.HandCards.Any(x => x.Suit == leadCard.Suit))
                {
                    if (bookCards.Any(x => x.Suit == "s"))
                    {
                        // Book has been trumped, play highest card in suit.
                        return
                            logic.HandCards
                                .Where(x => x.Suit == leadCard.Suit)
                                .OrderByDescending(x => x.Rank)
                                .First();
                    }

                    var highestOnSuit =
                        bookCards.Where(x => x.Suit == leadCard.Suit).OrderByDescending(x => x.Rank).First();

                    if (logic.HandCards.Any(x => x.Suit == leadCard.Suit && x.Rank < highestOnSuit.Rank))
                    {
                        // Play highest card under the highest in the book.
                        return
                            logic.HandCards
                                .Where(x => x.Suit == leadCard.Suit)
                                .Where(x => x.Rank < highestOnSuit.Rank)
                                .OrderByDescending(x => x.Rank)
                                .First();
                    }

                    return logic.HandCards
                        .Where(x => x.Suit == leadCard.Suit)
                        .OrderBy(x => x.Rank)
                        .First();
                }
                
                // Check non-trump suits for potential problems. (e.g. 2 cards left, 1 is the ace, etc.)
                var groups = logic.HandCards.GroupBy(x => x.Suit, (key, cards) => new
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

                if (bookCards.Any(x => x.Suit == "s"))
                {
                    // Book has been trumped, dump the highest spade under 
                    var trumpCard = bookCards.Where(x => x.Suit == "s").OrderByDescending(x => x.Rank).First();
                    var t1 = logic.HandCards.Where(x => x.Suit == "s" && x.Rank < trumpCard.Rank).ToList();

                    if (t1.Any())
                        return t1.OrderByDescending(x => x.Rank).First();
                }

                if (logic.SpadeTight)
                {
                    //Play the lowest spade in your hand.
                    return logic.HandCards.OrderBy(x => x.Rank).First();
                }

                return logic.HandCards.Where(x => x.Suit != "s").OrderByDescending(x => x.Rank).First();
            }
            
            if (logic.PartnerIsNil)
            {
                if (logic.HandCards.Any(x => x.Suit == leadCard.Suit))
                {
                    if (partnerCard == null)
                    {
                        // Partner has not played yet, play high.
                        return
                            logic.HandCards.Where(x => x.Suit == leadCard.Suit).OrderByDescending(x => x.Rank).First();
                    }
                    
                    // Partner has played, check if they are on suit.
                    if (partnerCard.Suit != leadCard.Suit && partnerCard.Suit != "s")
                    {
                        if (bookCards.All(x => x.Suit != "s"))
                        {
                            // Partner is off suit and safe, take book if sure.
                            var s0 = PlayHighestKnown(logic, leadCard.Suit);
                            if (s0 != null && highCard.Rank < s0.Rank)
                                return s0;    
                        }
                    }

                    if (logic.HandCards.Any(x => x.Suit == leadCard.Suit && x.Rank > partnerCard.Rank))
                        return logic.HandCards.Where(x => x.Suit == leadCard.Suit && x.Rank > partnerCard.Rank).OrderBy(x => x.Rank).First();

                    return logic.HandCards.Where(x => x.Suit == leadCard.Suit).OrderBy(x => x.Rank).First();
                }
                
                if (logic.HandCards.Any(x => x.Suit == "s"))
                {
                    // Check if partner has trumped the lead suit
                    if (logic.PartnerTrumpedSuits.Contains(leadCard.Suit))
                    {
                        // Play highest trump.
                        return logic.HandCards.Where(x => x.Suit == "s").OrderByDescending(x => x.Rank).First();
                    }

                    // Partner has not trumped this suit yet, play low trump.
                    return logic.HandCards.Where(x => x.Suit == "s").OrderBy(x => x.Rank).First();    
                }

                return logic.HandCards.OrderByDescending(x => x.Rank).First();
            }

            if (logic.HandCards.Any(x => x.Suit == leadCard.Suit))
            {
                if (bookCards.All(x => x.Suit != "s"))
                {
                    // Book has not been trumped, play highest known if possible.
                    var s1 = PlayHighestKnown(logic, leadCard.Suit);
                    if (s1 != null && s1.Rank > highCard.Rank)
                    {
                        if (partnerCard == null || highCard.Rank != partnerCard.Rank)
                            return s1;
                    }
                }
                
                // Can't play highest, play lowest.
                return logic.HandCards.Where(x => x.Suit == leadCard.Suit).OrderBy(x => x.Rank).First();
            }

            if (logic.HandCards.Any(x => x.Suit == "s"))
            {
                if (bookCards.All(x => x.Suit != "s"))
                    return logic.HandCards.Where(x => x.Suit == "s").OrderBy(x => x.Rank).First();
                
                var highTrump = bookCards.Where(x => x.Suit == "s").OrderByDescending(x => x.Rank).First();
                if (partnerCard != null &&
                    partnerCard != highTrump)
                {
                    var higherTrump = logic.HandCards.Where(x => x.Suit == "s" && x.Rank > highTrump.Rank).ToList();
                    if (higherTrump.Any())
                        return higherTrump.OrderBy(x => x.Rank).First();    
                }
            }
            
            // Play lowest card.
            return logic.HandCards.OrderBy(x => x.Rank).First();
        }

        private Card PlayHighestKnown(LogicContainer logic, string suit = null)
        {
            var suits = new[] {"h", "c", "d"};
            if (!String.IsNullOrEmpty(suit))
                suits = new[] {suit};
            
            var randomized = suits.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var s in randomized)
            {
                if (HasHighestKnownInHand(logic, s) && !SuitHasBeenTrumped(logic, s))
                    return HighestInHand(logic, s);
            }

            return null;
        }

        private bool HasHighestKnownInHand(LogicContainer logic, string suit)
        {
            var cards = DeckFactory.CreateCardDeck()
                .Where(x => x.Suit == suit)
                .Where(x => !logic.PlayedCards.Contains(x))
                .OrderByDescending(x => x.Rank)
                .ToList();

            return cards.Count != 0 && logic.HandCards.Contains(cards.First());
        }

        private Card HighestInHand(LogicContainer logic, string suit)
        {
            return logic.HandCards
                        .Where(x => x.Suit == suit)
                        .OrderByDescending(x => x.Rank)
                        .First();
        }

        private bool SuitHasBeenTrumped(LogicContainer logic, string suit)
        {
            return logic.PartnerTrumpedSuits.Contains(suit) || logic.OpponentTrumpedSuits.Contains(suit);
        }
    }
}