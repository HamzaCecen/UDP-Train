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

        // ... (existing code)

        // Reference to the Form1 instance (you need to set this when creating the UDPServer)
        private Form1 form;
        private UdpClient udpServer;


        public delegate void MessageReceivedEventHandler(string message);

        public event EventHandler<string> MessageReceived;
        // Define the event using the delegate
        //public event MessageReceivedEventHandler MessageReceived;



        public UDPServer(Form1 form)
        {
            this.form = form;
            udpServer = new UdpClient(5000); // Listening on port 5000.
        }

        public void StartListening()
        {
            try
            {
                while (true)
                {
                    byte[] data = udpServer.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(data);

                    OnMessageReceived(message);

                    form.Invoke(new Action(() =>
                    {
                        form.TxtMessageHistory.Text = $"Hello {Environment.NewLine}";
                    }));
                    /*
                    // Update the UI in a thread-safe manner
                    form.Invoke(new Action(() =>
                    {
                        form.TxtMessageHistory.AppendText("Received: " + message + Environment.NewLine);
                        //AppendMessageToHistory();

                    }));

                    // ... (existing code)
                    */
                }
            }
            catch (Exception ex)
            {
                // ... (existing code)
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            /*
            // Check if there are any subscribers to the event
            if (MessageReceived != null)
            {
                // Invoke the event with the received message
                MessageReceived(message);
            }
            */
            MessageReceived?.Invoke(this, message);
        }


        
    }

}
