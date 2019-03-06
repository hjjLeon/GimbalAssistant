using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GimbleAssistant
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string[] baud = { "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            this.comboBoxBaud.Items.AddRange(baud);

            this.comboBoxComNum.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            this.comboBoxBaud.Text = "115200";
            this.comboBoxDataSize.Text = "8";
            this.comboBoxjiaoyan.Text = "None";
            this.comboBoxStopBit.Text = "1";
            this.button1.Text = "打开串口";
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
