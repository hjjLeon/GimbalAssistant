using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZvGraph.UI;

namespace GimbleAssistant
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string[] baud = { "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            comboBoxBaud.Items.AddRange(baud);

            comboBoxComNum.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            comboBoxBaud.Text = "115200";
            comboBoxDataSize.Text = "8";
            comboBoxjiaoyan.Text = "None";
            comboBoxStopBit.Text = "1";
            button1.Text = "打开串口";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //将可能产生异常的代码放置在try块中
                //根据当前串口属性来判断是否打开
                if (serialPort1.IsOpen)
                {
                    //串口已经处于打开状态
                    serialPort1.Close();    //关闭串口
                    button1.Text = "打开串口";
                    button1.BackColor = Color.ForestGreen;
                    comboBoxBaud.Enabled = true;
                    comboBoxDataSize.Enabled = true;
                    comboBoxjiaoyan.Enabled = true;
                    comboBoxStopBit.Enabled = true;
                    comboBoxComNum.Enabled = true;
                    textBox1.Text = "";  //清空接收区
                    textBox2.Text = "";     //清空发送区
                }
                else
                {
                    //串口已经处于关闭状态，则设置好串口属性后打开
                    comboBoxBaud.Enabled = false;
                    comboBoxDataSize.Enabled = false;
                    comboBoxjiaoyan.Enabled = false;
                    comboBoxStopBit.Enabled = false;
                    comboBoxComNum.Enabled = false;
                    serialPort1.PortName = comboBoxComNum.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBoxBaud.Text);
                    serialPort1.DataBits = Convert.ToInt16(comboBoxDataSize.Text);

                    if (comboBoxjiaoyan.Text.Equals("None"))
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                    else if (comboBoxjiaoyan.Text.Equals("Odd"))
                        serialPort1.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBoxjiaoyan.Text.Equals("Even"))
                        serialPort1.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBoxjiaoyan.Text.Equals("Mark"))
                        serialPort1.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBoxjiaoyan.Text.Equals("Space"))
                        serialPort1.Parity = System.IO.Ports.Parity.Space;

                    if (comboBoxStopBit.Text.Equals("1"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBoxStopBit.Text.Equals("1.5"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBoxStopBit.Text.Equals("2"))
                        serialPort1.StopBits = System.IO.Ports.StopBits.Two;

                    serialPort1.Open();     //打开串口
                    button1.Text = "关闭串口";
                    button1.BackColor = Color.Firebrick;
                }
            }
            catch (Exception ex)
            {
                //捕获可能发生的异常并进行处理

                //捕获到异常，创建一个新的对象，之前的不可以再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBoxComNum.Items.Clear();
                comboBoxComNum.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBoxBaud.Enabled = true;
                comboBoxDataSize.Enabled = true;
                comboBoxjiaoyan.Enabled = true;
                comboBoxStopBit.Enabled = true;
                comboBoxComNum.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                    serialPort1.Write(textBox2.Text);
            }
            catch (Exception ex)
            {
                //捕获可能发生的异常并进行处理

                //捕获到异常，创建一个新的对象，之前的不可以再用
                serialPort1 = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBoxComNum.Items.Clear();
                comboBoxComNum.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBoxBaud.Enabled = true;
                comboBoxDataSize.Enabled = true;
                comboBoxjiaoyan.Enabled = true;
                comboBoxStopBit.Enabled = true;
                comboBoxComNum.Enabled = true;
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                //因为要访问UI资源，所以需要使用invoke方式同步ui
                this.Invoke((EventHandler)(delegate
                {
                    textBox1.AppendText(serialPort1.ReadExisting());
                }
                   )
                );

            }
            catch (Exception ex)
            {
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(ex.Message);
            }
        }

        public List<float> x1 = new List<float>();
        public List<float> y1 = new List<float>();
        public List<float> x2 = new List<float>();
        public List<float> y2 = new List<float>();
        public List<float> x3 = new List<float>();
        public List<float> y3 = new List<float>();
        public List<float> x4 = new List<float>();
        public List<float> y4 = new List<float>();
        private void button3_Click(object sender, EventArgs e)
        {
            ///-300~num画四条数据
            button3.Enabled = false;
            this.Focus();
            int num;
            num = 1580;
            x1.Clear();
            y1.Clear();
            x2.Clear();
            y2.Clear();
            x3.Clear();
            y3.Clear();
            x4.Clear();
            y4.Clear();
            for (int i = -300; i < num; i++)
            {
                x1.Add(i);
                y1.Add(i % 1000);
                x2.Add(i);
                y2.Add((float)Math.Sin(i / 100f) * 200);
                x3.Add(i);
                y3.Add(0);
                x4.Add(i);
                y4.Add((float)Math.Sin(i / 100) * 200);
            }
            zGraph1.f_ClearAllPix();
            zGraph1.f_reXY();
            zGraph1.f_LoadOnePix(ref x1, ref y1, Color.Red, 2);
            zGraph1.f_AddPix(ref x2, ref y2, Color.Blue, 4);
            zGraph1.f_AddPix(ref x3, ref y3, Color.FromArgb(0, 128, 192), 2);
            zGraph1.f_AddPix(ref x4, ref y4, Color.Yellow, 4);
            zGraph1.f_Refresh();
            button3.Enabled = true;
        }
    }
}
