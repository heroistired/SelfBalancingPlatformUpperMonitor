using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Drawer
{
    public delegate void ShowWindow();
    public delegate void HideWindow();
    public delegate void OpenPort();
    public delegate void ClosePort();
    public delegate Point GetMainPos();
    public delegate int GetMainWidth();
    public partial class MainForm : Form
    {
        private int count = 0, speed;
        private float pre_pitch = 0, pre_yaw = 0, pre_roll = 0, now_pitch = 0, now_yaw = 0, now_roll = 0, kp, ki, kd;
        private string buffer = ""; //存储某次接收时被截断的指令，以便下次拼接
        private byte[] buffer1 = new byte[11]; //存储某次接收时被截断的指令，以便下次拼接，自动初始化为0
        private int buffer_index = 0;
        private bool frame_head_founded = false; //数据帧头是否找到
        private bool textbox_show_data = false; //是否显示串口数据是否找到
        //上位机发送给下位机的命令
        public const byte CMD_UP = (byte)201;
        public const byte CMD_DOWN = (byte)202;
        public const byte CMD_LEFT = (byte)203;
        public const byte CMD_RIGHT = (byte)204;
        public const byte CMD_POS_RESET = (byte)205;
        public const byte CMD_KP_PLUS_1 = (byte)206;
        public const byte CMD_KP_MINUS_1 = (byte)207;
        public const byte CMD_KI_PLUS_1 = (byte)208;
        public const byte CMD_KI_MINUS_1 = (byte)209;
        public const byte CMD_KD_PLUS_1 = (byte)210;
        public const byte CMD_KD_MINUS_1 = (byte)211;
        public const byte CMD_PID_PRARMETRE_STEP_1 = (byte)212;
        public const byte CMD_KP_PLUS_2 = (byte)213;
        public const byte CMD_KP_MINUS_2 = (byte)214;
        public const byte CMD_KI_PLUS_2 = (byte)215;
        public const byte CMD_KI_MINUS_2 = (byte)216;
        public const byte CMD_KD_PLUS_2 = (byte)217;
        public const byte CMD_KD_MINUS_2 = (byte)218;
        public const byte CMD_PID_PRARMETRE_STEP_2 = (byte)219;
        //下位机发送给上位机的命令
        UInt16 CMD_PITCH =  1001;
        UInt16 CMD_YAW = 1002;
        UInt16 CMD_ROLL = 1003;
        UInt16 CMD_DEGREE1 = 1004;
        UInt16 CMD_DEGREE2 = 1005;
        UInt16 CMD_KP = 1006;
        UInt16 CMD_KI = 1007;
        UInt16 CMD_KD = 1008;
        UInt16 CMD_CTROUT1 = 1009;
        UInt16 CMD_CTROUT2 = 1010;
        UInt16 CMD_FELLOW_TEST = 1011;
        UInt16 CMD_CLOCK = 1012;

        //曲线数据
        public List<float> x_Excitation = new List<float>();
        public List<float> y_Excitation = new List<float>();
        public List<float> x_pitch = new List<float>();
        public List<float> y_pitch = new List<float>();
        public List<float> x_yaw = new List<float>();
        public List<float> y_yaw = new List<float>();
        public List<float> x4 = new List<float>();
        public List<float> y4 = new List<float>();
        public List<float> x5 = new List<float>();
        public List<float> y5 = new List<float>();
        //曲线相关的变量
        public int count_fellow_test = 0;
        public int count_pitch = 0;
        public int count_clock = 0; //所有要绘制的曲线统一的时钟
        private bool fellow_test_curce_added = false;
        private bool pitch_curce_added = false;
        private bool yaw_curce_added = false;
        private bool draw_fellow_test_curce = false;
        private bool draw_pitch_curce = false;
        private bool draw_yaw_curce = false;

        public MainForm()
        {
            InitializeComponent();
            serialPort1.Encoding = Encoding.GetEncoding("GB2312");                                  //串口接收编码
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;                   //
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
                string data = buffer + serialPort1.ReadExisting();
                string[] sArray = data.Split(' ');

                if (data[data.Length - 1] != '/')
                {
                    buffer = " " + sArray[sArray.Length - 1];
                    sArray[sArray.Length - 1] = "";
                }

                foreach (string i in sArray)
                {
                    //label26.Text = data;
                    if (i.Length > 4)
                    {
                        count++;
                        if(i.Substring(1, 4) == "1001") //CMD_PITCH
                        {
                            try
                            {
                                now_pitch = float.Parse(i.Substring(5, i.Length - 6));
                            }
                            catch
                            { }
                            label16.Text = (now_pitch/10).ToString() + "°";
                            count++;
                        }
                        else if (i.Substring(1, 4) == "1002") //CMD_YAW
                        {
                            try
                            {
                                now_yaw = float.Parse(i.Substring(5, i.Length - 6));
                            }
                            catch
                            {}
                            label17.Text = (now_yaw / 10).ToString() + "°";
                            count++;
                        }
                        else if (i.Substring(1, 4) == "1003") //CMD_ROLL
                        {
                        }
                        else if (i.Substring(1, 4) == "1004") //CMD_DEGREE1
                        {
                            try
                            {
                                label20.Text = (float.Parse(i.Substring(5, i.Length - 6)) / 10).ToString() + "°";
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1005") //CMD_DEGREE2
                        {
                            try
                            {
                                label22.Text = (float.Parse(i.Substring(5, i.Length - 6)) / 10).ToString() + "°";
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1008") //CMD_KD
                        {
                            try
                            {
                                label28.Text = (float.Parse(i.Substring(5, i.Length - 6)) / 10000).ToString(); 
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1006") //CMD_KP
                        {
                            try
                            {
                                label26.Text = (float.Parse(i.Substring(5, i.Length-6)) / 10000).ToString(); 
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1007") //CMD_KI
                        {
                            try
                            {
                                label27.Text = (float.Parse(i.Substring(5, i.Length - 6)) / 10000).ToString(); 
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1009") //CMD_CTROUT1
                        {
                            try
                            {
                                label32.Text = (float.Parse(i.Substring(5, i.Length - 6)) / 10).ToString();
                            }
                            catch
                            { }
                        }
                        else if (i.Substring(1, 4) == "1010") //CMD_CTROUT2
                        {
                        }
                        else if (i.Substring(1, 4) == "1011") //CMD_FELLOW_TEST
                        {
                        }
                    }
                }
                    textBox1.AppendText(data);                                //串口类会自动处理汉字，所以不需要特别转换
        }



        private void button4_Click(object sender, EventArgs e)
        {
            if (textbox_show_data)
            {
                button4.Text = "开启数据显示";
                textbox_show_data = false;
            }
            else
            {
                button4.Text = "关闭数据显示";
                textbox_show_data = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //初始化各个控件的属性
            comboBox1.Items.Add("无");
            comboBox1.Text = "无";
            comboBox2.Text = "4800";
            kp = float.Parse(label26.Text);
            ki = float.Parse(label27.Text);
            kd = float.Parse(label28.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开端口")
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;                                              //端口号
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);                             //波特率
                    serialPort1.Open();                                                                 //打开串口
                    button1.Text = "关闭端口";
                }
                catch
                {
                    MessageBox.Show("端口错误", "错误");
                }
            }
            else
            {
                try
                {
                    serialPort1.Close();                                                                 //打开串口
                    button1.Text = "打开端口";
                }
                catch
                {
                    MessageBox.Show("端口错误", "错误");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);       //扫描并讲课用串口添加至下拉列表
        }

        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {
            //将可用端口号添加到ComboBox
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            for (int i = 1; i < 20; i++)
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下拉列表
                    MyPort.Close();                                     //关闭
                }
                catch
                {
                }
            }
            try
            {
                MyBox.Text = MyBox.Items[0].ToString();
            }
            catch
            { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //当选择“自定义”时，下拉菜单是可以编辑的
            if (comboBox2.Text == "自定义")
                comboBox2.DropDownStyle = ComboBoxStyle.DropDown;
            else if (comboBox2.DropDownStyle != ComboBoxStyle.DropDownList)
                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            count = 0;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_UP, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_LEFT, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_RIGHT, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_DOWN, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_POS_RESET, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KP_PLUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }  
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KP_MINUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KI_PLUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KI_MINUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }
        

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KD_PLUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }
        

        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KD_MINUS_1, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }
        
        //CMD 指令 WithData 是否发送数据，为false时，后面两个参数将被忽略 data 数据 CMD1 保留 
        private void SendCmdData(byte CMD, bool WithData, byte CMD1, int data)
        {
            byte[] data_byte =   System.BitConverter.GetBytes(data);
            Array.Reverse(data_byte); 
            byte[] CMDandDATAtoSEND = new byte[4 + data_byte.Length];
            if (!WithData)
            {
                CMDandDATAtoSEND[0] = CMD;
                CMDandDATAtoSEND[1] = (byte)0x0d;
                CMDandDATAtoSEND[2] = (byte)0x0a;
                try
                {
                    serialPort1.Write(CMDandDATAtoSEND, 0, 3);
                }
                catch
                {
                    MessageBox.Show("串口数据写入错误", "错误");
                }
            }
            else
            {
                CMDandDATAtoSEND[0] = CMD;
                CMDandDATAtoSEND[1] = CMD1;
                for (int i = 0; i < data_byte.Length; i++)
                    CMDandDATAtoSEND[2 + i] = data_byte[i];
                CMDandDATAtoSEND[data_byte.Length + 2] = (byte)0x0d;
                CMDandDATAtoSEND[data_byte.Length + 3] = (byte)0x0a;
                try
                {
                    serialPort1.Write(CMDandDATAtoSEND, 0, 4 + data_byte.Length);
                }
                catch
                {
                    MessageBox.Show("串口数据写入错误", "错误");
                }
            }
        }
        private void ChangePidParanetreStep()
        {
            int data;
            data = (int)(float.Parse(textBox3.Text) * 10000);
            if (data != 0)
            {
                if (data > 0)
                    SendCmdData(CMD_PID_PRARMETRE_STEP_1, true, (byte)1, data);
                else
                    SendCmdData(CMD_PID_PRARMETRE_STEP_1, true, (byte)0, data);
            }
            else
            {
                textBox3.Text = (0.0001).ToString();
                SendCmdData(CMD_PID_PRARMETRE_STEP_1, true, (byte)1, 1);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ChangePidParanetreStep();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_POS_RESET, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KP_PLUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            } 
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KP_MINUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KI_PLUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KI_MINUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KD_PLUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                SendCmdData(CMD_KD_MINUS_2, false, (byte)0, 0);
            }
            catch
            {
                MessageBox.Show("串口数据写入错误", "错误");
            }
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void button17_Click_1(object sender, EventArgs e)
        {

        }

        private void timerDraw_Tick(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                draw_pitch_curce = true;
            else
            {
                draw_pitch_curce = false;
                pitch_curce_added = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                draw_yaw_curce = true;
            else
            {
                draw_yaw_curce = false;
                yaw_curce_added = false;
            }
        }
    }
}