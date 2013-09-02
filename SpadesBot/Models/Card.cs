using System;

namespace SpadesBot.Models
{
    public class Card
    {
        public Card(string card)
        {
            Suit = (card.Length) == 3 ? card.Substring(2, 1) : card.Substring(1, 1);
            var r = card.Replace(Suit, String.Empty);
            switch (r)
            {
                case "A":
                    Rank = 14;
                    break;
                case "K":
                    Rank = 13;
                    break;
                case "Q":
                    Rank = 12;
                    break;
                case "J":
                    Rank = 11;
                    break;
                default:
                    Rank = Convert.ToInt32(r);
                    break;
            }
        }

        public int Rank { get; set; }
        public string Suit { get; set; }

        public override string ToString()
        {
            switch (Rank)
            {
                case 14:
                    return "A" + Suit;
                case 13:
                    return "K" + Suit;
                case 12:
                    return "Q" + Suit;
                case 11:
                    return "J" + Suit;
                default:
                    return Rank + Suit;
            }
        }
    }
}