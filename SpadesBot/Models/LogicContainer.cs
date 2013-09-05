using System.Collections.Generic;
using System.Linq;

namespace SpadesBot.Models
{
    public class LogicContainer
    {
        public LogicContainer(List<Card> cards)
        {
            HandCards = cards;
            PlayedCards = new List<Card>();
            PartnerTrumpedSuits = new List<string>();
            OpponentTrumpedSuits = new List<string>();
        }

        public int PlayerId { get; set; }

        public List<Card> PlayedCards { get; set; }
        public List<Card> HandCards { get; set; }
        public List<string> PartnerTrumpedSuits { get; set; }
        public List<string> OpponentTrumpedSuits { get; set; }

        public int PlayerBid { get; set; }
        public int PartnerBid { get; set; }
        public int LeftBid { get; set; }
        public int RightBid { get; set; }

        public bool PlayerIsNil
        {
            get { return PlayerBid <= 0; }
        }

        public bool PartnerIsNil
        {
            get { return PartnerBid <= 0; }
        }

        public bool SpadesBroken
        {
            get { return PlayedCards.Any(x => x.Suit == "s"); }
        }

        public bool SpadeTight
        {
            get { return HandCards.All(x => x.Suit == "s"); }
        }

        public int TeamId
        {
            get { return (PlayerId == 1 || PlayerId == 3) ? 1 : 2; }
        }
    }
}