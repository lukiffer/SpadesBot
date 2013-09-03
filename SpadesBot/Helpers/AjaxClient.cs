using System;
using System.Collections.Generic;
using System.Text;
using SlimRest.Client;
using SlimRest.Request;
using SpadesBot.Models;

namespace SpadesBot.Helpers
{
    public class AjaxClient
    {
        private readonly Config _config;
        public AjaxClient(Config config)
        {
            _config = config;
        }

        public bool Blind(int player, Round round, Game game)
        {
            var client = new RestClient(GetBaseUrl(game, player));
            var result = client.Post<BlindResponse, dynamic>(new RestDataRequest<dynamic>("blind")
                                     {
                                         Data = new
                                                    {
                                                        round = round,
                                                        game = game
                                                    }
                                     });
            return result.blind;
        }

        public int Deal(int player, List<string> hand, Round round, Game game)
        {
            var client = new RestClient(GetBaseUrl(game, player));
            var result = new DealResponse { bid = 0 };
            try
            {
                result = client.Post<DealResponse, dynamic>(new RestDataRequest<dynamic>("deal")
                {
                    Data = new
                    {
                        hand = hand,
                        round = round,
                        game = game
                    }
                });
            }
            catch (ArgumentException ex)
            {
                // No response is required at this endpoint, SlimRest doesn't support responseless POST
                if (ex.Message != "Unrecognized Content-Type: ")
                    throw;
            }
            
            return result.bid;
        }

        public void Bid(int player, Round round, Game game)
        {
            var client = new RestClient(GetBaseUrl(game, player));
            try
            {
                client.Post<dynamic, Round>(new RestDataRequest<Round>("bid")
                {
                    Data = round
                });
            }
            catch (ArgumentException ex)
            {
                // No response is required at this endpoint, SlimRest doesn't support responseless POST
                if (ex.Message != "Unrecognized Content-Type: ")
                    throw;
            }
        }

        public string Play(int player, Book book, Round round, Game game)
        {
            var client = new RestClient(GetBaseUrl(game, player));
            var result = client.Post<PlayResponse, Book>(new RestDataRequest<Book>("play") { Data = book });

            return result.card;
        }

        public void Book(int player, Book book, Game game)
        {
            var client = new RestClient(GetBaseUrl(game, player));
            try
            {
                client.Post<dynamic, Book>(new RestDataRequest<Book>("book") { Data = book });
            }
            catch (ArgumentException ex)
            {
                // No response is required at this endpoint, SlimRest doesn't support responseless POST
                if (ex.Message != "Unrecognized Content-Type: ")
                    throw;
            }
        }

        private string GetBaseUrl(Game game, int player)
        {
            var result = new StringBuilder();
            if (player == 1)
                result.Append(_config.player1_url);

            if (player == 2)
                result.Append(_config.player2_url);

            if (player == 3)
                result.Append(_config.player3_url);

            if (player == 4)
                result.Append(_config.player4_url);

            result.AppendFormat("/{0}", game.id);
            result.AppendFormat("/{0}", player);

            return result.ToString();
        }
    }
}