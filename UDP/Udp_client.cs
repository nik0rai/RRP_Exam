using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace UDP_client
{
    public static class Udp_client
    {
        static void Main(string[] args)
        {
            var port = 1000;
            var host = Dns.GetHostEntry("localhost");
            var ipAddr = host.AddressList[0]; 
            var remoteEndPoint = new IPEndPoint(ipAddr, port); //ip адрес сервера

            //переменные
            int data = 5;
            char somesymbol = 'a';    
            
            using (var sender = new Socket(ipAddr.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
            {
                var message = ToBytes(data, somesymbol);
                int bytes = sender.SendTo(message, remoteEndPoint);
                Console.WriteLine($"Sent message containing: {data} and {somesymbol}");

                byte[] buffer = new byte[256];
                var res = sender.Receive(buffer);
                Console.WriteLine($"Got message: {Encoding.ASCII.GetString(buffer, 0, res)}");
                sender.Shutdown(SocketShutdown.Both);
            }
        }

        public static byte[] ToBytes(params object[] input)
        {
            List<int> individual_size = new(); // у каждого параметра есть индивидуальный размер - int 4, char 1

            int max_size = 0; //у нас может быть несколько параметров разных типов
            for (int i = 0; i < input.Length; i++)
            {
                int jumper = Marshal.SizeOf(input[i]);
                individual_size.Add(jumper);
                max_size += jumper;
            }

            var result = new byte[max_size];

            GCHandle gcHandle = new(); // предоставляет способ доступа к управляемому объеккту из неуправляемой памяти
            int index = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].GetType() == typeof(char)) input[i] = Convert.ToUInt16(input[i]); //ASCII support

                gcHandle = GCHandle.Alloc(input[i], GCHandleType.Pinned); // работаем с адресом закрепелнного объекта, таким образом запрещаем сборщику мусора перемещать объект
                Marshal.Copy(gcHandle.AddrOfPinnedObject(), result, index, individual_size[i]); // аналог ToByte
                index += individual_size[i]; //новый элемент будет находится с новго индекса
            }
            gcHandle.Free();// освобождаем дескриптор
            return result;
        }
    }
}
