using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TFTP_Client
{
    public class TFTPClient
    {

        #region enums

        /// <summary>
        /// Коды операций TFTP
        /// </summary>
        public enum Opcodes
        {
            Unknown = 0,
            Read = 1,
            Write = 2,
            Data = 3,
            Ack = 4,
            Error = 5
        }

        /// <summary>
        /// Режимы TFTP
        /// </summary>
        public enum Modes
        {
            Unknown = 0,
            NetAscii = 1,
            Octet = 2,
            Mail = 3
        }

        /// <summary>
        /// Исключения TFTP
        /// </summary>
        public class TFTPException : Exception
        {

            public string ErrorMessage = "";
            public int ErrorCode = -1;

            /// <summary>
            /// Инициализирует новый экземпляр <see cref="TFTPException"/> class.
            /// </summary>
            /// <param name="errCode">Код ошибки.</param>
            /// <param name="errMsg">Текст ошибки</param>
            public TFTPException(int errCode, string errMsg)
            {
                ErrorCode = errCode;
                ErrorMessage = errMsg;
            }

            /// <summary>
            /// Создает и возвращает строковое представление текущего исключения.
            /// </summary>
            /// <returns>
            /// Строковое представление текущего исключения.
            /// </returns>
            /// <filterPriority>1</filterPriority>
            /// <permissionSet class="System.Security.permissionSet" version="1">
            /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
            /// </permissionSet>
            public override string ToString()
            {
                return String.Format("TFTPException: ErrorCode: {0} Message: {1}", ErrorCode, ErrorMessage);
            }
        }

        private int tftpPort;
        private string tftpServer = "";
        #endregion

        #region конструкторы

        /// <summary>
        /// Создает объект <see cref="TFTPClient"/> class.
        /// </summary>
        /// <param name="server">Сервер</param>
        public TFTPClient(string server)
            : this(server, 69)
        {
        }

        /// <summary>
        /// Создает объект <see cref="TFTPClient"/> class.
        /// </summary>
        /// <param name="server">Сервер</param>
        /// <param name="port">Порт</param>
        public TFTPClient(string server, int port)
        {
            Server = server;
            Port = port;

        }

        #endregion

        public int Port
        {
            get { return tftpPort; }
            private set { tftpPort = value; }
        }

        public string Server
        {
            get { return tftpServer; }
            private set { tftpServer = value; }
        }

        #region методы

        /// <summary>
        /// Получает указанный удаленный файл
        /// </summary>
        /// <param name="remoteFile">Удаленный файл</param>
        /// <param name="localFile">Локальный файл</param>
        public void Get(string remoteFile, string localFile)
        {
            Get(remoteFile, localFile, Modes.Octet);
        }

        /// <summary>
        /// Получает указанный удаленный файл
        /// </summary>
        /// <param name="remoteFile">Удаленный файл</param>
        /// <param name="localFile">Локальный файл</param>
        /// <param name="tftpMode">Режим TFTP</param>
        public void Get(string remoteFile, string localFile, Modes tftpMode)
        {
            int len = 0;
            int packetNr = 1;
            byte[] sndBuffer = CreateRequestPacket(Opcodes.Read, remoteFile, tftpMode);
            byte[] rcvBuffer = new byte[516];

            BinaryWriter fileStream = new BinaryWriter(new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.Read));
            IPHostEntry hostEntry = Dns.GetHostEntry(tftpServer);
            IPEndPoint serverEP = new IPEndPoint(hostEntry.AddressList[0], tftpPort);
            EndPoint dataEP = (EndPoint)serverEP;
            Socket tftpSocket = new Socket(serverEP.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Request and Receive first Data Packet From TFTP Server
            tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
            tftpSocket.ReceiveTimeout = 1000;
            len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);

            // keep track of the TID 
            serverEP.Port = ((IPEndPoint)dataEP).Port;

            while (true)
            {
                // handle any kind of error 
                if (((Opcodes)rcvBuffer[1]) == Opcodes.Error)
                {
                    fileStream.Close();
                    tftpSocket.Close();
                    throw new TFTPException(((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3], Encoding.ASCII.GetString(rcvBuffer, 4, rcvBuffer.Length - 5).Trim('\0'));
                }
                // expect the next packet
                if ((((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3]) == packetNr)
                {
                    // Store to local file
                    fileStream.Write(rcvBuffer, 4, len - 4);

                    // Send Ack Packet to TFTP Server
                    sndBuffer = CreateAckPacket(packetNr++);
                    tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
                }
                // Was ist the last packet ?
                if (len < 516)
                {
                    break;
                }
                else
                {
                    // Receive Next Data Packet From TFTP Server
                    len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);
                }
            }

            // Close Socket and release resources
            tftpSocket.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Помещает указанный удаленный файл
        /// </summary>
        /// <param name="remoteFile">Удаленный файл</param>
        /// <param name="localFile">Локальный файл</param>
        public void Put(string remoteFile, string localFile)
        {
            Put(remoteFile, localFile, Modes.Octet);
        }

        /// <summary>
        /// Помещает указанный удаленный файл
        /// </summary>
        /// <param name="remoteFile">Удаленный файл</param>
        /// <param name="localFile">Локальный файл</param>
        /// <param name="tftpMode">Режим TFTP</param>
        /// <remarks>What if the ack does not come !</remarks>
        public void Put(string remoteFile, string localFile, Modes tftpMode)
        {
            int len = 0;
            int packetNr = 0;
            byte[] sndBuffer = CreateRequestPacket(Opcodes.Write, remoteFile, tftpMode);
            byte[] rcvBuffer = new byte[516];

            BinaryReader fileStream = new BinaryReader(new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            IPHostEntry hostEntry = Dns.GetHostEntry(tftpServer);
            IPEndPoint serverEP = new IPEndPoint(hostEntry.AddressList[0], tftpPort);
            EndPoint dataEP = (EndPoint)serverEP;
            Socket tftpSocket = new Socket(serverEP.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Запрос записи на TFTP-сервер
            tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
            tftpSocket.ReceiveTimeout = 1000;
            len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);

            // следить за TID
            serverEP.Port = ((IPEndPoint)dataEP).Port;

            while (true)
            {
                // обрабатывать любые ошибки
                if (((Opcodes)rcvBuffer[1]) == Opcodes.Error)
                {
                    fileStream.Close();
                    tftpSocket.Close();
                    throw new TFTPException(((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3], Encoding.ASCII.GetString(rcvBuffer, 4, rcvBuffer.Length - 5).Trim('\0'));
                }

                // ожидать следующего пакета ack
                if ((((Opcodes)rcvBuffer[1]) == Opcodes.Ack) && (((rcvBuffer[2] << 8) & 0xff00) | rcvBuffer[3]) == packetNr)
                {
                    sndBuffer = CreateDataPacket(++packetNr, fileStream.ReadBytes(512));
                    tftpSocket.SendTo(sndBuffer, sndBuffer.Length, SocketFlags.None, serverEP);
                }

                // ура
                if (sndBuffer.Length < 516)
                {
                    break;
                }
                else
                {
                    len = tftpSocket.ReceiveFrom(rcvBuffer, ref dataEP);
                }
            }

            // Закрыть сокет и освободить ресурсы
            tftpSocket.Close();
            fileStream.Close();
        }

        #endregion

        #region private

        /// <summary>
        /// Создает пакет запроса
        /// </summary>
        /// <param name="opCode">Код операции</param>
        /// <param name="remoteFile">Удаленный файл</param>
        /// <param name="tftpMode">Режим TFTP</param>
        /// <returns>пакет подтверждения</returns>
        private byte[] CreateRequestPacket(Opcodes opCode, string remoteFile, Modes tftpMode)
        {
            // Создаем новый массив байтов для хранения
            // Чтение пакета запроса
            int pos = 0;
            string modeAscii = tftpMode.ToString().ToLowerInvariant();
            byte[] ret = new byte[modeAscii.Length + remoteFile.Length + 4];

            // Установить первый код операции пакета, чтобы указать
            // является ли этот запрос (запросом на чтение или запись)
            ret[pos++] = 0;
            ret[pos++] = (byte)opCode;

            // Преобразование имени файла в массив символов
            pos += Encoding.ASCII.GetBytes(remoteFile, 0, remoteFile.Length, ret, pos);
            ret[pos++] = 0;
            pos += Encoding.ASCII.GetBytes(modeAscii, 0, modeAscii.Length, ret, pos);
            ret[pos] = 0;

            return ret;
        }

        /// <summary>
        /// Создает пакет данных
        /// </summary>
        /// <param name="packetNr">Пакет номер</param>
        /// <param name="data">Данные</param>
        /// <returns>пакет данных</returns>
        private byte[] CreateDataPacket(int blockNr, byte[] data)
        {
            // Создаем массив байтов для хранения пакета подтверждения
            byte[] ret = new byte[4 + data.Length];

            // Установливаем для первого кода операции пакета значение TFTP_ACK.
            ret[0] = 0;
            ret[1] = (byte)Opcodes.Data;
            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            Array.Copy(data, 0, ret, 4, data.Length);
            return ret;
        }

        /// <summary>
        /// Создает пакет подтверждения.
        /// </summary>
        /// <param name="blockNr">Пакет номер</param>
        /// <returns>пакет подтверждения</returns>
        private byte[] CreateAckPacket(int blockNr)
        {
            // Create Byte array to hold ack packet
            byte[] ret = new byte[4];

            // Set first Opcode of packet to TFTP_ACK
            ret[0] = 0;
            ret[1] = (byte)Opcodes.Ack;

            // Insert block number into packet array
            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            return ret;
        }

        #endregion
    }
}
