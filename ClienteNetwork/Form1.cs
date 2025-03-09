using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClienteNetwork//Titulo, icono SOLVED
{   
    public partial class Form1 : Form
    {
        public IPAddress ipServer;
        public int port;

        public Form1()
        {
            InitializeComponent();
            btnClose.Enabled = false;
            ipServer = IPAddress.Parse("127.0.0.1");
            port = 31416;
            //ipServer = null;
            //port = 0;
            btnDate.Enabled = true;
            btnTime.Enabled = true;
            btnAll.Enabled = true;
        }

        private void btnClick(object sender, EventArgs e)
        {


            IPEndPoint ie = new IPEndPoint(ipServer, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ie);
                IPEndPoint ieServer = (IPEndPoint)server.RemoteEndPoint;
                Console.WriteLine("Server on IP:{0} at port {1}", ieServer.Address,ieServer.Port);

                using (NetworkStream ns = new NetworkStream(server))
                using (StreamReader sr = new StreamReader(ns))
                using (StreamWriter sw = new StreamWriter(ns))
                {
                    String msg;
                    String userMsg;

                    Control button = (Control)sender;
                    userMsg = button.Tag.ToString();
                    if(userMsg == "close")
                    {
                        userMsg = "close " + txtPassword.Text.Trim();
                    }
                    sw.WriteLine(userMsg);
                    sw.Flush();

                    msg = sr.ReadLine();
                    lblResult.Text = msg;
                }
                server.Close();
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
                server.Close();
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            Form config = new FormConfig(this);
            config.ShowDialog(this);
            btnAll.Enabled = true;
            btnDate.Enabled = true;
            btnTime.Enabled = true;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            btnClose.Enabled = txtPassword.Text != ""; 
        }
    }
}
