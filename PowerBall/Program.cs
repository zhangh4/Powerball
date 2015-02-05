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
        public static readonly List<short> possilbeWhiteBalls = Enumerable.Range(1, 59).Select(o => (short)o).ToList();
        public static readonly List<short> possilbeRedBalls = Enumerable.Range(1, 35).Select(o => (short)o).ToList();
        public static readonly DateTime cutoffDate = new DateTime(2012, 1, 15);

        static void Main(string[] args)
        {
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

            var analysisResult = AnalyzeWinningNumbers(winningNumbers, winningNumbers);
            PrintWinningNumbers(analysisResult.Item1, analysisResult.Item2);

            ApplyStrategy(winningNumbers, new MostOccurredNumberFirst());
//            ApplyStrategy(winningNumbers, new RandomStrategy());

        }

        private static void ApplyStrategy(List<Ticket> winningNumbers, IStrategy strategy)
        {
            long totalPrize = 0;
            long totalTickets = 0;
            foreach (var winningNumber in winningNumbers.Where(o => o.Date >= cutoffDate).OrderBy(o => o.Date))
            {
                Ticket number = winningNumber;
                var tickets = strategy.Buy(winningNumbers.Where(o => o.Date < number.Date), 40).ToList();
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
                    if (prize > 0)
                    {
                        Console.WriteLine("Ticket {0} won {1}", ticket, prize);
                    }
                    totalPrize += prize;
                }
                Console.WriteLine();
            }
            Console.WriteLine(string.Format("Total Prize: {0} for {1} tickets", totalPrize, totalTickets));
        }

        internal static Tuple<IEnumerable<NumberWithStats>, IEnumerable<NumberWithStats>> AnalyzeWinningNumbers(
            IEnumerable<Ticket> winningNumbersForRedBall, IEnumerable<Ticket> winningNumbersForWhiteBall)
        {
            var countByRedBall = winningNumbersForRedBall.GroupBy(wn => wn.RedBall);
//            var countByRedBall = winningNumbers.GroupBy(wn => wn.RedBall).OrderByDescending(g => g.Count());
            var countByWhiteBall = winningNumbersForWhiteBall.SelectMany(t => t.WhiteBalls).GroupBy(o => o);

            var redBallByCount =
                from possbileNumber in possilbeRedBalls
                join winningNumberGroup in countByRedBall on possbileNumber equals winningNumberGroup.Key into ps
                from p in ps.DefaultIfEmpty()
                select new NumberWithStats(){Number = possbileNumber, Count = p == null ? 0 : p.Count()};

            var whiteBallByCount =
                from possbileNumber in possilbeWhiteBalls
                join winningNumberGroup in countByWhiteBall on possbileNumber equals winningNumberGroup.Key into ps
                from p in ps.DefaultIfEmpty()
                select new NumberWithStats() { Number = possbileNumber, Count = p == null ? 0 : p.Count() };

            return new Tuple<IEnumerable<NumberWithStats>, IEnumerable<NumberWithStats>>(redBallByCount.ToList(), whiteBallByCount.ToList());
        }

        private static void PrintWinningNumbers(
            IEnumerable<NumberWithStats> redballNumbers,
            IEnumerable<NumberWithStats> whiteballNumbers)
        {
            Console.WriteLine();
            Console.WriteLine("=========== Red Ball Stats =========");

            int count = 1;
            foreach (var redballCount in redballNumbers.OrderByDescending(o => o.Count))
            {
                Console.WriteLine(string.Format("{2}. RedBall {0} occurred {1} times", redballCount.Number, redballCount.Count,
                    count++));
            }

            Console.WriteLine();
            Console.WriteLine("=========== White Ball Stats =========");

            count = 1;
            foreach (var whiteBallCount in whiteballNumbers.OrderByDescending(o => o.Count))
            {
                Console.WriteLine(string.Format("{2}. WhiteBall {0} occurred {1} times", whiteBallCount.Number,
                    whiteBallCount.Count, count++));
            }
            
        }

        public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
        {
            return from m in Enumerable.Range(0, 1 << list.Count)
                      select
                          from i in Enumerable.Range(0, list.Count)
                          where (m & (1 << i)) != 0
                          select list[i];
        }

        public static IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list, int numOfElements)
        {
            foreach (var bagOfElements in GetPowerSet(list))
            {
                var bagOfElementsList = bagOfElements as IList<T> ?? bagOfElements.ToList();
                if (bagOfElementsList.Count == numOfElements) yield return bagOfElementsList;
            }
        }

        public static HashSet<short> GenerateRandomWhiteBalls(Random drum = null)
        {
            if(drum == null) drum = new Random();
            var whiteBalls = new HashSet<short>();
            while (whiteBalls.Count < 5)
            {
                whiteBalls.Add((short)drum.Next(1, 60));
            }
            return whiteBalls;
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
}
