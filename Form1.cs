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
using System.Threading;

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

    //For Excel structure
    struct IpPortData
    {
        public string IpAddress;
        public int Port;
    }

    public partial class Form1 : Form
    {
        private bool isConnected = false;
        public TextBox TxtMessageHistory => txtMessageHistory;
        public ComboBox CmbAnswer => cmbAnswer;
        public TextBox TxtPortNum => txtPort;

        private List<IpPortData> ipPortList = new List<IpPortData>();

        //starting as client for UDP
        UdpClient udpClient;
        IPEndPoint ep;

        //bağlamaya çalışıyoryum
        static Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public int PORT;

        private UDPServer udpServer;

        public Form1()
        {
            InitializeComponent();

            udpServer = new UDPServer(this);
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDataFromExcel();
            txtMessages.Text += $"0x11 -> 1.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x12 -> 2.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x13 -> 3.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x14 -> 4.Mesaj {Environment.NewLine}";
            txtMessages.Text += $"0x15 -> 5.Mesaj {Environment.NewLine}";


            this.BackColor = Color.LightBlue;
            btnSaveMessage.Enabled = false;
            btnUpdate.Enabled = false;

        }
        private void LoadDataFromExcel()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("IP Address", typeof(string));
                dataTable.Columns.Add("Port", typeof(int));

                dataTable.Rows.Add("192.0.0.0", 55);
                dataTable.Rows.Add("192.0.0.1", 56);
                dataTable.Rows.Add("192.0.0.2", 57);

                ipPortList = dataTable.AsEnumerable().Select(row => new IpPortData
                {
                    IpAddress = row.Field<string>("IP Address"),
                    Port = row.Field<int>("Port")
                }).ToList();
                

            }
            catch (Exception ex)
            {

            }
        }

        private bool IsValidIpAndPort(string ip, int port)
        {
            return ipPortList.Any(item => item.IpAddress == ip && item.Port == port);
        }

        private string previousIP = "";
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string enteredIp = txtIP.Text;
            int enteredPort;
            
            string ipAdressText = txtIP.Text;
            try
            {
                if (!int.TryParse(txtPort.Text, out enteredPort))
                {
                    MessageBox.Show("Invalid port number. Please enter a valid integer.");
                    txtColor.BackColor = Color.Red;
                    return;
                }
                s.Connect(new IPEndPoint(IPAddress.Parse(ipAdressText), PORT));
                if (IsValidIpAndPort(enteredIp, enteredPort))
                {
                    // Başarılı bir şekilde eşleştiğinde buraya gelecek
                    //txtMessageHistory.Text += $"Connected: {enteredIp}:{enteredPort}{Environment.NewLine}";
                    txtMessageHistory.Text += $"Servera Bağlanıldı: " + ipAdressText + "/" + txtPort.Text + Environment.NewLine;
                    txtMessageHistory.Text += $"Serverdan gelen mesaj: Serverdan Merhaba{Environment.NewLine}";
                    txtColor.BackColor = Color.Green;
                }
                /*
                txtMessageHistory.Text += $"Servera Bağlanıldı: " + ipAdressText  + "/" + txtPort.Text +  Environment.NewLine;
                txtMessageHistory.Text += $"Serverdan gelen mesaj: Serverdan Merhaba{Environment.NewLine}";
                */

                string currentIP = txtIP.Text.Trim();
                if (currentIP != previousIP)
                {
                    txtMessageHistory.AppendText($"Disconnected: {previousIP}{Environment.NewLine}");
                    txtMessageHistory.AppendText($"Connected: {currentIP}{Environment.NewLine}");

                    // Update the previousIP variable with the current IP for the next comparison
                    previousIP = currentIP;
                };

                //txtColor.BackColor = Color.Green;
                btnUpdate.Enabled = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("IP address and port do not match any valid entry in the Excel list.");
                txtMessageHistory.Text = $"Error {Environment.NewLine}" + ex.Message;
                txtColor.BackColor = Color.Red;
                
                /*
                txtMessageHistory.Text = $"Error {Environment.NewLine}" +ex.Message;
                txtColor.BackColor = Color.Red;
                */

                if (!isConnected)
                {
                    txtMessageHistory.Text += $"{Environment.NewLine}Try Again {Environment.NewLine}";
                    txtMessageHistory.Text += $"-----------------------------------------------------------";
                }
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            btnSaveMessage.Enabled = true;
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


            //Thread.Sleep(txtDelay.Text * 1000); Temel şey
            int delayMilliseconds;
            if (int.TryParse(txtDelay.Text, out delayMilliseconds))
            {
                Thread.Sleep(delayMilliseconds * 1000);
            }
            
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


                    udpServer.AppendMessageToHistory();
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

