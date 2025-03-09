using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace ChatroomServer
{
    internal class Server
    {
        private static int[] PORT = { 135, 31416, 31516, 31616, 31716 };
        private static int port;
        private bool serverAwake;
        private Socket s;
        private int portCont = 0;
        private bool portSearching = false;
        private List<StreamWriter> streams = new List<StreamWriter>();
        private Dictionary<IPEndPoint, string> clients = new Dictionary<IPEndPoint, string>();
        static object l = new object();
        //private Service1 service ;
        private bool serverdown = false;
        public Server()
        {
            //this.service = service;
            init();
        }
        public void init()
        {
            do
            {

                if (!portSearching)
                {

                    try
                    {
                        using (StreamReader streamReader = new StreamReader(Environment.GetEnvironmentVariable("programdata") + "\\chatroom.config"))
                        {
                            if (!Int32.TryParse(streamReader.ReadLine(), out port))
                            {
                                //service.writeEvent("Error al leer el archivo");
                                port = 31416;
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        port = 31416;
                    }
                }
                else
                {
                    if (port == 31416)
                    {
                        serverdown = true;
                        portSearching = false;
                    }
                    else
                    {
                        port = 31416;
                    }
                }

                if (!serverdown)
                {

                    serverAwake = true;
                    IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);
                    portSearching = false;

                    using (s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        try
                        {
                            s.Bind(ie);
                            s.Listen(10);
                            Console.WriteLine($"Server waiting, port: {ie.Port} ");
                            //service.writeEvent($"Server waiting, port: {ie.Port}");

                            /*---------------------------------------------------------------------------*/
                            while (serverAwake)
                            {
                                Socket sClient = s.Accept();
                                Thread hilo = new Thread(hiloCliente);
                                hilo.IsBackground = true;
                                hilo.Start(sClient);
                            }

                        }
                        catch (SocketException ex) when (ex.ErrorCode == (int)SocketError.Interrupted)
                        {

                        }
                        catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                        {
                            portSearching = true;
                        }
                    }
                }

            } while (portSearching);
        }

        private void hiloCliente(Object sClient)
        {
            bool clientAlive = true;
            string nombreCliente = "";
            string mensaje;
            Socket cliente = (Socket)sClient;
            IPEndPoint ieCliente = (IPEndPoint)cliente.RemoteEndPoint;
            using (NetworkStream ns = new NetworkStream(cliente))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {
                Console.WriteLine("Connected with client {0} at port {1}", ieCliente.Address, ieCliente.Port);
                sw.WriteLine("Welcome to the sever");
                sw.WriteLine("Introduce un nombre: ");
                sw.Flush();
                nombreCliente = sr.ReadLine();
                lock (l)
                {

                    streams.Add(sw);
                    clients.Add(ieCliente, nombreCliente);
                }
                try
                {
                    lock (l)
                    {
                        foreach (StreamWriter s in streams)
                        {

                            if (s != sw)
                            {
                                s.WriteLine("Connected {0}@{1}", nombreCliente, ieCliente.Address);
                                s.Flush();
                            }
                        }
                    }

                    while (clientAlive)
                    {
                        string message = sr.ReadLine();
                        if (message != null)
                        {
                            if (message.StartsWith("#"))
                            {
                                switch (message)
                                {
                                    case "#lista":
                                        sw.WriteLine("---------------------");
                                        sw.WriteLine("Usuarios conectados");
                                        foreach (IPEndPoint client in clients.Keys)
                                        {
                                            sw.WriteLine("{0}@{1}", clients[client], client.Address);
                                        }
                                        sw.WriteLine("---------------------");
                                        break;
                                    case "#exit":
                                        lock (l)
                                        {

                                            foreach (StreamWriter s in streams)
                                            {
                                                if (s != sw)
                                                {
                                                    s.WriteLine("Desconnected: {0}@{1}", nombreCliente, ieCliente.Address);
                                                    s.Flush();
                                                }
                                            }
                                        }
                                        clientAlive = false;
                                        break;
                                    default:
                                        break;
                                }
                                sw.Flush();
                            }
                            else
                            {
                                lock (l)
                                {

                                    foreach (StreamWriter s in streams)
                                    {
                                        if (s != sw)
                                        {
                                            s.WriteLine("{0}@{1}: {2}", nombreCliente, ieCliente.Address, message);
                                            s.Flush();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            lock (l)
                            {

                                foreach (StreamWriter s in streams)
                                {
                                    if (s != sw)
                                    {
                                        s.WriteLine("Desconnected: {0}@{1}", nombreCliente, ieCliente.Address);
                                        s.Flush();
                                    }
                                }
                            }
                            clientAlive = false;
                        }
                    }

                }
                catch (IOException)
                {
                    //Salta al acceder al socket
                    //y no estar permitido
                }
                lock (l)
                {

                    streams.Remove(sw);
                    clients.Remove(ieCliente);

                }
            }
            cliente.Close();
        }
    }
}
