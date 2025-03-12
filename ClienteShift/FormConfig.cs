using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClienteShift
{
    public partial class FormConfig : Form
    {
        private Form1 form1;
        private bool valid;
        private IPAddress ipAddress;
        ushort port;
        public FormConfig(Form1 form)
        {
            InitializeComponent();
            this.form1 = form;
            this.btnAccept.Enabled = false;
            valid = true;
            if (form1.ipAddress != null)
            {
                this.txtIp.Text = form1.ipAddress.ToString();
            }
            if (form1.port != 0)
            {
                this.txtPort.Text = form1.port + "";
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            Console.WriteLine("entra");

            if (valid)
            {
                form1.ipAddress = ipAddress;
                form1.port = port;
                this.Close();
            }
        }

        private void ControlTextChanged(object sender, EventArgs e)
        {
            bool ipvalid = IPAddress.TryParse(this.txtIp.Text.ToString().Trim(), out ipAddress);
            bool portvalid = ushort.TryParse(this.txtPort.Text.ToString().Trim(), out port);
            if (!ipvalid || !portvalid)
            {
                labelError.Text = "Datos del servidor invalidos";
                valid = false;
                btnAccept.Enabled = false;
            }
            else
            {
                valid = true;
                btnAccept.Enabled = true;
                labelError.Text = "";

            }
        }
    }
}
