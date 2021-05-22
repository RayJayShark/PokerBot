using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerBot.Classes
{
    public class Hand
    {
        private readonly List<Card> cards;
        private int scoreTierOne;   // Type of hand
        private int scoreTierTwo;   // Hand rank
        private int scoreTierThree; // High card 

        public Hand(ICollection<Card> cards)
        {
            if (cards.Count != 5)
            {
                throw new Exception("A hand must consist of 5 cards.");
            }
            this.cards = cards.ToList();
            CalculateScore();
        }

        public Hand(Card card1, Card card2, Card card3, Card card4, Card card5)
        {
            cards = new List<Card>
            {
                card1,
                card2,
                card3,
                card4,
                card5
            };

            CalculateScore();
        }

        public void CalculateScore()
        {
            // Sort cards for better comparing
            cards.Sort();
            
            CheckFlushStraight();
            
            // Pair
            var pair = new int[2];
            var next = false;
            for (var i = 0; i < 4; i++)
            {
                for (var j = i + 1; j < 5; j++)
                {
                    if (cards[i].GetValue() == cards[j].GetValue())
                    {
                        next = true;
                        pair[0] = i;
                        pair[1] = j;
                        break;
                    }
                }
            }
        }

        private void CheckFlushStraight()
        {
            var flush = cards.All(card => card.GetSuit() == cards[0].GetSuit());
            //var straight = 
        }

    }
}