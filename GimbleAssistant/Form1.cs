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
using System.Windows.Forms.DataVisualization.Charting;
using ZvGraph.UI;

namespace GimbleAssistant
{
    public partial class Form1 : Form
    {
        private Queue<float> dataQueueCh1 = new Queue<float>();
        private Queue<float> dataQueueCh2 = new Queue<float>();
        private Queue<float> dataQueueCh3 = new Queue<float>();
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

            string[] ANO = { "加速度计", "陀螺仪", "磁编码器",};
            checkedListBox1.Items.AddRange(ANO);
            string[] CH = { "ch1", "ch2", "ch3", };
            checkedListBox2.Items.AddRange(CH);

            numericUpDown1.Value = 100;

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

        private void UpdateQueueValue(int ch, Queue<float> dataQueue, float data)
        {

            while(dataQueue.Count > numericUpDown1.Value-1)
            {
                dataQueue.Dequeue();
            }
            dataQueue.Enqueue(data);
            
            switch(ch)
            {
                case 1:
                    y1.Clear();
                    x1.Clear();
                    for (int i = 0; i < dataQueue.Count; i++)
                    {
                        y1.Add(dataQueue.ElementAt(i));
                        x1.Add((i + 1));
                    }
                    break;
                case 2:
                    y2.Clear();
                    x2.Clear();
                    for (int i = 0; i < dataQueue.Count; i++)
                    {
                        y2.Add(dataQueue.ElementAt(i));
                        x2.Add((i + 1));
                    }
                    break;
                case 3:
                    y3.Clear();
                    x3.Clear();
                    for (int i = 0; i < dataQueue.Count; i++)
                    {
                        y3.Add(dataQueue.ElementAt(i));
                        x3.Add((i + 1));
                    }
                    break;
            }
        }
        
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                if (anoStatus)
                {
                    byte[] buff = new byte[100];
                    int num = serialPort1.BytesToRead;
                    serialPort1.Read(buff, 0, num);
                    if (buff[0]==0xAA && buff[1] == 0xAA)
                    {
                        int dataLen = buff[3];
                        Int16 mlx3 = BitConverter.ToInt16(buff, 15);
                        float mlx3f = Convert.ToSingle(mlx3);
                        Int16 mlx1 = BitConverter.ToInt16(buff, 17);
                        float mlx1f = Convert.ToSingle(mlx1);
                        Int16 mlx2 = BitConverter.ToInt16(buff, 19);
                        float mlx2f = Convert.ToSingle(mlx2);
                        //因为要访问UI资源，所以需要使用invoke方式同步ui
                        this.Invoke((EventHandler)(delegate
                        {
                            if (checkedListBox2.GetItemChecked(0))
                            {
                                UpdateQueueValue(1, dataQueueCh1, mlx3f);
                            }
                            if (checkedListBox2.GetItemChecked(1))
                            {
                                UpdateQueueValue(2, dataQueueCh2, mlx2f);
                            }
                            if (checkedListBox2.GetItemChecked(2))
                            {
                                UpdateQueueValue(3, dataQueueCh3, mlx1f);
                            }
                            zGraph1.f_Refresh();
                        }
                           )
                        );
                    }
                }
                else
                {
                    //因为要访问UI资源，所以需要使用invoke方式同步ui
                    this.Invoke((EventHandler)(delegate
                    {
                        textBox1.AppendText(serialPort1.ReadExisting());
                    }
                       )
                    );
                }
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
        public List<float> x5 = new List<float>();
        public List<float> y5 = new List<float>();
        public List<float> x6 = new List<float>();
        public List<float> y6 = new List<float>();
        public List<float> x7 = new List<float>();
        public List<float> y7 = new List<float>();
        public List<float> x8 = new List<float>();
        public List<float> y8 = new List<float>();

        public Boolean anoStatus = false;
        private void zGraphLoadPix()
        {
            zGraph1.f_ClearAllPix();
            zGraph1.f_reXY();
            if(checkedListBox2.GetItemChecked(0))
            {
                zGraph1.f_AddPix(ref x1, ref y1, Color.Blue, 2);
            }
            if (checkedListBox2.GetItemChecked(1))
            {
                zGraph1.f_AddPix(ref x2, ref y2, Color.Green, 2);
            }
            if (checkedListBox2.GetItemChecked(2))
            {
                zGraph1.f_AddPix(ref x3, ref y3, Color.Red, 2);
            }

        }
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {

                    anoStatus = !anoStatus;
                    if (anoStatus)
                    {
                        serialPort1.Write("ANO\r\n");
                        button7.Text = "关闭波形显示";
                        x1.Clear();
                        y1.Clear();
                        x2.Clear();
                        y2.Clear();
                        x3.Clear();
                        y3.Clear();
                        zGraphLoadPix();
                    }
                    else
                    {
                        serialPort1.Write("q");
                        button7.Text = "打开波形显示";
                    }
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
        }

        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //zGraphLoadPix();
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (i != e.Index)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
        }
    }
}
