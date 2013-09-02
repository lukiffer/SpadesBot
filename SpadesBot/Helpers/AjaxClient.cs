using System.Collections.Generic;
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
            var client = new SlimRest.Client.RestClient(GetBaseUrl(player));
            var result = client.Post<BlindResponse, dynamic>(new RestDataRequest<dynamic>(game.id + "/blind")
                                     {
                                         Data = new
                                                    {
                                                        player = player,
                                                        round = round,
                                                        game = game
                                                    }
                                     });
            return result.blind;
        }

        public int Deal(int player, List<string> hand, Round round, Game game)
        {
            var client = new SlimRest.Client.RestClient(GetBaseUrl(player));
            var result = client.Post<DealResponse, dynamic>(new RestDataRequest<dynamic>(game.id + "/deal")
                                                     {
                                                         Data = new
                                                                    {
                                                                        player = player,
                                                                        hand = hand,
                                                                        round = round,
                                                                        game = game
                                                                    }
                                                     });
            return result.bid;
        }

        public string Play(int player, Book book, Round round, Game game)
        {
            var client = new SlimRest.Client.RestClient(GetBaseUrl(player));
            var result = client.Post<PlayResponse, Book>(
                new RestDataRequest<Book>(game.id + "/play") { Data = book });

            return result.card;
        }

        public void Book(int player, Book book, Game game)
        {
            var client = new SlimRest.Client.RestClient(GetBaseUrl(player));
            client.Post<PlayResponse, Book>(
                new RestDataRequest<Book>(game.id + "/play") { Data = book });
        }

        private string GetBaseUrl(int player)
        {
            if (player == 1)
                return _config.player1_url;

            if (player == 2)
                return _config.player2_url;

            if (player == 3)
                return _config.player3_url;

            if (player == 4)
                return _config.player4_url;

            return null;
        }
    }
}