using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkServer
{
    class ConnectTCP
    {
        // Variables for storing clients, socket for listening to new packets and total number of clients as well as port number for the server to run
        public static Dictionary<int, ServerTCP> TCPclientDict = new Dictionary<int, ServerTCP>();
        private static TcpListener tcpListener;
        int totalClients;
        int portNum;

        // Boolean allows clients to update the server milliseconds
        bool finishedAddingClients = false;

        // Starts the TCP listener and look for for incoming connections
        public void Start(int _portNum, int _totalClients)
        {
            totalClients = _totalClients;
            portNum = _portNum;

            tcpListener = new TcpListener(IPAddress.Any, portNum);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPCallback), null);

            // Creates a Client and adds it to the dictionary, with the appropriate ID
            for (int i = 0; i < totalClients; i++)
            {
                TCPclientDict.Add(i, new ServerTCP(i));
            }

            finishedAddingClients = true;
        }

        // Callback function runs itself, keeps trying to accept a new client and connect it to a free socket
        public void TCPCallback(IAsyncResult _result)
        {

            TcpClient _socket = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPCallback), null);
            Console.WriteLine($"Attempted connection from: {_socket.Client.RemoteEndPoint}");

            // If it finds a client to connect, runs the server-side connect function, and connects it to the socket of the client side socket.
            for (int i = 0; i <= totalClients; i++)
            {
                if (TCPclientDict[i].socket == null)
                {
                    TCPclientDict[i].Connect(_socket);
                    return;
                }
            }
        }

        // Updates the server timer for all clients, if the dictionary has finished being created
        public void UpdateTimer(int _ms)
        {
            if (finishedAddingClients == true)
            {
                for (int i = 0; i < totalClients; i++)
                {
                    TCPclientDict[i].UpdateTime(_ms);
                }
            }
        }

        // Calls the Disconnect function for a specific Server-Client, for both UDP and TCP 
        public static void Disconnect(int _clientID)
        {
            Console.WriteLine($"ID: {_clientID} has disconnected from the game");

            TCPclientDict[_clientID].Disconnect();
            ConnectUDP.UDPclientDict[_clientID].Disconnect();
        }
    }
}
