using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_server
{
    public class TCP_server
    {
        static void Main(string[] args)
        {
            //LocalEndPoint: возвращает локальную точку (объект типа EndPoint), по которой запущен сокет и по которой он принимает данные
            //RemoteEndPoint: возвращает адрес удаленного хоста, к которому подключен сокет (объект типа EndPoint)
            //tcp: bind, listen accept, (у полученного объекта от метода accept вызываются методы send и receive). Если надо подкл к серверу, то Connect (для отправки те же send и receive)    
            //после завершения работы закрыть сокет: Close() или с самого начала обернуть в using(Socket socket = new Socket(...){}
            //лучшей практикой будет try{socket.Shutdown(SocketShutdown.Both);}catch(Exception ex){Console.WriteLine(ex.Message);}finally{socket.Close();}


            //переменная 
            int DATA = 0; //
            //server
            var port = 1000;
            var host = Dns.GetHostEntry("localhost");
            var ipAddr = host.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddr, port); 

            using (var listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                listener.Bind(localEndPoint);
                listener.Listen(10); //макс кол-во запросов одновременно
                Console.WriteLine($"Server is running and waiting for a connection on port: {port}");
                mover: //если произойдет ошибка то просто прыгаем в начало и ждем новых клиентов
                var handler = listener.Accept();
                Console.WriteLine($"[connect: {handler.RemoteEndPoint}]: Client connected!");
                while (true)
                {
                    string data = null;
                    byte[] buffer = new byte[1024];
                    int bytesRecieved = 0;
                    try
                    {
                        bytesRecieved = handler.Receive(buffer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[error: {handler.RemoteEndPoint}]: {ex.Message}");
                        goto mover;
                    }
                    data += Encoding.ASCII.GetString(buffer, 0, bytesRecieved);
                    Console.WriteLine($"[message: {handler.RemoteEndPoint}]: {data}");

                    int ind = -1;
                    if (data.IndexOf("<DATA>") > -1) //это только пример с одной переменной, которую мы получили можно наделать кучу таких команд (например <X_0>) Или вообще по-своему команды придумать :)
                    {
                        ind = data.IndexOf("<DATA>") + 6; //слово <DATA> = 6 символов
                        try
                        {
                            DATA = int.Parse(data.Substring(ind));

                            //здесь мы можем поработать с переменной и отправить например проверим больше ли 0?
                            var result = (DATA >= 0) ? "Greater or equal then 0" : "Less then 0";
                            handler.Send(Encoding.ASCII.GetBytes(result)); //нужен русский? использовать UTF8 везде где ASCII
                        }
                        catch (FormatException)
                        {
                            handler.Send(Encoding.ASCII.GetBytes("Wrong type!"));
                        }

                        //либо мы можем это сделать здесь
                    }
                    else if (data.IndexOf("<EOF>") > -1)
                    {
                        handler.Send(Encoding.ASCII.GetBytes("Goodbye"));
                        handler.Shutdown(SocketShutdown.Both);
                        break;
                    }
                    else handler.Send(Encoding.ASCII.GetBytes("\nUse <DATA> to show that you`re sending data info\nUse <EOF> to end session"));               
                }
            }
        }
    }
}
