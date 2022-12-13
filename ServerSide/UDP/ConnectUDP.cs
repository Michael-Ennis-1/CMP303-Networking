using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace NetworkServer
{
    class ConnectUDP
    {
        // Stores all UDP clients, stores server UDP socket
        public static Dictionary<int, ServerUDP> UDPclientDict = new Dictionary<int, ServerUDP>();
        static UdpClient udpListener;

        // Stores buffer size, number of Clients and server Port Number
        static int dataBufferSize = 128;
        int totalClients;
        int serverPortNum;

        // Starts the UDP listener to listen for any UDP information being sent to the Server
        public void Start(int _portNum, int _totalClients)
        {
            totalClients = _totalClients;
            serverPortNum = _portNum;

            udpListener = new UdpClient(serverPortNum);

            udpListener.Client.ReceiveBufferSize = dataBufferSize;
            udpListener.Client.SendBufferSize = dataBufferSize;

            udpListener.BeginReceive(UDPCallback, null);

            // Creates a Client and adds it to the dictionary, with the appropriate ID
            for (int i = 0; i < totalClients; i++)
            {
                UDPclientDict.Add(i, new ServerUDP(i));
            }
        }

        // Callback function runs itself, keeps trying to accept a new client and connect it's endpoint to our client
        public void UDPCallback(IAsyncResult _result)
        {
            // Looks for information from any IP address
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPCallback, null);

            // Creates a temporary read package
            ReadPackage tempRead = new ReadPackage(_data);

            // Reads Client ID
            int _clientID = tempRead.Int();

            if (_data.Length < 4)
            {
                return;
            }

            // If the server doesn't have the client's end point, connect to this one
            if (UDPclientDict[_clientID].UDPendPoint == null)
            {
                UDPclientDict[_clientID].Connect(_clientEndPoint, udpListener);
                return;
            }
            else
            {
                // Handles the data sent on the main thread
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    UDPclientDict[_clientID].HandleData(_data);
                }
                );
            }
        }
    }
}
