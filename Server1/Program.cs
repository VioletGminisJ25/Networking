﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server1
{

    internal class Program
    {
      
        static void Main(string[] args)
        {
            Thread thread = new Thread(() => { new Server(); });
            thread.Start();
        }

      
    }
}
