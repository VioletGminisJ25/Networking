using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatroomServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Thread thread = new Thread(() => new Server());
            thread.Start();
            
        }
    }
}
