using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poker_exercise
{
    class Program
    {
        enum CardValue {T=10, J, Q, K, A};
        enum CardSuit {D, H, S, C};

        class Card : IComparable<Card>
        {
            public int Value;
            public CardSuit Suit;
            public bool Marked;

            public Card(string data)
            {
                if (data == null || data.Length != 2)
                    throw new ArgumentException(string.Format("Invalid card: [{0}]", data));

                if (char.IsDigit(data[0]))
                    Value = int.Parse(data[0].ToString());
                else
                {
                    CardValue v;
                    if (Enum.TryParse<CardValue>(data[0].ToString(), out v))
                        Value = Convert.ToInt32(v);
                }

                if (Value < 2 || Value > 14)
                    throw new ArgumentException(string.Format("Invalid card value: [{0}], data: [{1}]", Value, data));

                if (!Enum.TryParse<CardSuit>(data[1].ToString(), out Suit))
                    throw new ArgumentException(string.Format("Invalid card suit: [{0}]", data));
            }

            public int CompareTo(Card otherCard)
            {
                if (otherCard == null)
                    return 1;
                else
                    return this.Value.CompareTo(otherCard.Value);
            }
        }

        static int p1_wins = 0;
        static int p2_wins = 0;

        static void Main(string[] args)
        {
            string line;

            try
            {
                if (Console.IsInputRedirected)
                {
                    while ((line = Console.ReadLine()) != null)
                        if (!ProcessHand(line))
                            return;
                }
                else
                {
                    Console.WriteLine("Please enter hands:");
                    line = Console.ReadLine();
                    ProcessHand(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            finally
            {
                Console.WriteLine("Player 1: {0} hands", p1_wins);
                Console.WriteLine("Player 2: {0} hands", p2_wins);
                Console.ReadLine();
            }
        }

        private static bool ProcessHand(string hand)
        {
            if (hand == null)
            {
                Console.WriteLine("Error: Hand is null");
                return false;
            }

            string[] cards = hand.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (cards.Length != 10)
            {
                Console.WriteLine("Error: Invalid hand [{0}]", hand);
                return false;
            }

            List<Card> p1_cards = new List<Card>();
            for (int i = 0; i < 5; i++)
                p1_cards.Add(new Card(cards[i]));

            p1_cards.Sort();
            int p1_rank = Rank(p1_cards);

            List<Card> p2_cards = new List<Card>();
            for (int i = 5; i < 10; i++)
                p2_cards.Add(new Card(cards[i]));

            p2_cards.Sort();
            int p2_rank = Rank(p2_cards);

            if (p1_rank > p2_rank)
                p1_wins++;
            else if (p2_rank > p1_rank)
                p2_wins++;
            else // tie
                TieBreak(p1_rank, p1_cards, p2_cards);

            return true;
        }

        private static int Rank(List<Card> cards)
        {
            if (cards == null || cards.Count != 5)
                throw new ArgumentException(string.Format("Invalid hand"));

            if (cards.TrueForAll(x => x.Value == cards.IndexOf(x) + 10)
                &&
                cards.TrueForAll(x => x.Suit == cards[0].Suit))
                return 10;

            if (cards.TrueForAll(x => x.Value == cards[0].Value + cards.IndexOf(x))
                &&
                cards.TrueForAll(x => x.Suit == cards[0].Suit))
                return 9;

            if (cards.TrueForAll(x => x.Value == cards[0].Value || cards.IndexOf(x) == 4) && Mark(cards, 0, 1, 2, 3)
                ||
                cards.TrueForAll(x => x.Value == cards[4].Value || cards.IndexOf(x) == 0) && Mark(cards, 1, 2, 3, 4))
                return 8;

            if (cards[0].Value == cards[1].Value && cards[0].Value == cards[2].Value && cards[3].Value == cards[4].Value
                ||
                cards[0].Value == cards[1].Value && cards[2].Value == cards[3].Value && cards[2].Value == cards[4].Value)
                return 7;

            if (cards.TrueForAll(x => x.Suit == cards[0].Suit))
                return 6;

            if (cards.TrueForAll(x => x.Value == cards[0].Value + cards.IndexOf(x)))
                return 5;

            if (cards[0].Value == cards[1].Value && cards[0].Value == cards[2].Value && Mark(cards, 0, 1, 2)
                ||
                cards[1].Value == cards[2].Value && cards[1].Value == cards[3].Value && Mark(cards, 1, 2, 3)
                ||
                cards[2].Value == cards[3].Value && cards[2].Value == cards[4].Value && Mark(cards, 2, 3, 4))
                return 4;

            if (cards[0].Value == cards[1].Value && cards[2].Value == cards[3].Value && Mark(cards, 0, 1, 2, 3)
                ||
                cards[0].Value == cards[1].Value && cards[3].Value == cards[4].Value && Mark(cards, 0, 1, 3, 4)
                ||
                cards[1].Value == cards[2].Value && cards[3].Value == cards[4].Value && Mark(cards, 1, 2, 3, 4))
                return 3;

            if (cards[0].Value == cards[1].Value && Mark(cards, 0, 1)
                ||
                cards[1].Value == cards[2].Value && Mark(cards, 1, 2)
                ||
                cards[2].Value == cards[3].Value && Mark(cards, 2, 3)
                ||
                cards[3].Value == cards[4].Value && Mark(cards, 3, 4))
                return 2;

            return 1;
        }

        private static bool Mark(List<Card> cards, params int[] indices)
        {
            Array.ForEach(indices, i => cards[i].Marked = true);
            return true;
        }

        private static bool TieBreak(int rank, List<Card> p1_cards, List<Card> p2_cards)
        {
            int v1, v2;
            List<Card> c1, c2;

            //Note - suits are not taken into account to break a tie for this exercise - only the value of the card determines a winner.
            switch (rank)
            {
                case 1:
                case 5:
                case 6:
                case 9:
                    for (int i = p1_cards.Count - 1; i >= 0; i--)
                    {
                        v1 = p1_cards[i].Value;
                        v2 = p2_cards[i].Value;
                        if (v1 != v2)
                        {
                            if (v1 > v2)
                                p1_wins++;
                            else
                                p2_wins++;
                            return true;
                        }
                    }
                    break;

                case 2:
                case 4:
                case 8:
                    v1 = p1_cards.Find(c => c.Marked).Value;
                    v2 = p2_cards.Find(c => c.Marked).Value;
                    if (v1 > v2)
                        p1_wins++;
                    else if (v2 > v1)
                        p2_wins++;
                    else
                    {
                        c1 = p1_cards.Where(c => !c.Marked).ToList();
                        c2 = p2_cards.Where(c => !c.Marked).ToList();
                        c1.Sort();
                        c2.Sort();
                        return TieBreak(1, c1, c2);
                    }
                    break;

                case 3:
                    c1 = p1_cards.Where(c => c.Marked).ToList();
                    c2 = p2_cards.Where(c => c.Marked).ToList();
                    c1.Sort();
                    c2.Sort();
                    if (!TieBreak(1, c1, c2))
                    {
                        v1 = p1_cards.Find(c => !c.Marked).Value;
                        v2 = p2_cards.Find(c => !c.Marked).Value;
                        if (v1 > v2)
                            p1_wins++;
                        else if (v2 > v1)
                            p2_wins++;
                        else
                            return false;
                    }
                    break;

                case 7:
                    c1 = p1_cards.Where(c => c.Value == p1_cards[0].Value).ToList();
                    c2 = p2_cards.Where(c => c.Value == p2_cards[0].Value).ToList();
                    v1 = (c1.Count == 3) ? c1[0].Value : p1_cards[0].Value;
                    v2 = (c2.Count == 3) ? c2[0].Value : p2_cards[0].Value;
                    if (v1 > v2)
                        p1_wins++;
                    else if (v2 > v1)
                        p2_wins++;
                    else
                    {
                        v1 = (c1.Count == 2) ? c1[0].Value : p1_cards[0].Value;
                        v2 = (c2.Count == 2) ? c2[0].Value : p2_cards[0].Value;
                        if (v1 > v2)
                            p1_wins++;
                        else if (v2 > v1)
                            p2_wins++;
                        else
                            return false;
                    }
                    break;

                case 10:
                    return false;

                default:
                    throw new ArgumentException(string.Format("Invalid rank: {0}", rank));
            }

            return true;
        }
    }
}
