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
            using (var sr = new StreamReader("/users/p/documents/powerball-winning-numbers.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);    
                }
                
            }
        }
    }

    class WinningNumber
    {
        public DateTime Date { get; set; }
        public HashSet<short> WhiteBalls { get; set; }
        public short RedBall { get; set; }
    }
}
