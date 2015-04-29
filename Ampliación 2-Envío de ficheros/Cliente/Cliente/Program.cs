using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocabulary;

namespace Cliente
{
    class Client
    {
        private readonly String SERVER = "localhost";
        private readonly int SERVERPORT = 23456;
        private readonly int RECV_TIMEOUT = 1000;

        private int sec = 1, receivedSec;
        private String receivedString;
        private readonly int[] nArray = { 5, 4, 3, 2, 1 };
        private byte[] sentBytes, receivedBytes = new byte[128];
        BinaryMessageCodec encoding = new BinaryMessageCodec();
        UdpClient udpClient = null;
        Message sentMessage;
        Random random = new Random();
        int randomNumber;
        IPEndPoint remoteIPEndPoint;

        //Fichero:
        private readonly String fichName = "";
        private byte[] binaryFile, bSent, bReceived;
        private FileStream fs;

        public void Run()
        {
            try
            {
                udpClient = new UdpClient();
                remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                udpClient.Client.Bind(remoteIPEndPoint);
                ProcessFile();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                udpClient.Close();
            }
            Console.ReadKey();
        }

        private void ProcessFile()
        {
            try
            {
                fs = new FileStream(fichName, FileMode.Open, FileAccess.Read);
                binaryFile = Encoding.UTF8.GetBytes(fichName);

                //Enviar nombre:
                datagram
                
                //Dividir el mensaje en fragmentos del tamaño adecuado:
                byte[] fileFraction =new byte [511];
                for(int i=0;true;i++){
                    try
                    {
                        fs.Read(fileFraction, i * 511, 511);
                        Message msg = new Message(i, fileFraction);
                        udpClient.Send(encoding.Encode(msg), 512,SERVER,SERVERPORT);
                    }
                    catch (ArgumentOutOfRangeException e){
                        //Implica que el Offset es mayor que el tamaño del fichero.
                        //Si es así es que ya acabamos de leerlo
                        break;
                    }
                }
               

                ChangeState(new WaitConfirmation(this));

                Console.WriteLine("Transferencia del archivo finalizada");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        private void ProcessMessages(int[] nArray)
        {

            for (int i = 0; i < nArray.Length; i++)
            {
                if (i == nArray.Length - 1)
                {
                    sec = -1;
                    sentMessage = new Message(sec, nArray[i]);
                }
                else
                {
                    sentMessage = new Message(sec, nArray[i]);
                }

                sentBytes = Encode(sentMessage);

                sendMessage();
            }
        }

        private void sendFile()
        {
            try
            {
                fs = new FileStream(fichName, FileMode.Open, FileAccess.Read);
                
                binaryFile = Encoding.UTF8.GetBytes(fichName);

                Message msg = new Message(1,binaryFile);


                Packet newFile = new NewFile((int)PacketBodyType.NewFile, binaryFile.Length, binaryFile);

                bSent = encoding.Encode(newFile);

                client.Send(bSent, bSent.Length, SERVER, SERVERPORT);

                ChangeState(new WaitConfirmation(this));

                while (!finish)
                {
                    _state.HandleMessage();
                }

                Console.WriteLine("Transferencia del archivo finalizada");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
        
       

        private byte[] Encode(Message msg)
        {
            return encoding.Encode(sentMessage);
        }

        private void sendMessage()
        {
            randomNumber = random.Next(11);
            if (randomNumber > 1)
            {
                udpClient.Send(sentBytes, sentBytes.Length, SERVER, SERVERPORT);
                Console.WriteLine("Mensaje enviado: " + sentMessage.packet);
            }

            Console.WriteLine("random: " + randomNumber);

            try
            {
                SetTimer();
                
                WaitACK();
            }
            catch (SocketException e)
            {
                ProcessException(e);
            }
        }

        private void SetTimer()
        {
            udpClient.Client.ReceiveTimeout = RECV_TIMEOUT;
        }

        private void WaitACK()
        {
            receivedBytes = udpClient.Receive(ref remoteIPEndPoint);
            
            receivedString = encoding.Decode(receivedBytes);
            receivedSec = int.Parse(receivedString);

            if (sec != receivedSec)
            {
                sendMessage();
            }
            else
            {
                Console.WriteLine("Numero de secuencia recibido: " + receivedSec + "\n");

                if (sec == -1)
                {
                    Console.WriteLine("Fin del envio de datos");
                }
                sec++;
            }
        }

        private void ProcessException(SocketException e)
        {
            if (e.SocketErrorCode == SocketError.TimedOut)
            {
                Console.WriteLine("Ha expirado el temporizador, se reenvia el mensaje");
                sendMessage();
            }
            else
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            const int N = 1;

            Thread[] threads = new Thread[N];
            for (int i = 0; i < N; i++)
            {
                Client client = new Client();
                threads[i] = new Thread(new ThreadStart(client.Run));
                threads[i].Start();
            }

            for (int i = 0; i < N; i++)
            {
                threads[i].Join();
            }

        }
    }
}
