using System;
using Discord;

namespace PokerBot.Classes
{
    public struct Card: IComparable<Card>
    {
        private readonly string _suit;
        private readonly int _value;
        private readonly string _color;
        private readonly Emoji _emoji;

        public Card(string suit, int value)
        {
            if (value < 2 || value > 14)
            {
                throw new Exception("Index for card out of bounds.");
            }

            switch (suit.ToLower())
            {
                case "clubs":
                case "c":
                    _suit = "clubs";
                    _color = "black";
                    _emoji = new Emoji("♣");
                    break;
                case "spades":
                case "s":
                    _suit = "spades";
                    _color = "black";
                    _emoji = new Emoji("♠");
                    break;
                case "hearts":
                case "h":
                    _suit = "hearts";
                    _color = "red";
                    _emoji = new Emoji("♥");
                    break;
                case "diamonds": 
                case "d":
                    _suit = "diamonds";
                    _color = "red";
                    _emoji = new Emoji("♦");
                    break;
                default:
                    throw new Exception("Invalid suit. Use plural name or first character.");
            }
            this._value = value;
        }

        public int GetValue()
        {
            return _value;
        }

        public string GetSuit()
        {
            return _suit;
        }

        public int CompareTo(Card card)
        {
            if (_value < card._value)
            {
                return -1;
            }

            return _value == card._value ? 0 : 1;
        }

        public override string ToString()
        {
            return _value switch
            {
                11 => "J" + _emoji,
                12 => "Q" + _emoji,
                13 => "K" + _emoji,
                14 => "A" + _emoji,
                _ => $"{_value}{_emoji}"
            };
        }
    }
}