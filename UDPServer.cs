using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPClient;

namespace TrainUDP
{
    class UDPServer
    {
        private IPEndPoint clientEndPoint;
        // Reference to the Form1 instance (you need to set this when creating the UDPServer)
        private Form1 form;
        private UdpClient udpServer;


        public delegate void MessageReceivedEventHandler(object sender, string message);
        
        // Define the event using the delegate
        public event MessageReceivedEventHandler MessageReceived;



        public UDPServer(Form1 form)
        {
            this.form = form;
            udpServer = new UdpClient(5000); // 
        }

        public void StartListening()
        {
            try
            {
                while (true)
                {
                    byte[] data = udpServer.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(data);

                    //OnMessageReceived(message);


                    if (form.CmbAnswer.SelectedItem.ToString() == "0x11")
                    {
                        form.Invoke(new Action(() =>
                        {
                            AppendMessageToHistory();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public void AppendMessageToHistory()
        {
            string answer = form.CmbAnswer.SelectedItem.ToString();
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




            form.TxtMessageHistory.AppendText("------------Message From Server------------" + Environment.NewLine);
            form.TxtMessageHistory.AppendText("messageId: "+ mh.MessageID + Environment.NewLine);
            form.TxtMessageHistory.AppendText("messageInterface: " + mh.MessageInterface + Environment.NewLine);
            form.TxtMessageHistory.AppendText("messagePortNum: " + form.TxtPortNum.Text + Environment.NewLine);
            form.TxtMessageHistory.AppendText("messageCount: " + mh.MessageByteCount + Environment.NewLine);
            form.TxtMessageHistory.AppendText("-------------------------------------------" + Environment.NewLine);


        }

        /*
        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }
        */


        
    }

}
