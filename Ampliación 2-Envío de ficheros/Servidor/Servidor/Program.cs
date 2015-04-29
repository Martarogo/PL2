using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocabulary;

namespace Servidor
{
    class Server
    {
        private readonly int PORT = 23456;
        private readonly String[] SEPARATOR = { " | " };
        private byte[] receivedBytes = new byte[128], bytesACK;
        private String receivedString;
        private String[] receivedElements;
        private int receivedNumber, receivedSec, sec = 1;
        private UdpClient client = null;

        BinaryMessageCodec encoding = new BinaryMessageCodec();
        IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        
        public void Run()
        {
            Console.WriteLine("Servidor en ejecución...");

            try
            {
                client = new UdpClient(PORT);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ErrorCode + ": " + se.Message);
                return;
            }
            
            // El servidor se ejecuta infinitamente
            for (; ; )
            {
                try
                {
                    WaitMessage();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }

            }
        }

        private void WaitMessage()
        {
            // Recibir información
            receivedBytes = client.Receive(ref remoteIPEndPoint);

            receivedString = encoding.Decode(receivedBytes);

            receivedElements = receivedString.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            
            receivedSec = int.Parse(receivedElements[0]);
            receivedNumber = int.Parse(receivedElements[1]);

            if (sec != receivedSec)
            {
                if (receivedSec == -1)
                {
                    Console.WriteLine("Mensaje recibido: " + receivedString);

                    sec = -1;

                    sendACK();

                    Console.WriteLine("\nFin del envio de datos\n");

                    sec = 1;
                }
                else
                {
                    Console.WriteLine("El numero de secuencia recibido no conicide. Se descarta el paquete");
                    WaitMessage();
                }
            }
            else if (sec == receivedSec)
            {
                Console.WriteLine("Mensaje recibido: " + receivedString);

                sendACK();
            }
        }

        private void sendACK()
        {
            Message ack = new Message(sec);

            bytesACK = encoding.Encode(ack);

            client.Send(bytesACK, bytesACK.Length, remoteIPEndPoint);

            Console.WriteLine("Numero de secuencia enviado: " + sec);

            sec++;
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            Server serv = new Server();
            Thread hilo = new Thread(new ThreadStart(serv.Run));
            hilo.Start();
            hilo.Join();
        }
    }
}


