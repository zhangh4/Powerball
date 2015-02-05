using System;
using System.Collections.Generic;

namespace PowerBall
{
    class RandomStrategy : IStrategy
    {
        public IEnumerable<Ticket> Buy(IEnumerable<Ticket> pastWinners, int numOfTickets)
        {
            var drum = new Random();
            for (int i = 0; i < numOfTickets; i++)
            {
                yield return new Ticket()
                {
                    Date = new DateTime(),
                    WhiteBalls = Program.GenerateRandomWhiteBalls(drum),
                    RedBall = (short) drum.Next(1, 36)
                };
            }
        }
    }
}