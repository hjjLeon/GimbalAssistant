using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GimbleAssistant
{
    public partial class Form1 : Form
    {
        public List<float> x1 = new List<float>(1000);
        public List<float> y1 = new List<float>(1000);
        public List<float> x2 = new List<float>(1000);
        public List<float> y2 = new List<float>(1000);
        public List<float> x3 = new List<float>(1000);
        public List<float> y3 = new List<float>(1000);
        private Boolean anoStatus = false;
        private bool AllowSelect = true;
        private bool normalUpdate = true;
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

            numericUpDown1.Value = 400;

            checkedListBox1.SetItemChecked(0, true);
            checkedListBox2.SetItemChecked(0, true);
            checkedListBox2.SetItemChecked(1, true);
            checkedListBox2.SetItemChecked(2, true);


            serialPort1.NewLine = "\r\n";

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 10000; toolTip1.InitialDelay = 500; toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.button8, "1.确保云台BootLoader完好\r\n2.确保串口连接正常\r\n3.切断云台供电\r\n4.点击修复按钮\r\n5.在5S内给云台重新上电");
        }

        private void AnoSwitch(Boolean status)
        {
            if (serialPort1.IsOpen)
            {

                anoStatus = status;

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

        private void terminalSend()
        {
            if (serialPort1.IsOpen)
            {
                if (checkBox1.Checked)
                {
                    serialPort1.WriteLine(textBox2.Text);
                }
                else
                {
                    serialPort1.Write(textBox2.Text);
                }
            }
        }

        private void zGraphLoadPix()
        {
            zGraph1.f_ClearAllPix();
            zGraph1.f_reXY();
            if (checkedListBox2.GetItemChecked(0))
            {
                x1.Clear();
                y1.Clear();
                zGraph1.f_AddPix(ref x1, ref y1, Color.Blue, 2);
            }
            if (checkedListBox2.GetItemChecked(1))
            {
                x2.Clear();
                y2.Clear();
                zGraph1.f_AddPix(ref x2, ref y2, Color.Green, 2);
            }                     
            if (checkedListBox2.GetItemChecked(2))
            {
                x3.Clear();
                y3.Clear();
                zGraph1.f_AddPix(ref x3, ref y3, Color.Red, 2);
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
        
        private void AnoAnalysis(byte[] buff, int offset, ref float[] data)
        {
            Int16 temp;
            temp = BitConverter.ToInt16(buff, offset + 0);
            data[0] = Convert.ToSingle(temp);
            temp = BitConverter.ToInt16(buff, offset + 2);
            data[1] = Convert.ToSingle(temp);
            temp = BitConverter.ToInt16(buff, offset + 4);
            data[2] = Convert.ToSingle(temp);
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (anoStatus)
            {
                byte[] buff = new byte[1000];
                float[] data = new float[3];
                int num = serialPort1.BytesToRead;
                serialPort1.Read(buff, 0, num);
                if (buff[0]==0xAA && buff[1] == 0xAA)
                {
                    int dataLen = buff[3];
                    if (checkedListBox1.GetItemChecked(0))
                    {
                        AnoAnalysis(buff, 3, ref data);
                    }
                    else if (checkedListBox1.GetItemChecked(1))
                    {
                        AnoAnalysis(buff, 9, ref data);
                    }
                    else if (checkedListBox1.GetItemChecked(2))
                    {
                        AnoAnalysis(buff, 15, ref data);
                    }

                    if (checkedListBox2.GetItemChecked(0))
                    {
                        UpdateQueueValue(1, dataQueueCh1, data[0]);
                    }
                    if (checkedListBox2.GetItemChecked(1))
                    {
                        UpdateQueueValue(2, dataQueueCh2, data[1]);
                    }
                    if (checkedListBox2.GetItemChecked(2))
                    {
                        UpdateQueueValue(3, dataQueueCh3, data[2]);
                    }
                    //因为要访问UI资源，所以需要使用invoke方式同步ui
                    this.Invoke((EventHandler)(delegate
                    {
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
                terminalSend();
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

        private void button7_Click(object sender, EventArgs e)
        {
            if(anoStatus)
            {
                AnoSwitch(false);
            }
            else
            {
                AnoSwitch(true);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
            dataQueueCh1.Clear();
            dataQueueCh2.Clear();
            dataQueueCh3.Clear();
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {

            if (anoStatus)
            {
                if (e.TabPage != tabPage2)
                {
                    AnoSwitch(false);
                }
            }
            else
            {
                if (e.TabPage == tabPage2)
                {
                    AnoSwitch(true);
                }
            }


            if (e.TabPage == tabPage1)
            {

            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && checkBox1.Checked)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (e.KeyData == (Keys.Enter) && checkBox1.Checked)
                {
                    e.Handled = true;
                    terminalSend();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox3.Text = openFileDialog.FileName.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("update");
                System.Threading.Thread.Sleep(500);

                button6.Text = "升级中";
                button6.Enabled = false;
                button5.Enabled = false;
                button8.Enabled = false;
                AllowSelect = false;

                ymodem = new Ymodem.Ymodem();
                ymodem.serialPort = serialPort1;
                ymodem.Path = textBox3.Text.ToString();
                ymodem.PortName = comboBoxComNum.SelectedItem.ToString();
                ymodem.BaudRate = Convert.ToInt32(comboBoxBaud.SelectedItem.ToString());
                downloadThread = new System.Threading.Thread(ymodem.YmodemUploadFile);
                ymodem.NowDownloadProgressEvent += new EventHandler(NowDownloadProgressEvent);
                ymodem.DownloadResultEvent += new EventHandler(DownloadFinishEvent);
                downloadThread.Start();
            }
        }

        #region 下载进度委托及事件响应
        private delegate void NowDownloadProgress(int nowValue);
        private void NowDownloadProgressEvent(object sender, EventArgs e)
        {
            int value = Convert.ToInt32(sender);
            NowDownloadProgress count = new NowDownloadProgress(UploadFileProgress);
            this.Invoke(count, value);
        }
        private void UploadFileProgress(int count)
        {
            progressBar1.Value = count;
            if(normalUpdate)
            {
                if (count % 5 == 0)
                {
                    button6.Text += ".";
                    if (button6.Text.Length > 6)
                    {
                        button6.Text = "升级中.";
                    }
                }
            }
            else
            {
                if (count % 5 == 0)
                {
                    button8.Text += ".";
                    if (button8.Text.Length > 6)
                    {
                        button8.Text = "修复中.";
                    }
                }

            }
        }
        #endregion
        #region 下载完成委托及事件响应
        private delegate void DownloadFinish(bool finish);
        private void DownloadFinishEvent(object sender, EventArgs e)
        {
            bool finish = (Boolean)sender;
            DownloadFinish status = new DownloadFinish(UploadFileResult);
            this.Invoke(status, finish);
        }
        private void UploadFileResult(bool result)
        {
            AllowSelect = true;
            button6.Enabled = true;
            button5.Enabled = true;
            button8.Enabled = true;
            if (result == true)
            {
                MessageBox.Show("升级成功");
                this.progressBar1.Value = 0;
            }
            else
            {
                MessageBox.Show("升级失败");
                this.progressBar1.Value = 0;
            }
            if (normalUpdate)
            {
                this.button6.Text = "升级";
            }
            else
            {
                normalUpdate = true;
                this.button8.Text = "固件修复";
            }
        }
        #endregion

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = !AllowSelect;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int i;
                button8.Text = "等待中...";
                button6.Enabled = false;
                button5.Enabled = false;
                button8.Enabled = false;
                AllowSelect = false;

                for(i = 0; i < 30; i++)
                {
                    serialPort1.Write("l");
                    if (serialPort1.BytesToRead != 0 && serialPort1.ReadByte() == 'C')
                    {
                        break;
                    }
                    button8.Text += ".";
                    if (button8.Text.Length > 6)
                    {
                        button8.Text = "等待中.";
                    }
                    System.Threading.Thread.Sleep(200);
                }
                if(i >= 30)
                {
                    button6.Enabled = true;
                    button5.Enabled = true;
                    button8.Enabled = true;
                    AllowSelect = true;
                    return;
                }

                normalUpdate = false;

                //因为要访问UI资源，所以需要使用invoke方式同步ui
                this.Invoke((EventHandler)(delegate
                {
                    button8.Text = "修复中.";
                }
                ));

                ymodem = new Ymodem.Ymodem();
                ymodem.serialPort = serialPort1;
                ymodem.Path = textBox3.Text.ToString();
                ymodem.PortName = comboBoxComNum.SelectedItem.ToString();
                ymodem.BaudRate = Convert.ToInt32(comboBoxBaud.SelectedItem.ToString());
                downloadThread = new System.Threading.Thread(ymodem.YmodemUploadFile);
                ymodem.NowDownloadProgressEvent += new EventHandler(NowDownloadProgressEvent);
                ymodem.DownloadResultEvent += new EventHandler(DownloadFinishEvent);
                downloadThread.Start();

            }
        }
    }
}
