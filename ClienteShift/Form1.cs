using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClienteShift
{
    public partial class Form1 : Form
    {
        public IPAddress ipAddress;
        public ushort port;
        IPEndPoint endpoint;
        String user;

        public Form1()
        {
            InitializeComponent();
            leerDatos();
            endpoint = new IPEndPoint(ipAddress, port);
            txtUsuario.Text = user.Trim();
        }

        private void btn_Click(object sender, EventArgs e)
        {
            Socket server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            try
            {
                endpoint = new IPEndPoint(ipAddress, port);
                server.Connect(endpoint);
            }
            catch (SocketException ex)
            {
                txtInfo.Text = String.Format("Error connection: {0}",ex.Message);
                return;
            }
            IPEndPoint ieServer = (IPEndPoint)server.RemoteEndPoint;
            Console.WriteLine("Server on IP:{0} at port {1}", ieServer.Address, ieServer.Port);
            using (NetworkStream ns = new NetworkStream(server))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {

                sw.WriteLine(txtUsuario.Text);
                sw.Flush();

                string comando = ((Button)sender).Tag.ToString();
                sw.WriteLine(comando);
                sw.Flush();
                txtInfo.Text = "";
                user = txtUsuario.Text;
                txtInfo.Text += sr.ReadToEnd();


            }
            Console.WriteLine("Ending connection");
            guardarDatos();
            server.Close();
        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {
            if (txtUsuario.Text != "admin")
            {
                btnList.Enabled = true;
                btnAdd.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
                btnList.Enabled = false;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            FormConfig fc = new FormConfig(this);
            fc.ShowDialog();
        }
        public void guardarDatos()
        {
            try
            {
                string directory = Environment.GetEnvironmentVariable("userprofile");
                using (StreamWriter sw = new StreamWriter(directory + "\\datos.txt"))
                {
                    sw.WriteLine($"{port}_{ipAddress}_{user}");
                }
            }
            catch (Exception ex) when (ex is IOException | ex is ArgumentException)
            {
            }
        }

        public void leerDatos()
        {
            try
            {
                using (StreamReader sr = new StreamReader(Environment.GetEnvironmentVariable("userprofile") + "\\datos.txt"))
                {
                    String datos = sr.ReadToEnd();

                    ushort.TryParse(datos.Split('_')[0], out port);
                    IPAddress.TryParse(datos.Split('_')[1], out ipAddress);
                    user = datos.Split('_')[2];
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException)
            {
                port = 31416;
                ipAddress = IPAddress.Loopback;
                user = "";
            }
        }

    }
}
