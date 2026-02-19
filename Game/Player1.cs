using System.Collections.Generic;

namespace CardFool
{
    public class MPlayer1
    {
        private string Name = """Bot 0.1beta "T.A.Б." """;
        private List<SCard> hand = new List<SCard>();       // карты на руке
        private SCard trump;
        private int enemyCards = 6;     //количество карт у противника
        private int pricup = 24;
        private bool attack;        //атакует ли сейчас бот или защищается


        // Возвращает имя игрока
        public string GetName()
        {
            return Name;
        }
        //Возвращает количество карт на руке
        public int GetCount()
        {
            return hand.Count;
        }
        //Добавление карты в руку, во время добора из колоды, или взятия карт
        public void AddToHand(SCard card)
        {
            int ind = 0;
            if (hand.Count != 0)
            {
                int reit = card.Rank; //+ (card.Suit == trump.Suit ? 100 : 0);
                int reit1 = hand[0].Rank; //+ (hand[0].Suit == trump.Suit ? 100 : 0);
                while (reit1 < reit)
                {
                    reit1 = hand[0].Rank; //+ (hand[0].Suit == trump.Suit ? 100 : 0);
                    ind++;
                }
                hand.Insert(ind, card);
            }
            else hand.Add(card);
        }

        //Начальная атака
        public List<SCard> LayCards()
        {
            attack = true;
            List<SCard> at = new List<SCard>();
            if (hand.Count == 1)
            {
                at.Add(hand[0]);
                hand.RemoveAt(0);
                return at;
            }
            int count = 1;
            int indl = 0;
            int mc = 1;
            for (int i = 1; i < hand.Count; i++)
            {
                if (hand[i].Rank == hand[i - 1].Rank) count++;
                else
                {
                    if (mc < count)
                    {
                        mc = count;
                        indl = i - 1;
                    }
                    count = 1;
                }
            }
            if (mc > 1 && enemyCards > 1)
            {
                int c = enemyCards;
                while (mc > 0 && c > 0)
                {
                    at.Add(hand[indl]);
                    hand.RemoveAt(indl);
                    indl--;
                    mc--;
                    c--;
                }
            }
            else
            {
                at.Add(hand[0]);
                hand.RemoveAt(0);
            }
            
            return at;
        }

        //Защита от карт
        //На вход подается набор карт на столе, часть из них могут быть уже покрыты
        public bool Defend(List<SCardPair> table)
        {
            attack = false;
            foreach (SCardPair pair in table)
            {
                if (!pair.Beaten)
                {
                    for(int i = 0; i < hand.Count; i++)
                    {
                        if (pair.SetUp(hand[i], trump.Suit))
                        {
                            hand.RemoveAt(i);
                            break;
                        }
                    }
                    if (!pair.Beaten) return false;
                }
            }
            return true;
        }

        //Добавление карт
        //На вход подается набор карт на столе, а также отбился ли оппонент
        public bool AddCards(List<SCardPair> table, bool OpponentDefenced)
        {
            int ind = 0;
            bool isAdded = false;
            while (table.Count < Math.Min(enemyCards, 6) && ind < table.Count)
            {
                foreach (SCardPair pair in table)
                {
                    if(SCardPair.CanBeAddedToPair(hand[ind], pair))
                    {
                        table.Add(new SCardPair(hand[ind]));
                        hand.RemoveAt(ind);
                        isAdded = true;
                        break;
                    }
                }
                ind++;
            }
            return isAdded;
        }

        //Вызывается после основной битвы, когда известно отбился ли защищавшийся
        //На вход подается набор карт на столе, а также была ли успешной защита
        public void OnEndRound(List<SCardPair> table, bool IsDefenceSuccesful)
        {
            int reqCards1 = Math.Max(0, 6 - (hand.Count - table.Count));
            int reqCards2 = Math.Max(0, 6 - (enemyCards - table.Count));
            if (IsDefenceSuccesful) 
            {
                if(pricup >= reqCards1 + reqCards2)
                {
                    enemyCards = Math.Max(6, enemyCards - table.Count);
                    pricup -= reqCards1 + reqCards2;
                }
                else
                {
                    
                    if (attack)
                        pricup = Math.Max(0, pricup - reqCards1);
                    enemyCards -= Math.Min(0, table.Count - pricup);
                    pricup = 0;
                }
            }
            else
            {
                if (attack)
                {
                    enemyCards += table.Count;
                    pricup = Math.Min(0, pricup - reqCards1);
                }
                else
                {
                    enemyCards += Math.Min(pricup, reqCards2) - table.Count;
                    pricup = Math.Min(0, pricup - reqCards2);
                }
            }
            
        }

        //Установка козыря, на вход подаётся козырь, вызывается перед первой раздачей карт
        public void SetTrump(SCard NewTrump)
        {
            trump = NewTrump;
        }
    }
}
