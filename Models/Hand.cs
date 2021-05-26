#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerBot.Classes
{
    public class Hand : IComparable<Hand>
    {
        private readonly List<Card> cards;
        private string? handName;
        private int scoreTierOne;   // Type of hand
        private int scoreTierTwo;   // Hand rank
        private int scoreTierThree; // High card 
        private int scoreTierFour;  // High card for two pairs

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

        public Hand()
        {
            scoreTierOne = 0;
            scoreTierTwo = 0;
            scoreTierThree = 0;
            scoreTierFour = 0;
        }

        public string? GetHandName()
        {
            return handName;
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
            for (var i = 4; i > 1; i--)
            {
                if (cards[i].GetValue() - cards[i - 1].GetValue() != 1)
                    straight = false;
            }
            
            if (flush && straight)
            {
                //Royal Flush
                if (cards[4].GetValue() == 14)
                {
                    scoreTierOne = 9;
                    handName = "A Royal Flush";
                    return;
                }

                //Straight Flush
                scoreTierOne = 8;  
                scoreTierTwo = cards[4].GetValue();
                handName = "A Straight Flush";
                
                return;
            }
            // Flush
            if (flush)
            {
                scoreTierOne = 5;
                handName = "A Flush";
            }
            //Straight
            else if (straight)
            {
                scoreTierOne = 4;
                handName = "A Straight";
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
                handName = "Four of a Kind";
                return;
            }
            if (cards[1].GetValue() == cards[4].GetValue())
            {
                scoreTierOne = 7;
                scoreTierTwo = cards[1].GetValue();
                scoreTierThree = cards[0].GetValue();
                handName = "Four of a Kind";

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
                    handName = "A Full House";
                }
                else
                {
                    scoreTierOne = 3;                       // Three of a kind
                    scoreTierTwo = cards[0].GetValue();
                    scoreTierThree = cards[4].GetValue();
                    handName = "Three of a Kind";
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
                    handName = "A Full House";
                }
                else 
                {
                    scoreTierOne = 3;                       // Three of a kind
                    scoreTierTwo = cards[2].GetValue();
                    scoreTierThree = cards[1].GetValue();
                    handName = "Three of a Kind";
                }

                return;
            }
            
            //Pairs
            var pairLoc = new List<int>();
            for (var i = 0; i < 4; i++)
            {
                if (cards[i].GetValue() == cards[i + 1].GetValue())
                {
                    pairLoc.Add(i);
                }
            }

            if (pairLoc.Count == 2)             // Two pairs
            {
                handName = "Two Pair";
                
                scoreTierOne = 2;
                scoreTierThree = cards[pairLoc[0]].GetValue();
                scoreTierTwo = cards[pairLoc[1]].GetValue();
                if (pairLoc[0] == 1)
                {
                    scoreTierFour = cards[0].GetValue();
                }
                else if (pairLoc[1] == 2)
                {
                    scoreTierFour = cards[4].GetValue();
                }
                else
                {
                    scoreTierFour = cards[2].GetValue();
                }

                return;
            }

            if (pairLoc.Count == 1)             // One pair
            {
                handName = "A Pair";
                scoreTierOne = 1;
                scoreTierTwo = cards[pairLoc[0]].GetValue();
                scoreTierThree = pairLoc[0] == 3 ? cards[2].GetValue() : cards[4].GetValue();   // High card
            }
            else
            {
                scoreTierTwo = cards[4].GetValue();
                handName = "High Card";
            }

        }

        public int CompareTo(Hand hand)
        {
            if (scoreTierOne > hand.scoreTierOne)
            {
                return 1;
            }
            if (scoreTierOne < hand.scoreTierOne)
            {
                return -1;
            }

            if (scoreTierTwo > hand.scoreTierTwo) 
            {
                return 1;
            }
            if (scoreTierTwo < hand.scoreTierTwo) 
            {
                return -1;
            }

            if (scoreTierThree > hand.scoreTierThree) 
            {
                return 1;
            }
            if (scoreTierThree < hand.scoreTierThree) 
            {
                return -1;
            }

            if (scoreTierFour > hand.scoreTierFour) 
            {
                return 1;
            }
            if (scoreTierFour < hand.scoreTierFour) 
            {
                return -1;
            }

            return 0;
        }

    }
}