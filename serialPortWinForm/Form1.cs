using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace serialPortWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            updatePorts();
            init();
        }

        private void init()
        {
            cmbBaudRate.SelectedIndex = 7;
            cmbDataBits.SelectedIndex = 1;
            cmbParity.SelectedIndex = 0;
            cmbPortName.SelectedIndex = 0;
            cmbStopBits.SelectedIndex = 0;
        }

        private void updatePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cmbPortName.Items.Add(port);
            }
        }
        //private SerialPort serialPort1 = new SerialPort();
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                disconnect();
            }
            else
            {
                connect();
            }
        }

        private void disconnect()
        {

            serialPort1.Close();
            btnConnect.Text = "Connect";
            btnSend.Enabled = false;
            groupBox1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }

        private void connect()
        {

            bool error = false;
            // Check if all settings have been selected
            if (cmbPortName.SelectedIndex != -1 & cmbBaudRate.SelectedIndex != -1 &
            cmbParity.SelectedIndex != -1 & cmbDataBits.SelectedIndex != -1 &
            cmbStopBits.SelectedIndex != -1)
            {  //if yes than Set The Port's settings
                serialPort1.PortName = cmbPortName.Text;
                serialPort1.BaudRate = int.Parse(cmbBaudRate.Text); //convert Text                to Integer
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                //convert Text to Parity
                serialPort1.DataBits = int.Parse(cmbDataBits.Text); //convert Text                to Integer
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits),
                cmbStopBits.Text); //convert Text to stop bits
                try //always try to use this try and catch method to open your port.
                    //if there is an error your program will not display a message
                    //instead of freezing.
                {
                    //Open Port
                    serialPort1.Open();
                }
                catch (UnauthorizedAccessException) { error = true; }
                catch (System.IO.IOException) { error = true; }
                catch (ArgumentException) { error = true; }
                if (error) MessageBox.Show(this, "Could not open the COM port. Most likely it is already in use, has been removed, or is unavailable.", "COM Port unavailable", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                MessageBox.Show("Please select all the COM Serial Port Settings", "Serial Port Interface", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            //if the port is open, Change the Connect button to disconnect, enable the send button.
            //and disable the groupBox to prevent changing configuration of an open
            //port.
            if (serialPort1.IsOpen)
            {
                btnConnect.Text = "Disconnect";
                btnSend.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                if (!rdText.Checked & !rdHex.Checked) //if no data mode is selected, then select text mode by default
                {
                    rdText.Checked = true;
                }
                groupBox1.Enabled = false;
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            //Clear the screen
            rtxtDataArea.Clear();
            txtSend.Clear();
        }

        private void sendData()
        {

            bool error = false;
            if (rdText.Checked == true) //if text mode is selected, send data as                tex
            {
                // Send the user's text straight out the port
                serialPort1.Write(txtSend.Text);
                // Show in the terminal window
                rtxtDataArea.AppendText(txtSend.Text + "\n");
                txtSend.Clear(); //clear screen after sending data
            }
            else //if Hex mode is selected, send data in hexadecimal
            {
                try
                {
                    // Convert the user's string of hex digits (example: E1 FF 1B) to a
                    byte array;
                    byte[] data = HexStringToByteArray(txtSend.Text);
                    // Send the binary data out the port
                    serialPort1.Write(data, 0, data.Length);
                    // Show the hex digits on in the terminal window
                    rtxtDataArea.AppendText(txtSend.Text.ToUpper() + "\n");
                    txtSend.Clear(); //clear screen after sending                    data
                }
                catch (FormatException) { error = true; }
                // Inform the user if the hex string was not properly formatted
                catch (ArgumentException) { error = true; }
                if (error) MessageBox.Show(this, "Not properly formatted hex string: " +
                txtSend.Text + "\n", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
        private string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
        private void BtnSend_Click(object sender, EventArgs e)
        {
            sendData();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(10);
            string recievedData = serialPort1.ReadExisting(); //read all available data in the receiving buffer.
                                                              // Show in the terminal window

            rtxtDataArea.Invoke(new Action(() => rtxtDataArea.AppendText(recievedData + "\n")));
        }

        private void SerialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void RtxtDataArea_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            rtxtDataArea.SelectionStart = rtxtDataArea.Text.Length;
            // scroll it automatically
            rtxtDataArea.ScrollToCaret();
        }


        private void TxtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return))
            {
                btnSend.PerformClick();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            txtSend.Text = "A";
            btnSend.PerformClick();
        }

        private void Button2_Click(object sender, EventArgs e)
        {

            txtSend.Text = "B";
            btnSend.PerformClick();
        }

        private void Button3_Click(object sender, EventArgs e)
        {

            txtSend.Text = "C";
            btnSend.PerformClick();
        }

        private void Button4_Click(object sender, EventArgs e)
        {

            txtSend.Text = "D";
            btnSend.PerformClick();
        }

        private void Button5_Click(object sender, EventArgs e)
        {

            txtSend.Text = "E";
            btnSend.PerformClick();
        }
    }
}
