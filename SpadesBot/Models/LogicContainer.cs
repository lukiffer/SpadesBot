using System.Collections.Generic;
using System.Linq;
using SpadesBot.Helpers;

namespace SpadesBot.Models
{
    public class LogicContainer
    {
        private readonly List<Card> _deck;

        public LogicContainer()
        {
            _deck = DeckFactory.CreateCardDeck();
        }
        
        public List<Card> PlayedCards { get; set; }
        public List<Card> HandCards { get; set; }
        public List<string> PartnerTrumped { get; set; }

        public List<Card> FindRemainingCards(string suit, bool includeCardsInHand = false)
        {
            return _deck.Where(x => !PlayedCards.Contains(x) &&
                HandCards.Contains(x) == includeCardsInHand && x.Suit == suit).ToList();
        }
    }
}