using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using TrainUDP;

namespace UDPClient
{
    
    //For message header structure
    struct MessageHeader
    {
        public byte MessageID;
        public byte MessageInterface;
        public byte PortNumber;
        public ushort MessageByteCount;
    }
    public partial class Form1 : Form
    {
        private bool isConnected = false;



        public TextBox TxtMessageHistory => txtMessageHistory;
        //starting as client for UDP
        UdpClient udpClient;
        IPEndPoint ep;

        //bağlamaya çalışıyoryum
        static Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public int PORT;

        private UDPServer udpServer;
        private Task _;

        public Form1()
        {
            InitializeComponent();

            udpServer = new UDPServer(this);
        }

        
        private void UdpServer_MessageReceived(string message)
        {
            // Update the UI in a thread-safe manner
            TxtMessageHistory.Invoke(new Action(() =>
            {
                TxtMessageHistory.AppendText("Received: " + message + Environment.NewLine);
                AppendMessageToHistory(message);
            }));
        }
        
        private void AppendMessageToHistory(string message)
        {
            MessageHeader mh = new MessageHeader
            {
                MessageID = 0x01,
                MessageInterface = 0x02,
                PortNumber = 0x03,
                MessageByteCount = (ushort)(5 + Encoding.UTF8.GetByteCount(message))
            };
            TxtMessageHistory.AppendText("------------Serverden Gelen Mesaj------------" + Environment.NewLine);
            TxtMessageHistory.AppendText("messageID: b'\xc" + mh.MessageID + "'" + Environment.NewLine);
            TxtMessageHistory.AppendText("messageInterface: b'\xe" + mh.MessageInterface + "'" + Environment.NewLine);
            TxtMessageHistory.AppendText("messagePortNum: " + mh.PortNumber + Environment.NewLine);
            TxtMessageHistory.AppendText("messageByteCount: " + mh.MessageByteCount + Environment.NewLine);
            TxtMessageHistory.AppendText("--------------------------------------------------" + Environment.NewLine);

            TxtMessageHistory.Invoke(new Action(() =>
            {
                TxtMessageHistory.AppendText(message + Environment.NewLine);
            }));



        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            txtMessages.Text += $"0x11 -> 1.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x12 -> 2.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x13 -> 3.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x14 -> 4.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x15 -> 5.Mesaj {Environment.NewLine}";


            this.BackColor = Color.LightBlue;
            btnSaveMessage.Enabled = false;
            btnUpdate.Enabled = false;

        }

        private void txtMessageHistory_TextChanged(object sender, EventArgs e)
        {
            string firstMessage = txtMessageHistory.Text;
            firstMessage = $"A Message from Server: Hello";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            string ipAdressText = txtIP.Text;

            
            try
            {
                s.Connect(new IPEndPoint(IPAddress.Parse(ipAdressText), PORT));
                txtMessageHistory.Text += $"Servera Bağlanıldı: " + ipAdressText  + Environment.NewLine;
                txtMessageHistory.Text += $"Serverdan gelen mesaj: Serverdan Merhaba{Environment.NewLine}";
                txtColor.BackColor = Color.Green;
                btnUpdate.Enabled = true;

                
            }
            catch (Exception ex)
            {

                txtMessageHistory.Text = $"Error {Environment.NewLine}" +ex.Message;
                txtColor.BackColor = Color.Red;

                if (!isConnected)
                {
                    txtMessageHistory.Text += $"{Environment.NewLine}Try Again {Environment.NewLine}";
                    txtMessageHistory.Text += $"-----------------------------------------------------------";
                }




            }
        }

        private async Task SimulateDelayAndStartListening()
        {
            // Get the delay value from the txtDelay TextBox
            int delayMilliseconds;
            if (!int.TryParse(txtDelay.Text, out delayMilliseconds))
            {
                //MessageBox.Show("Invalid delay value. Please enter a valid integer.");
                return;
            }

            // Simulate the delay using Task.Delay
            await Task.Delay(delayMilliseconds);

            // Start the UDP server to listen for messages
            udpServer.StartListening();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            btnSaveMessage.Enabled = true;

            //await SimulateDelayAndStartListening();

            string answer = cmbAnswer.SelectedItem.ToString();
            string delay = txtDelay.Text;

            MessageHeader mh = new MessageHeader
            {
                MessageID = 0x01,
                MessageInterface = 0x02,
                PortNumber = 0x03,
                MessageByteCount = (ushort)(5 + Encoding.UTF8.GetByteCount(answer))
            };

            ////////////////
            byte[] headerBytes = new byte[5];
            headerBytes[0] = mh.MessageID;
            headerBytes[1] = mh.MessageInterface;
            headerBytes[2] = mh.PortNumber;
            headerBytes[3] = (byte)(mh.MessageByteCount & 0xFF);
            headerBytes[4] = (byte)((mh.MessageByteCount >> 8) & 0xFF);

            // convert message to BYTEs
            byte[] bodyBytes = Encoding.UTF8.GetBytes(answer);

            byte[] fullMessage = new byte[headerBytes.Length + bodyBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, fullMessage, 0, headerBytes.Length);
            Buffer.BlockCopy(bodyBytes, 0, fullMessage, headerBytes.Length, bodyBytes.Length);
            //////////

            byte[] data = Encoding.UTF8.GetBytes(answer);
            byte[] data2 = Encoding.UTF8.GetBytes(delay);



            try
            {
                s.Send(data);

                if (cmbAnswer.SelectedItem.ToString() == "0x11")
                {
                    txtMessageHistory.Text += $"------------Clientten Gönderilen Mesaj------------{Environment.NewLine}";
                    txtMessageHistory.Text += $"messageID: b'\'x{cmbAnswer.SelectedItem}'{Environment.NewLine}";
                    txtMessageHistory.Text += $"messageInterface:b'\xc{txtDelay.Text}'{Environment.NewLine}";
                    txtMessageHistory.Text += $"messagePortNum: {txtPort.Text}{Environment.NewLine}";
                    txtMessageHistory.Text += $"messageByteCount: {mh.MessageByteCount}{Environment.NewLine}";
                    txtMessageHistory.Text += $"dataBytes: b'1.Mesaj' {Environment.NewLine}";
                    txtMessageHistory.Text += $"--------------------------------------------------{Environment.NewLine}";
                }

            }
            catch(Exception ex)
            {
                txtMessageHistory.Text += $"Error: {ex.Message}{Environment.NewLine}";
            }
            

        }

        private void btnSaveMessage_Click(object sender, EventArgs e)
        {
            string not = txtMessageHistory.Text;

            if (!string.IsNullOrEmpty(not))
            {
                try
                {
                    string path = Path.Combine(@"C: /Users/515227/Documents/Visual Studio 2015/Projects/UDPClient/TrainUDP/notlar.txt");

                    File.AppendAllText(path, not + Environment.NewLine);
                    MessageBox.Show("Notes are saved!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error" + ex.Message);
                }

            }
            else
            {
                MessageBox.Show("Here is Empty!");
            }
        }
    }
}

