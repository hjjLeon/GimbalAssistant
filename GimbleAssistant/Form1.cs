using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;

namespace GimbleAssistant
{
    public partial class Form1 : Form
    {
        #region 用于各功能的私有属性
        //存储波形数据
        private List<float> x1 = new List<float>(1000);
        private List<float> y1 = new List<float>(1000);
        private List<float> x2 = new List<float>(1000);
        private List<float> y2 = new List<float>(1000);
        private List<float> x3 = new List<float>(1000);
        private List<float> y3 = new List<float>(1000);
        private Queue<float> dataQueueCh1 = new Queue<float>();
        private Queue<float> dataQueueCh2 = new Queue<float>();
        private Queue<float> dataQueueCh3 = new Queue<float>();
        //ANO波形显示开关标志
        private Boolean anoStatus = false;
        //禁止切换标签页标志（用于固件升级时阻止误操作）
        private bool AllowSelect = true;
        //固件升级或修复标志
        private bool normalUpdate = true;
        //串口助手&参数状态解析标志
        private enum serialTypeS
        {
            normal,
            status,
        };
        private serialTypeS serialType = serialTypeS.normal;
        //状态标志的集合
        private List<CheckBox> statusCheckBox = new List<CheckBox>(16);
        //PID数值框的集合
        private List<List<TextBox>> pidList = new List<List<TextBox>>(6);
        //PID读取按钮的集合
        private List<Button> pidRbList = new List<Button>();
        //PID设置按钮的集合
        private List<Button> pidSbList = new List<Button>();
        //用于判断页面切入及切出的记录变量
        private TabPage lastPage, currenPage;
        //Ymodem下载的线程
        System.Threading.Thread downloadThread;
        //Ymodem对象
        private Ymodem.Ymodem ymodem;
        #endregion

        //页面初始化
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
            toolTip1.SetToolTip(this.button8, "1.确保云台BootLoader完好\r\n2.确保串口连接正常\r\n3.切断云台供电\r\n4.选择用于修复的固件\r\n5.点击修复按钮\r\n6.在5S内给云台重新上电");

            statusCheckBox.Clear();
            statusCheckBox.Add(checkBox5);//mlx1
            statusCheckBox.Add(checkBox4);//mlx2
            statusCheckBox.Add(checkBox3);//mlx3
            statusCheckBox.Add(checkBox2);//imu
            statusCheckBox.Add(checkBox6);//CAN
            statusCheckBox.Add(checkBox9);//mt1
            statusCheckBox.Add(checkBox8);//mt2
            statusCheckBox.Add(checkBox7);//mt3

            timer1.Stop();

            List<TextBox> temp = new List<TextBox>();
            temp.Clear();
            temp.Add(textBox4);
            temp.Add(textBox5);
            temp.Add(textBox6);
            temp.Add(textBox7);
            temp.Add(textBox8);
            pidList.Add(temp);
            temp = new List<TextBox>();
            temp.Clear();
            temp.Add(textBox13);
            temp.Add(textBox12);
            temp.Add(textBox11);
            temp.Add(textBox10);
            temp.Add(textBox9);
            pidList.Add(temp);
            temp = new List<TextBox>();
            temp.Clear();
            temp.Add(textBox18);
            temp.Add(textBox17);
            temp.Add(textBox16);
            temp.Add(textBox15);
            temp.Add(textBox14);
            pidList.Add(temp);

            pidRbList.Clear();
            pidRbList.Add(button10);
            pidRbList.Add(button12);
            pidRbList.Add(button14);

            pidSbList.Clear();
            pidSbList.Add(button9);
            pidSbList.Add(button11);
            pidSbList.Add(button13);

            bluetooth.ValueChanged += eventRun;

        }

        //串口处理函数
        private int sp = 0;
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (anoStatus)
            {
                byte[] buff = new byte[1000];
                float[] data = new float[3];
                int num = serialPort1.BytesToRead;
                serialPort1.Read(buff, 0, num);
                if (buff[0] == 0xAA && buff[1] == 0xAA)
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
                    else
                    {
                        return;
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
                switch (serialType)
                {
                    case serialTypeS.normal:
                        //因为要访问UI资源，所以需要使用invoke方式同步ui
                        this.Invoke((EventHandler)(delegate
                        {
                            textBox1.AppendText(serialPort1.ReadExisting());
                        }
                            )
                        );
                        break;
                    case serialTypeS.status:
                        if(sp == 0)
                        {
                            string spType = serialPort1.ReadLine();
                            if (spType.Contains("status"))
                            {
                                sp = 1;
                            }
                            else if (spType.Contains("pidconf"))
                            {
                                sp = 2;
                                while (true)
                                {
                                    if (serialPort1.BytesToRead >= 25)
                                        break;
                                }
                            }
                        }
                        else
                        {
                            byte[] buff = new byte[1000];
                            int num = serialPort1.BytesToRead;
                            serialPort1.Read(buff, 0, num);
                            if (sp == 1)
                            {
                                sp = 0;
                                if (buff[0] == 0xAA && buff[1] == 0xAA)
                                {
                                    UInt16 temp, i = 0, j = 0;
                                    temp = BitConverter.ToUInt16(buff, 4);
                                    for (i = 0x0001, j = 0; i <= 0x0080; i <<= 1, j++)
                                    {

                                        //因为要访问UI资源，所以需要使用invoke方式同步ui
                                        this.Invoke((EventHandler)(delegate
                                        {
                                            if ((temp & i) != 0)
                                            {
                                                statusCheckBox[j].Checked = true;
                                                statusCheckBox[j].BackColor = Color.Red;
                                                statusCheckBox[j].ForeColor = Color.White;
                                            }
                                            else
                                            {
                                                statusCheckBox[j].Checked = false;
                                                statusCheckBox[j].BackColor = Color.Transparent;
                                                statusCheckBox[j].ForeColor = Color.Black;
                                            }
                                        }
                                            )
                                        );
                                    }
                                }
                            }
                            else if (sp == 2)
                            {
                                sp = 0;
                                if (buff[0] == 0xAA && buff[1] == 0xAA)
                                {
                                    /*int remainByte = buff[11 + 3] + 5 - (num - 11);
                                    if (remainByte > 0)
                                    {
                                        while (true)
                                        {
                                            if (serialPort1.ReadBufferSize >= remainByte)
                                                break;
                                        }
                                        serialPort1.Read(buff, num, remainByte);
                                    }*/
                                    UInt16 i = 0, j = 0;
                                    float P = BitConverter.ToSingle(buff, 4);
                                    float I = BitConverter.ToSingle(buff, 8);
                                    float D = BitConverter.ToSingle(buff, 12);
                                    float S = BitConverter.ToSingle(buff, 16);
                                    float V = BitConverter.ToSingle(buff, 20);
                                    //因为要访问UI资源，所以需要使用invoke方式同步ui
                                    this.Invoke((EventHandler)(delegate
                                    {
                                        pidList[pidLastRead][0].Clear();
                                        pidList[pidLastRead][0].Text += P.ToString();
                                        pidList[pidLastRead][1].Clear();
                                        pidList[pidLastRead][1].Text += I.ToString();
                                        pidList[pidLastRead][2].Clear();
                                        pidList[pidLastRead][2].Text += D.ToString();
                                        pidList[pidLastRead][3].Clear();
                                        pidList[pidLastRead][3].Text += S.ToString();
                                        pidList[pidLastRead][4].Clear();
                                        pidList[pidLastRead][4].Text += V.ToString();
                                    }
                                        )
                                    );
                                }
                            }
                        }
                        break;
                }
            }
        }

        #region 标签页控件
        //标签页切换中事件
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = !AllowSelect;
        }

        //标签页切换后事件
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            lastPage = currenPage;
            currenPage = e.TabPage;
            if (lastPage != currenPage)
            {
                if (lastPage == tabPage2)//ANO
                {
                    AnoSwitch(false);
                }
                else if (lastPage == tabPage4)//status
                {
                    //statusSwitch(false);
                    serialType = serialTypeS.normal;
                    timer1.Stop();
                }

                if (currenPage == tabPage2)//ANO
                {
                    AnoSwitch(true);
                }
                else if (currenPage == tabPage4)//status
                {
                    //statusSwitch(true);
                    serialType = serialTypeS.status;
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.WriteLine("spdconf");
                    }
                    timer1.Start();
                }
            }
        }

        #endregion

        #region 串口助手
        //发送框键盘点击事件
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && checkBox1.Checked)
            {
                e.Handled = true;
            }
        }

        //发送框键盘按下事件
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
        
        //接收框清空按钮
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        //发送框清空按钮
        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
        
        //串口发送按钮
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

        //串口发送函数
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

        #endregion

        #region 波形显示
        //检查波形类型为单选
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

        //波形显示开关函数
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

        //波形数据重新加载
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

        //波形数据平移
        private void UpdateQueueValue(int ch, Queue<float> dataQueue, float data)
        {

            while (dataQueue.Count > numericUpDown1.Value - 1)
            {
                dataQueue.Dequeue();
            }
            dataQueue.Enqueue(data);

            switch (ch)
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

        //波形数据解析
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

        //波形显示开关按钮
        private void button7_Click(object sender, EventArgs e)
        {
            if (anoStatus)
            {
                AnoSwitch(false);
            }
            else
            {
                AnoSwitch(true);
            }
        }

        //改变波形周期
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
        }

        //更换波形
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
            dataQueueCh1.Clear();
            dataQueueCh2.Clear();
            dataQueueCh3.Clear();
        }

        //更换通道
        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            zGraphLoadPix();
        }

        #endregion

        #region 状态及参数
        #region 状态指示
        //周期获取状态
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("status");
            }
        }
        #endregion
        
        #region PID设置
        //记录上次读取的是哪一组PID，用于正确显示读取回来的结果
        private int pidLastRead = -1;

        //读取一组PID
        private void pidRbClick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int i;
                for (i = 0; i < pidRbList.Count; i++)
                {
                    if (sender == pidRbList[i])
                    {
                        serialPort1.WriteLine("pidconf " + i);
                        pidLastRead = i;
                        break;
                    }
                }
            }
        }

        //设置一组PID
        private void pidSbClick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                int i;
                for (i = 0; i < pidSbList.Count; i++)
                {
                    if (sender == pidSbList[i])
                    {
                        serialPort1.Write("pidconf " + i + " ");
                        serialPort1.Write(pidList[i][0].Text + " ");
                        serialPort1.Write(pidList[i][1].Text + " ");
                        serialPort1.Write(pidList[i][2].Text + " ");
                        serialPort1.Write(pidList[i][3].Text + " ");
                        serialPort1.WriteLine(pidList[i][4].Text);
                        break;
                    }
                }
            }
        }
        #endregion

        //编码器校准
        private void button21_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("mlxaju");
            }
        }

        //IMU校准
        private void button22_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("imuaju");
            }
        }

        #endregion

        #region 固件升级
        //固件选择按钮
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox3.Text = openFileDialog.FileName.ToString();
            }
        }
        
        //固件修复按钮
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

                //for (i = 0; i < 30; i++)
                while(true)
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
                /*if (i >= 30)
                {
                    button6.Enabled = true;
                    button5.Enabled = true;
                    button8.Enabled = true;
                    AllowSelect = true;
                    return;
                }*/

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
        
        //固件升级按钮
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
        #endregion

        #region 串口设置
        //串口号更新
        private void comboBoxComNum_Click(object sender, EventArgs e)
        {
            String temp = (String)comboBoxComNum.SelectedItem;
            comboBoxComNum.Items.Clear();
            comboBoxComNum.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            int index = comboBoxComNum.FindStringExact(temp);
            if (index == -1)
                return;
            comboBoxComNum.SelectedIndex = index;
        }

        //串口开关按钮
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

        #endregion

        #region BT
        private static string serviceGuid              = "00009501-0000-1000-8000-00805F9B34FB";
        private static string writeCharacteristicGuid  = "00009511-0000-1000-8000-00805F9B34FB";
        private static string notifyCharacteristicGuid = "00009512-0000-1000-8000-00805F9B34FB";
        BluetoothLECode bluetooth = new BluetoothLECode(serviceGuid, writeCharacteristicGuid, notifyCharacteristicGuid);
        private void eventRun(MsgType type, string str, byte[] data = null)
        {
           
            //因为要访问UI资源，所以需要使用invoke方式同步ui
            this.Invoke((EventHandler)(delegate
            {
                textBox22.Text += str + "\r\n";
            }
                )
            );
        }

        private void button24_Click(object sender, EventArgs e)
        {
            bluetooth.Write(System.Text.Encoding.Default.GetBytes(textBox23.Text));
        }

        private void button25_Click(object sender, EventArgs e)
        {
            bluetooth.Dispose();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            bluetooth.CurrentDeviceName = "Nordic_UART_leon";
            bluetooth.StartBleDeviceWatcher();
        }
        #endregion
    }
}
