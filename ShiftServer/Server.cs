using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShiftServer
{
    internal class Server
    {
        private string[] users;
        private List<string> waitQueue;
        private ushort port;
        public Socket serverSocket;
        private object l = new object();
        private object l2 = new object();
        public Server()
        {
            port = 31416;
            waitQueue = new List<string>();
        }

        private void ReadNames(string ruta)
        {
            try
            {
                using (StreamReader sr = new StreamReader(ruta))
                {
                    this.users = sr.ReadLine().Split(';');
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Ha habido problemilas, {ex.Message}");
            }
        }

        private int ReadPin(string ruta)
        {
            if (ruta != null || ruta == "")
            {
                try
                {

                    using (BinaryReader br = new BinaryReader(new FileStream(ruta, FileMode.Open)))
                    {

                        int pin = br.ReadInt32();

                        return pin.ToString().Length >= 4 ? pin : -1;
                    }
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        public void Init()
        {
            leer();
            bool portSearching = false;
            int vueltas = 0;
            do
            {
                if (portSearching)
                {
                    if (vueltas == 1)
                    {
                        this.port = 1024;
                    }
                    else
                    {
                        if (this.port == ushort.MaxValue)
                        {
                            portSearching = false;
                        }
                        else
                        {
                            this.port++;
                        }
                    }
                }

                bool serverAwake = true;
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
                using (serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        serverSocket.Bind(endPoint);
                        portSearching = false;
                        serverSocket.Listen(10);
                        Console.WriteLine($"Server Wating at port {port}");
                        this.ReadNames(Environment.GetEnvironmentVariable("userprofile") + "/usuarios.txt");
                        while (serverAwake)
                        {
                            Socket scliente = serverSocket.Accept();
                            Thread hiloCliente = new Thread(cliente);
                            hiloCliente.IsBackground = true;
                            hiloCliente.Start(scliente);
                        }
                    }
                    catch (SocketException ex) when (ex.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    {
                        portSearching = true;
                    }
                    catch (SocketException ex) when (ex.ErrorCode == (int)SocketError.Interrupted) { serverAwake = false; }
                }
                vueltas++;
            } while (portSearching);
            guardar();
        }

        private void cliente(Object sClient)
        {
            Socket socketCliente = (Socket)sClient;
            IPEndPoint clienteEndPoint = (IPEndPoint)socketCliente.RemoteEndPoint;
            using (NetworkStream ns = new NetworkStream(socketCliente))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {
                try
                {
                    sw.WriteLine("Bienvenido al servidor por turnos");
                    sw.WriteLine("Introduce el nombre: ");
                    sw.Flush();
                    string nombre = sr.ReadLine();
                    bool isAdmin = false;
                    bool valid = false;
                    if (nombre != null || nombre != "")
                    {
                        if (nombre != "admin")
                        {
                            foreach (string s in users)
                            {
                                if (nombre == s)
                                {
                                    valid = true;
                                }
                            }
                        }
                        else
                        {
                            int pin = this.ReadPin(Environment.GetEnvironmentVariable("userprofile") + "/pin.bin");
                            if (pin == -1)
                            {
                                pin = 1234;
                            }
                            sw.WriteLine("Introduce PIN: ");
                            sw.Flush();
                            Int32.TryParse(sr.ReadLine(), out int pinIntroducido);
                            if (pinIntroducido == pin)
                            {
                                valid = true;
                                isAdmin = true;
                            }
                        }
                    }

                    if (!valid)
                    {
                        sw.WriteLine("Usuario Desconocido");
                        sw.Flush();
                    }
                    else
                    {
                        do
                        {
                            string comando = sr.ReadLine();
                            if (comando != null || comando != "")
                            {
                                switch (comando)
                                {
                                    case "list":

                                        lock (l)
                                        {
                                            foreach (string s in waitQueue)
                                            {
                                                sw.WriteLine(s);
                                            }
                                            sw.Flush();
                                        }
                                        break;

                                    case "add":
                                        lock (l)
                                        {
                                            bool inlist = false;
                                            foreach (string s in waitQueue)
                                            {
                                                if (s.Split(';')[0] == nombre)
                                                {
                                                    inlist = true;
                                                }
                                            }
                                            if (!inlist)
                                            {
                                                string nombreFormateado = nombre + ";" + DateTime.Now;
                                                waitQueue.Add(nombreFormateado);
                                                sw.WriteLine("Añadido a la lista");
                                            }
                                            else
                                            {
                                                sw.WriteLine("Ya en la lista");
                                            }
                                            sw.Flush();
                                        }
                                        break;


                                    case string aux when comando.StartsWith("del ") && isAdmin:
                                        lock (l)
                                        {
                                            bool deleted = false;
                                            string[] com = comando.Split(' ');
                                            if (com.Length == 2)
                                            {
                                                if (Int32.TryParse(com[1], out int pos))
                                                {
                                                    if (pos >= 0 && pos < waitQueue.Count)
                                                    {
                                                        waitQueue.RemoveAt(pos);
                                                        deleted = true;
                                                    }
                                                }
                                                if (!deleted)
                                                {
                                                    sw.WriteLine("delete error");
                                                    sw.Flush();
                                                }
                                            }
                                        }
                                        break;
                                    case string aux when comando.StartsWith("chpin ") && isAdmin:
                                        lock (l2)
                                        {
                                            string[] com2 = comando.Split(' ');
                                            if (com2.Length == 2)
                                            {
                                                if (Int32.TryParse(com2[1], out int pin))
                                                {
                                                    if (pin.ToString().Length >= 4)
                                                    {
                                                        try
                                                        {

                                                            using (BinaryWriter bw = new BinaryWriter(new FileStream(Environment.GetEnvironmentVariable("userprofile") + "/pin.bin", FileMode.Create)))
                                                            {
                                                                bw.Write(pin);
                                                                sw.WriteLine("El pin se ha guardado correctamente");
                                                            }
                                                        }
                                                        catch (IOException ex)
                                                        {
                                                            sw.WriteLine("Ha habido un error guardando el pin");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sw.WriteLine("El pin debe se de al menos 4 digitos");
                                                    }
                                                }
                                            }
                                            sw.Flush();
                                        }
                                        break;
                                    case "exit" when isAdmin:
                                        isAdmin = false;
                                        break;
                                    case "shutdown" when isAdmin:
                                        isAdmin = false;
                                        serverSocket.Close();
                                        break;
                                }

                            }
                        } while (isAdmin);
                    }
                }
                catch (IOException ex) { }
            }
            socketCliente.Close();
        }

        private void guardar()
        {
            string path = Environment.GetEnvironmentVariable("userprofile") + "/data.txt";
            try
            {

                using (StreamWriter s = new StreamWriter(path, false))
                {
                    foreach (string user in waitQueue)
                    {
                        s.WriteLine(user);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Ha habido problemilas, {ex.Message}");
            }

        }
        private void leer()
        {
            string path = Environment.GetEnvironmentVariable("userprofile") + "/data.txt";
            try
            {

                using (StreamReader s = new StreamReader(path, false))
                {
                    waitQueue.AddRange(s.ReadToEnd().Split('\n'));
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"Ha habido problemilas, {ex.Message}");
            }
        }
    }
}
