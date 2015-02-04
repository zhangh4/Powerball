using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBall
{
    class Program
    {
        static void Main(string[] args)
        {
            List<short> possilbeWhiteBalls = Enumerable.Range(1, 59).Select(o => (short)o).ToList();
            List<short> possilbeRedBalls = Enumerable.Range(1, 35).Select(o => (short)o).ToList();

            List<Ticket> winningNumbers = new List<Ticket>();
            using (var sr = new StreamReader("powerball-winning-numbers.txt"))
            {
                sr.ReadLine(); // skip the column header line
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    var parts = line.Split(new []{"  "}, StringSplitOptions.None);
                    var winningNumber = new Ticket()
                    {
                        Date = DateTime.Parse(parts[0]),
                        WhiteBalls = new HashSet<short>(parts.Skip(1).Take(5).Select(short.Parse)),
                        RedBall = short.Parse(parts[6])
                    };
                    winningNumbers.Add(winningNumber);
                    Console.WriteLine(winningNumber);
                }
                
            }

            var countByRedBall = winningNumbers.GroupBy(wn => wn.RedBall);
//            var countByRedBall = winningNumbers.GroupBy(wn => wn.RedBall).OrderByDescending(g => g.Count());

            var query =
                from possbileNumber in possilbeRedBalls
                join winningNumberGroup in countByRedBall on possbileNumber equals winningNumberGroup.Key into ps
                from p in ps.DefaultIfEmpty()
                select new {Number = possbileNumber, Count = p.Count()};

            int count = 1;
            foreach (var redballCount in query.OrderByDescending(o => o.Count))
            {
                Console.WriteLine(string.Format("{2}. RedBall {0} occurred {1} times", redballCount.Number, redballCount.Count, count++));
            }
        }
    }

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

    enum Prize
    {
        None = 0, RedOnly = 4, RedPlusOneWhite = 4, RedPlusTwoWhite = 7, ThreeWhite = 7, RedPlusThreeWhite = 100, FourWhite = 100, 
        RedPlusFourWhite = 10000, FiveWhite = 1000000, RedPlusFiveWhite = 100000000
    }
}
