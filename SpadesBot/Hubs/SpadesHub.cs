using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using SpadesBot.Helpers;
using SpadesBot.Models;

namespace SpadesBot.Hubs
{
    public class SpadesHub : Hub
    {
        public void Start(Config config)
        {
            var game = new Game
                           {
                               id = Guid.NewGuid(),
                               dealer = 1,
                               leader = 2,
                               team1_score = 0,
                               team1_bags = 0,
                               team2_score = 0,
                               team2_bags = 0
                           };

            var client = new AjaxClient(config);

            while (game.team1_score < config.limit &&
                   game.team2_score < config.limit)
            {
                Round(client, game);
            }

            Clients.Caller.gameComplete(game);
        }

        private void Round(AjaxClient client, Game game)
        {
            Clients.Caller.roundStart();

            var hands = Deal();
            var round = new Round();

            #region BIDDING

            for (var i = game.leader; i < (game.leader + 4); i++)
            {
                var p = (i > 4) ? (i - 4) : i;
                int bid;

                if (client.Blind(p, round, game))
                {
                    bid = -1;
                    client.Deal(p, hands[p], round, game);
                }
                else
                {
                    bid = client.Deal(p, hands[p], round, game);
                    if (bid < 0 || bid > 13)
                        Clients.Caller.log("WARNING: Player " + p + " has made an illegal bid.");
                }
                
                switch (p)
                {
                    case 1:
                        round.player1_bid = bid;
                        break;
                    case 2:
                        round.player2_bid = bid;
                        break;
                    case 3:
                        round.player3_bid = bid;
                        break;
                    case 4:
                        round.player4_bid = bid;
                        break;
                }

                Clients.Caller.bid(p, bid);
                Thread.Sleep(1000);
            }

            #endregion

            #region PLAYING

            var books = new List<Book>();
            var leader = game.leader;

            for (var i = 0; i < 13; i++)
            {
                var book = new Book { leader = leader };

                for (var j = leader; j < (leader + 4); j++)
                {
                    var p = (j > 4) ? (j - 4) : j;
                    var card = client.Play(p, book, round, game);

                    //TODO: VALIDATE THAT THE CARD RETURNED IS ACTUALLY IN PLAYERS HAND.
                    //TODO: VALIDATE THAT THE CARD CAN BE PLAYED (PLAYER IS FOLLOWING SUIT IF POSSIBLE + SPADES HAVE/HAVENT BEEN BROKEN)

                    if (p == 1)
                        book.player1_card = card;
                    if (p == 2)
                        book.player2_card = card;
                    if (p == 3)
                        book.player3_card = card;
                    if (p == 4)
                        book.player4_card = card;

                    Clients.Caller.play(p, card);

                    Thread.Sleep(300);
                }

                ScoreBook(book);
                switch (book.winner)
                {
                    case 1:
                        round.player1_taken += 1;
                        break;
                    case 2:
                        round.player2_taken += 1;
                        break;
                    case 3:
                        round.player3_taken += 1;
                        break;
                    case 4:
                        round.player4_taken += 1;
                        break;
                }

                for (var p = 1; p <= 4; p++)
                {
                    client.Book(p, book, game);
                }

                leader = book.winner;
                books.Add(book);

                Thread.Sleep(1000);
                Clients.Caller.bookComplete(book, round, game);
                Thread.Sleep(1000);
            }

            #endregion

            #region SCORING

            if (round.player1_bid.HasValue && 
                round.player2_bid.HasValue &&
                round.player3_bid.HasValue &&
                round.player4_bid.HasValue)
            {
                var t1 = ScoreRound(
                    round.player1_bid.Value, round.player1_taken, 
                    round.player3_bid.Value, round.player3_taken);
                var t2 = ScoreRound(
                    round.player2_bid.Value, round.player2_taken, 
                    round.player4_bid.Value, round.player4_taken);

                game.team1_score += t1.Score;
                Bag(1, t1.Bags, game);
                game.team2_score += t2.Score;
                Bag(2, t2.Bags, game);
            }
            else
            {
                Clients.Caller.log("WARNING: One or more player bids were lost.");
            }
            
            #endregion

            game.dealer = (game.dealer == 4) ? 1 : (game.dealer + 1);
            game.leader = (game.leader == 4) ? 1 : (game.leader + 1);

            Clients.Caller.roundComplete(round, game);
            Thread.Sleep(1000);
        }

        private Dictionary<int, List<string>> Deal()
        {
            var deck = DeckFactory.CreateStringDeck();
            var result = new Dictionary<int, List<string>>();

            for (var p = 1; p <= 4; p++)
            {
                result[p] = new List<string>();

                for (var i = 0; i < 13; i++)
                    result[p].Add(PickRandom(deck));    
            }

            return result;
        }

        private string PickRandom(List<string> deck)
        {
            var card = deck[new Random().Next(0, deck.Count - 1)];
            deck.Remove(card);
            return card;
        }

        private void ScoreBook(Book book)
        {
            var cards = new List<Card>
                            {
                                new Card(book.player1_card),
                                new Card(book.player2_card),
                                new Card(book.player3_card),
                                new Card(book.player4_card)
                            };

            Card leadCard;
            switch (book.leader)
            {
                case 1:
                    leadCard = new Card(book.player1_card);
                    break;
                case 2:
                    leadCard = new Card(book.player2_card);
                    break;
                case 3:
                    leadCard = new Card(book.player3_card);
                    break;
                default:
                    leadCard = new Card(book.player4_card);
                    break;
            }
            
            var highestSuit = leadCard.Suit;

            if (cards.Any(x => x.Suit == "s"))
                highestSuit = "s";

            var winner = cards.Where(x => x.Suit == highestSuit).OrderByDescending(x => x.Rank).First();


            if (winner.ToString() == book.player1_card)
                book.winner = 1;

            if (winner.ToString() == book.player2_card)
                book.winner = 2;

            if (winner.ToString() == book.player3_card)
                book.winner = 3;

            if (winner.ToString() == book.player4_card)
                book.winner = 4;
        }

        private dynamic ScoreRound(int xBid, int xTaken, int yBid, int yTaken)
        {
            var score = 0;
            var bags = 0;

            if (xBid == 0 || xBid == -1)
                score += Nil(xBid, xTaken);

            if (yBid == 0 || yBid == -1)
                score += Nil(yBid, yTaken);

            var totalBid = (xBid > 0 ? xBid : 0) + (yBid > 0 ? yBid : 0);
            var totalTaken = xTaken + yTaken;

            if (totalBid > totalTaken)
                score -= totalBid*10;
            else
            {
                bags = totalTaken - totalBid;
                score += (totalBid*10) + bags;
            }

            return new
                       {
                           Score = score,
                           Bags = bags
                       };
        }

        private int Nil(int bid, int taken)
        {
            if (bid == 0)
            {
                if (taken == 0)
                    return 100;
                return -100;
            }
            
            if (bid == -1)
            {
                if (taken == 0)
                    return 200;

                return -200;
            }

            return 0;
        }

        private void Bag(int team, int bags, Game game)
        {
            var count = bags;

            if (team == 1)
                count += game.team1_bags;

            if (team == 2)
                count += game.team2_bags;

            if (count >= 10)
            {
                var penalty = (count - (count%10))*10;

                if (team == 1)
                {
                    game.team1_score -= penalty;
                    game.team1_bags = (count%10);
                }

                if (team == 2)
                {
                    game.team2_score -= penalty;
                    game.team2_bags = (count%10);
                }
            }
            else
            {
                if (team == 1)
                    game.team1_bags = count;

                if (team == 2)
                    game.team2_bags = count;
            }
        }
    }
}