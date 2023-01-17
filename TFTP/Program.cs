namespace TFTP_Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            // transfer mode octet (слишком долго другие писать)
            TFTPClient tftp = new("127.0.0.1");
            tftp.Put(@"test.zip", @"c:\Temp\FileWrite.zip"); // write 
            tftp.Get(@"test.zip", @"c:\temp\FileRead.zip"); // read
        }
    }
}

/*
Структура пакетов:

Data Packet
2 bytes     2 bytes	        Data bytes
Opcode	    Block number	Data


ACK Packet
2 bytes	    2 bytes
Opode	    Block number


Error Packet
2 bytes	    2 bytes	        String	        1 byte
Opcode	    Error code	    Error message	   0 
 */
