using System;
using System.Net;
using System.Net.Sockets;

namespace TCP_client
{
    public class TCP_Client
    {
        IPAddress ip = null;
        IPEndPoint remote = null;
        int port;
        Socket socket = null;

        public bool Connected { get; private set; }


        public TCP_Client(string ip, int port)
        {
            this.ip = Dns.GetHostEntry(ip).AddressList[0];
            this.port = port;
            remote = new IPEndPoint(this.ip, this.port);
        }

        public string Connect()
        {
            string result_message = string.Empty;
            try
            {
                socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(remote);

                result_message = (socket.Connected) ? $"[Connect] to: {socket.RemoteEndPoint}" : "Connecting";
                Connected = true;
            }
            catch (Exception ex) { result_message = ex.Message; Connected = false; }
            return result_message;
        }

        public string Send(byte[] message)
        {
            string result_message = string.Empty;
            try
            {
                socket.Send(message);
                result_message = $"[Send] to: {socket.RemoteEndPoint}";
            }
            catch(Exception ex) { result_message = ex.Message; }
            return result_message;
        }

        public (byte[], int) Recieve()
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = socket.Receive(buffer);
            Connected = bytesReceived > 0 ? true : false;
            return (buffer, bytesReceived);
        }
    }
}
