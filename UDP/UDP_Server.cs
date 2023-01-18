using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDP_Server
{
    public class UDP_Server
    {
        static void Main(string[] args)
        {
            //LocalEndPoint: возвращает локальную точку (объект типа EndPoint), по которой запущен сокет и по которой он принимает данные
            //RemoteEndPoint: возвращает адрес удаленного хоста, к которому подключен сокет (объект типа EndPoint)

            //udp: bind, reciveFrom/reciveFromAsync (для отправки данных SendTo/SendToAsync)
            //после завершения работы закрыть сокет: Close() или с самого начала обернуть в using(Socket socket = new Socket(...){}

            //перменые 
            int DATA = int.MinValue; // переменная от клиента
            char CHAR = char.MinValue; // переменная от клиента
            //server
            var port = 1000;
            var host = Dns.GetHostEntry("localhost");
            var ipAddr = host.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddr, port); // создали сервер и ip и портом

            using (var listener = new Socket(ipAddr.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
            {
                listener.ReceiveTimeout = 5000; // задаем 5 секунд таймаут на получение 
                listener.SendTimeout = 500;// задаем тайм аут на отправление
                listener.Bind(localEndPoint); //забили ip и порт у ОС

                byte[] buffer = new byte[256];
                Console.WriteLine($"Server is running and waiting for a connection on port: {port}");

                EndPoint remoteIp = new IPEndPoint(ipAddr, port); // создаем клиентский ip 

                
                var result = listener.ReceiveFrom(buffer, ref remoteIp); // даем клиенту созданный клиентсий ip и получаем сообщение

                DATA = BitConverter.ToInt32(buffer, 0); 
                CHAR = Convert.ToChar(Encoding.ASCII.GetString(buffer[(sizeof(int) - 1)..])[1]); //только так char получить
                
                Console.WriteLine($"Got: {DATA} and {CHAR}");
                //что-то своё
                string res = (DATA*2).ToString()+CHAR;
                listener.SendTo(Encoding.ASCII.GetBytes(res), remoteIp);
                Console.WriteLine($"Sent message to {remoteIp}");
                listener.Shutdown(SocketShutdown.Both);
            }
        }
    }
}
