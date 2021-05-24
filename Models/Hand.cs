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
            
            CheckFlushStraight();   // Checks for a flush, straight, or related hand
            
            if (scoreTierOne > 0)
            {
                return;
            }

            CheckForMatches();      // Checks for pair, three of a kind, four of a kind, and full house

        }

        private void CheckFlushStraight()
        {
            var flush = cards.All(card => card.GetSuit() == cards[0].GetSuit());
            var straight = true;
            for (var i = 4; i > 1; i++)
            {
                if (cards[i].GetValue() - cards[i - 1].GetValue() != 1)
                    straight = false;
            }
            
            //Royal Flush
            if (flush && straight)
            {
                scoreTierOne = cards[4].GetValue() == 13 ? 9 : 8;   // Checks for straight flush
                return;
            }
            // Flush
            if (flush)
            {
                scoreTierOne = 5;
            }
            //Straight
            else if (straight)
            {
                scoreTierOne = 4;
            }

            scoreTierThree = cards[4].GetValue();
        }
        private void CheckForMatches() 
        {
            // Four of a kind
            if (cards[0].GetValue() == cards[3].GetValue())
            {
                scoreTierOne = 7;
                scoreTierTwo = cards[0].GetValue();
                scoreTierThree = cards[4].GetValue();
                return;
            }
            if (cards[1].GetValue() == cards[4].GetValue())
            {
                scoreTierOne = 7;
                scoreTierTwo = cards[1].GetValue();
                scoreTierThree = cards[0].GetValue();
                return;
            }
            
            //Three of a kind
            if (cards[0].GetValue() == cards[2].GetValue())
            {
                if (cards[3].GetValue() == cards[4].GetValue())
                {
                    scoreTierOne = 6;                       // Full House
                    scoreTierTwo = cards[0].GetValue();
                    scoreTierThree = cards[3].GetValue();
                }
                else
                {
                    scoreTierOne = 3;                       // Three of a kind
                    scoreTierTwo = cards[0].GetValue();
                    scoreTierThree = cards[4].GetValue();
                }

                return;
            }

            if (cards[2].GetValue() == cards[4].GetValue()) 
            {
                if (cards[0].GetValue() == cards[1].GetValue()) 
                {
                    scoreTierOne = 6;                       // Full House
                    scoreTierTwo = cards[2].GetValue();
                    scoreTierThree = cards[0].GetValue();
                }
                else 
                {
                    scoreTierOne = 3;                       // Three of a kind
                    scoreTierTwo = cards[2].GetValue();
                    scoreTierThree = cards[1].GetValue();
                }

                return;
            }
            
            //Pairs
            var pairs = 0;


        }

    }
}