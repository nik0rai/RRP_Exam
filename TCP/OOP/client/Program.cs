using System;
using System.Text;

namespace TCP_client
{
    public class Program
    {
        static void Main(string[] args)
        {
            var port = 1000;
            var ip = "localhost";
            TCP_Client tcpClient = new(ip, port);
            var conn = tcpClient.Connect();
            Console.WriteLine(conn);
            while (tcpClient.Connected)
            {
                Console.Write(">> "); var message = Console.ReadLine();
                Console.WriteLine(tcpClient.Send(Utils.ToBytes(message, Encoding.UTF8)));
                var get = tcpClient.Recieve(); //item1 => массив байтов, item2 => кол-во полученных байт
                var rcv = Utils.ToString(get.Item1, get.Item2, Encoding.UTF8);
                Console.WriteLine($"[Got]: {rcv}");
            }
        }
    }
}