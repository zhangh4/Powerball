using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerBall
{
    class MostOccurredNumberFirst : IStrategy
    {
        public IEnumerable<Ticket> Buy(IEnumerable<Ticket> pastWinners, int numOfTickets)
        {
            var pastWinnersList = pastWinners as IList<Ticket> ?? pastWinners.ToList();
            var analysisResult = Program.AnalyzeWinningNumbers(
                pastWinnersList.OrderByDescending(o => o.Date).Take(100),
                pastWinnersList.OrderByDescending(o => o.Date));

            var redballNumbersOrderedByOccurence = analysisResult.Item1.OrderBy(o => o.Count);
//            var redballNumbersOrderedByOccurence = analysisResult.Item1.OrderByDescending(o => o.Count);
            int numOfLeastLikelyWhiteBalls = 4;
            var leastLikelyWhiteBalls = analysisResult.Item2.OrderBy(o => o.Count).Take(numOfLeastLikelyWhiteBalls);
            var mostLikelyWhiteBalls = analysisResult.Item2.OrderByDescending(o => o.Count).Take(8 - numOfLeastLikelyWhiteBalls);

            List<short> redballList = redballNumbersOrderedByOccurence.Take(1).Select(o => o.Number).ToList();
            List<IEnumerable<short>> whiteballList = Program.GetPowerSet(leastLikelyWhiteBalls.Concat(mostLikelyWhiteBalls).Select(o => o.Number).ToList(), 5).ToList();
            for (int i = 0; i < numOfTickets; i++)
            {
                yield return new Ticket()
                {
                    Date = new DateTime(),
                    RedBall = redballList[i%redballList.Count],
//                    WhiteBalls = Program.GenerateRandomWhiteBalls()
                    WhiteBalls = new HashSet<short>(whiteballList[i])
                };
            }
        }
    }
}