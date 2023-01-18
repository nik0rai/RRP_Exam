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
            var port = 1000;// сами задаем порт
            var host = Dns.GetHostEntry("localhost");
            var ipAddr = host.AddressList[0]; //localhost переводим в цифры 127.0.0.1
            var remoteEndPoint = new IPEndPoint(ipAddr, port); //ip:port

            using(var sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp)) // обертка, после работы которой вызывается Dispose (socket.close)
            {
                try //пытаемся подключиться к серверу
                {
                    sender.Connect(remoteEndPoint); 
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); return; } //если не можем подключиться то прокидываем ошибку и закрываемся
                
                Console.WriteLine($"Connected to: {sender.RemoteEndPoint}");

                byte[] buffer = new byte[1024];
                string message = string.Empty; //само сообщение
                while (message.IndexOf("<EOF>") < 0) // е6сли в сообщении от пользователя будет <EOF>, то закончить работу
                {
                    Console.Write(">>");
                    message = Console.ReadLine(); // считываем данные в формате строки (переменная будет тут просто нужно обернуть это всё в строку для протоко)ла

                    byte[] msg = Encoding.ASCII.GetBytes(message); //переводим считанную строку в байты (нужен русский? использовать UTF8 везде где ASCII)

                    try 
                    {
                        sender.Send(msg);
                    }
                    catch(Exception ex) { Console.WriteLine(ex.Message); return; }

                    int bytesReceived = sender.Receive(buffer); //количество полученных от сервера байт записали в буфер
                    Console.WriteLine($"Got message: {Encoding.ASCII.GetString(buffer, 0, bytesReceived)}");
                }
                sender.Shutdown(SocketShutdown.Both); // заблокировать сокет на отправку и получение тк все закончили работу
            }
        }
    }
}
