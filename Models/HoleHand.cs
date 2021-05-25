using System;

namespace PokerBot.Classes
{
    public struct HoleHand
    {
        private Card[] _cards;

        public HoleHand(Card card1, Card card2)
        {
            _cards = new Card[] {card1, card2};
        }

        public HoleHand(Card[] cards)
        {
            if (cards.Length == 2)
            {
                _cards = cards;
            }
            else
            {
                throw new Exception("Hand must contain 2 cards.");
            }
        }

        public Card[] GetCards()
        {
            return _cards;
        }

        public override string ToString()
        {
            return $"{_cards[0].ToString()}, {_cards[1].ToString()}";
        }
    }
}