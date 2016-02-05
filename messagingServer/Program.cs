using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Net.Sockets;

namespace messagingServer
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static List<TcpClient> clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            int noOfClients = 2;

            IPAddress IP = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(IP, PORT_NO);
            
            //start listener on specified IP and port number
            listener.Start();

            for (int i = 0; i < noOfClients; i++)
            {
                //Thread listenThread = new Thread(start);
                //listenThread.Start();
                Thread newThread = new Thread(() => listeners(listener, clients));
                newThread.Start();
            }
        }
        static void listeners(TcpListener listener, List<TcpClient> clients)
        {          
            TcpClient client = new TcpClient();
            byte[] receive = new byte[1024];
            byte[] send = new byte[1024];
            NetworkStream nwStream;
            string clientName = " ";
          

            //accept incoming client
            client = listener.AcceptTcpClient();

            //add client connection to list to enable the sending of messages between clients
            clients.Add(client);

            //server-side confirmation message
            Console.WriteLine("Client is connected");
            //client-side confirmation message
            byte[] welcomeConfirm = Encoding.ASCII.GetBytes("You are connected to the Server");
            byte[] buffer = welcomeConfirm;
            int bytesRead = welcomeConfirm.Length;
            nwStream = client.GetStream();
            nwStream.Write(buffer, 0, bytesRead);
           

            while (client.Connected)
            {
                nwStream = client.GetStream();
                buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert data received to string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + dataReceived);

                //---one time username acquisition---
                if (dataReceived.Contains("12czx5"))
                {
                    clientName = dataReceived.Remove(0, 6);
                    //---write back the text to client---
                    Console.WriteLine("Sending back: " + clientName);
                    buffer = Encoding.ASCII.GetBytes(clientName);
                    bytesRead = clientName.Length;
                    nwStream.Write(buffer, 0, bytesRead);
                }
                else
                {
                    //---write back the text to each client---
                    foreach(var clientConn in clients)
                    {
                        nwStream = clientConn.GetStream();
                        //Console.WriteLine(clientConn.ToString());
                        Console.WriteLine("Sending back: " + clientName + ": " + dataReceived);
                        buffer = Encoding.ASCII.GetBytes(clientName + ": " + dataReceived);
                        bytesRead = clientName.Length + dataReceived.Length + 2;
                        nwStream.Write(buffer, 0, bytesRead);
                    }                   
                }
            }
            client.Close();
            listener.Stop();
            Console.ReadLine();
        }
    }
}
