using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Server1
{
    internal class Server//Comprobar archivos SOLVED
    {
        private static int[] PORT = { 135, 31416, 31516, 31616, 31716 };
        private string password;
        private bool serverAwake;
        private Socket s;
        private int portCont = 0;
        private bool portSearching = false;
        public Server()
        {
            init();
        }
        private void init()
        {
            try
            {
                using (StreamReader sw = new StreamReader(Environment.GetEnvironmentVariable("PROGRAMDATA") + "/password.txt"))
                {
                    password = sw.ReadLine();
                }
            }
            catch (UnauthorizedAccessException e) {
                Console.WriteLine("Permisos de archivo de contraseña incorrecto");
            }
            catch (IOException e)
            {
                Console.WriteLine("Achivo de contraseña no encontrado");

            }
            do
            {

                if (portCont == PORT.Length)
                {
                    Console.WriteLine("Server Error: No port avaliable");
                    portSearching = false;
                }
                else
                {

                    serverAwake = true;
                    IPEndPoint ie = new IPEndPoint(IPAddress.Any, PORT[portCont++]);
                    portSearching = false;

                    using (s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        try
                        {
                            s.Bind(ie);
                            s.Listen(10);
                            Console.WriteLine($"Server waiting, port: {ie.Port} ");

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


        private void hiloCliente(object sClient)
        {
            string mensaje;
            Socket cliente = (Socket)sClient;
            IPEndPoint ieCliente = (IPEndPoint)cliente.RemoteEndPoint;
            Console.WriteLine("Connected with client {0} at port {1}",
            ieCliente.Address, ieCliente.Port);
            using (NetworkStream ns = new NetworkStream(cliente))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {

                try
                {
                    mensaje = sr.ReadLine();
                    Console.WriteLine(mensaje);
                    //El mensaje es null al cerrar
                    if (mensaje != null)
                    {


                        if (mensaje.StartsWith("close "))
                        {
                            if (mensaje.Length > 6)
                            {
                                if (mensaje == "close " + password)
                                {
                                    serverAwake = false;
                                    s.Close();
                                }
                                else
                                {
                                    sw.WriteLine("Incorect Password");
                                }
                            }
                            else
                            {
                                sw.WriteLine("Password is required");
                            }
                        }
                        else
                        {
                            switch (mensaje)
                            {
                                case "date":
                                    sw.WriteLine(DateTime.Now.ToShortDateString());
                                    break;
                                case "time":
                                    sw.WriteLine(DateTime.Now.ToShortTimeString());
                                    break;
                                case "all":
                                    sw.WriteLine(DateTime.Now.ToString());
                                    break;
                                default:
                                    sw.WriteLine("Comando invalido");
                                    break;
                            }
                        }

                        sw.Flush();
                        Console.WriteLine("{0} says: {1}",
                        ieCliente.Address, mensaje);
                    }
                }
                catch (IOException)
                {
                    //Salta al acceder al socket
                    //y no estar permitido
                }

                Console.WriteLine("Finished connection with {0}:{1}",
                ieCliente.Address, ieCliente.Port);
            }
            cliente.Close();

        }
    }
}
