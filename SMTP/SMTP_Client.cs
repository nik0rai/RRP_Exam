using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMTP_Client
{
    public class SMTP_Client
    {
        static async Task Main(string[] args)
        {
            var server = "smtp.mail.ru"; //smtp.mail.ru
            var port = int.Parse("465"); //port 465
            var server_name = new IPEndPoint(Dns.GetHostEntry(server).AddressList[0], port);

            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) //using вызовет Close в конце
            {
                await client.ConnectAsync(server_name);
                var socketStream = new NetworkStream(client);
                SslStream ssl_stream = new(socketStream); //SSL поддержка
                await ssl_stream.AuthenticateAsClientAsync(server);

                while (true)
                {
                    Console.Write(">> ");
                    var message = Console.ReadLine() + "\r\n"; //надо по протоколу добавлять \r\n
                    var result = await SendCommand(ssl_stream, message);
                    if (result.StartsWith("500 ")) result = await SendCommand(ssl_stream, message); //странно

                    Console.WriteLine($"[Recvd]: {result}");
                    if (result.StartsWith("221 ")) break;
                }
                client.Shutdown(SocketShutdown.Both);
            }
        }

        //Дальше собирать сообщение по протоколу

        static async Task<string> SendCommand(SslStream sslSteam, string command)
        {
            sslSteam.Write(Encoding.UTF8.GetBytes(command));

            byte[] buffer = new byte[2048];
            int bytes = await sslSteam.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytes);
        }
    }
}
}
