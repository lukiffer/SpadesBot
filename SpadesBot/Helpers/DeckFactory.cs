using System.Collections.Generic;
using System.Linq;
using SpadesBot.Models;

namespace SpadesBot.Helpers
{
    public static class DeckFactory
    {
        public static List<string> CreateStringDeck()
        {
            return new List<string> {
                "As", "Ks", "Qs", "Js", "10s", "9s", "8s", "7s", "6s", "5s", "4s", "3s", "2s",
                "Ah", "Kh", "Qh", "Jh", "10h", "9h", "8h", "7h", "6h", "5h", "4h", "3h", "2h",
                "Ac", "Kc", "Qc", "Jc", "10c", "9c", "8c", "7c", "6c", "5c", "4c", "3c", "2c",
                "Ad", "Kd", "Qd", "Jd", "10d", "9d", "8d", "7d", "6d", "5d", "4d", "3d", "2d"
            };
        } 

        public static List<Card> CreateCardDeck()
        {
            return CreateStringDeck().Select(x => new Card(x)).ToList();
        }
    }
}