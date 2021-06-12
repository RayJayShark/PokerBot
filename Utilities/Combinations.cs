using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerBot.Models;

namespace PokerBot.Utilities
{
    public static class Combinations
    {
        public static List<Hand> FindCombinations(List<Card> cards)
        {
            var hands = new List<Hand>();
            CombinationUtil(hands, cards, new Card[5], 0, 0);
            return hands;
        }

        private static void CombinationUtil(List<Hand> hands, List<Card> cards, Card[] hand, int start, int index)
        {
            if (index == 5)
            {
                hands.Add(new Hand(hand));
                return;
            }

            for (var i = start; i < cards.Count && cards.Count - i >= 5 - index; i++)
            {
                hand[index] = cards[i];
                CombinationUtil(hands, cards, hand, i + 1, index + 1);
            }
        }
    }
}
