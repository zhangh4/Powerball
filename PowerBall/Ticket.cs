using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBall
{
    class Ticket
    {
        public DateTime Date { get; set; }
        public HashSet<short> WhiteBalls { get; set; }
        public short RedBall { get; set; }

        public override string ToString()
        {
            return string.Format("Date: {0}, WhiteBalls: {1}, RedBall: {2}", 
                Date, 
                "(" + WhiteBalls.Aggregate(string.Empty, (result, number) => result + " " + number) + ")", 
                RedBall);
        }

        public int DeterminePrize(Ticket winningNumber)
        {
            if (winningNumber == null) throw new ArgumentNullException("winningNumber");

            var countOfWhiteMatches = winningNumber.WhiteBalls.Intersect(WhiteBalls).Count();

            if (winningNumber.RedBall == RedBall)
            {
                switch (countOfWhiteMatches)
                {
                    case 0:
                        return (int) Prize.RedOnly;
                    case 1:
                        return (int) Prize.RedPlusOneWhite;
                    case 2:
                        return (int) Prize.RedPlusTwoWhite;
                    case 3:
                        return (int) Prize.RedPlusThreeWhite;
                    case 4:
                        return (int) Prize.RedPlusFourWhite;
                    case 5:
                        return (int) Prize.RedPlusFiveWhite;
                }
            }
            else
            {
                switch (countOfWhiteMatches)
                {
                    case 3:
                        return (int)Prize.ThreeWhite;
                    case 4:
                        return (int)Prize.FourWhite;
                    case 5:
                        return (int)Prize.FiveWhite;
                }
            }

            return (int) Prize.None;
        }
    }
}