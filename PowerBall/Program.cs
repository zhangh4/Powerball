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
            DateTime formatChangeDate = new DateTime(2012, 1, 15);

            List<Ticket> winningNumbers = new List<Ticket>();
            using (var sr = new StreamReader("powerball-winning-numbers.txt"))
            {
                sr.ReadLine(); // skip the column header line
                string line;
                while ((line = sr.ReadLine()) != null)
                {
//                    Console.WriteLine(line);
                    var parts = line.Split(new []{"  "}, StringSplitOptions.None);
                    var winningNumber = new Ticket()
                    {
                        Date = DateTime.Parse(parts[0]),
                        WhiteBalls = new HashSet<short>(parts.Skip(1).Take(5).Select(short.Parse)),
                        RedBall = short.Parse(parts[6])
                    };
                    winningNumbers.Add(winningNumber);
//                    Console.WriteLine(winningNumber);
                }
                
            }

            long totalPrize = 0;
            long totalTickets = 0;
            IStrategy strategy = new RandomStrategy();
            foreach (var winningNumber in winningNumbers.Where(o => o.Date >= formatChangeDate))
            {
                var tickets = strategy.Buy(winningNumbers.Where(o => o.Date < winningNumber.Date), 40).ToList();
//                tickets[0].RedBall = 34;
//                tickets[0].WhiteBalls.RemoveWhere(o => true);
//                tickets[0].WhiteBalls.Add(16);
//                tickets[0].WhiteBalls.Add(5);
//                tickets[0].WhiteBalls.Add(50);
//                tickets[0].WhiteBalls.Add(11);
//                tickets[0].WhiteBalls.Add(26);
                Console.WriteLine(string.Format("Winning Number: {0}", winningNumber));
                foreach (var ticket in tickets)
                {
                    totalTickets++;
                    int prize = ticket.DeterminePrize(winningNumber);
                    Console.WriteLine("Ticket {0} won {1}", ticket, prize);
                    totalPrize += prize;
                }
                Console.WriteLine();
            }
            Console.WriteLine(string.Format("Total Prize: {0} for {1} tickets", totalPrize, totalTickets));

//            AnalyzeWinningNumbers(winningNumbers, possilbeRedBalls, possilbeWhiteBalls);
        }

        private static void AnalyzeWinningNumbers(List<Ticket> winningNumbers, List<short> possilbeRedBalls, List<short> possilbeWhiteBalls)
        {
            var countByRedBall = winningNumbers.GroupBy(wn => wn.RedBall);
//            var countByRedBall = winningNumbers.GroupBy(wn => wn.RedBall).OrderByDescending(g => g.Count());
            var countByWhiteBall = winningNumbers.SelectMany(t => t.WhiteBalls).GroupBy(o => o);

            var redBallByCount =
                from possbileNumber in possilbeRedBalls
                join winningNumberGroup in countByRedBall on possbileNumber equals winningNumberGroup.Key into ps
                from p in ps.DefaultIfEmpty()
                select new {Number = possbileNumber, Count = p.Count()};

            Console.WriteLine();
            Console.WriteLine("=========== Red Ball Stats =========");

            int count = 1;
            foreach (var redballCount in redBallByCount.OrderByDescending(o => o.Count))
            {
                Console.WriteLine(string.Format("{2}. RedBall {0} occurred {1} times", redballCount.Number, redballCount.Count,
                    count++));
            }

            Console.WriteLine();
            Console.WriteLine("=========== White Ball Stats =========");

            var whiteBallByCount =
                from possbileNumber in possilbeWhiteBalls
                join winningNumberGroup in countByWhiteBall on possbileNumber equals winningNumberGroup.Key into ps
                from p in ps.DefaultIfEmpty()
                select new {Number = possbileNumber, Count = p.Count()};

            count = 1;
            foreach (var whiteBallCount in whiteBallByCount.OrderByDescending(o => o.Count))
            {
                Console.WriteLine(string.Format("{2}. WhiteBall {0} occurred {1} times", whiteBallCount.Number,
                    whiteBallCount.Count, count++));
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

    interface IStrategy
    {
        IEnumerable<Ticket> Buy(IEnumerable<Ticket> pastWinners, int numOfTickets);
    }

    class RandomStrategy : IStrategy
    {
        public IEnumerable<Ticket> Buy(IEnumerable<Ticket> pastWinners, int numOfTickets)
        {
            var drum = new Random();
            for (int i = 0; i < numOfTickets; i++)
            {
                HashSet<short> whiteBalls = new HashSet<short>();
                while (whiteBalls.Count < 5)
                {
                    whiteBalls.Add((short) drum.Next(1, 60));
                }
                yield return new Ticket()
                {
                    Date = new DateTime(),
                    WhiteBalls = whiteBalls,
                    RedBall = (short) drum.Next(1, 36)
                };
            }
        }
    }
}
