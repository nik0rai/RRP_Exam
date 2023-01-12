using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_client
{
    public class TCP_client
    {
        static void Main(string[] args)
        {
            var port = 1000;
            var host = Dns.GetHostEntry("localhost");
            var ipAddr = host.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddr, port);

            using(var sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try //если не можем подключиться то прокидываем ошибку и закрываемся
                {
                    sender.Connect(remoteEndPoint); 
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); return; }
                Console.WriteLine($"Connected to: {sender.RemoteEndPoint}");

                byte[] buffer = new byte[1024];
                string message = string.Empty;
                while (message.IndexOf("<EOF>") < 0)
                {
                    Console.Write(">>");
                    message = Console.ReadLine(); //переменная будет тут просто нужно обернуть это всё в строку для протокола

                    byte[] msg = Encoding.ASCII.GetBytes(message); //нужен русский? использовать UTF8 везде где ASCII

                    try
                    {
                        sender.Send(msg);
                    }
                    catch(Exception ex) { Console.WriteLine(ex.Message); return; }

                    int bytesReceived = sender.Receive(buffer);
                    Console.WriteLine($"Got message: {Encoding.ASCII.GetString(buffer, 0, bytesReceived)}");
                }
                sender.Shutdown(SocketShutdown.Both);
            }
        }
    }
}
