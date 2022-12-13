using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace NetworkServer
{
    class ServerUDP
    {
        // Stores the client's ID, it's EndPoint and the server's socket
        public int clientID;
        public IPEndPoint UDPendPoint;
        public UdpClient serverSocket;

        ServerUDPSend sendUDPData = new ServerUDPSend();

        // Stores the most recent position of the client
        public Vector2 position = new Vector2(0, 0);

        // Sets the client ID
        public ServerUDP(int _clientID)
        {
            clientID = _clientID;
        }

        // Sets the Client's endpoint and the Server's socket
        public void Connect(IPEndPoint _endPoint, UdpClient _socket)
        {
            UDPendPoint = _endPoint;
            serverSocket = _socket;
        }

        // Handles any data packets that are sent via UDP to the Server
        public void HandleData(byte[] _data)
        {
            ReadPackage readPackage = new ReadPackage(_data);

            // Reads Client ID
            int _clientID = readPackage.Int();

            // Reads Packet Type
            int packetType = readPackage.Int();

            // Checks the Packet Type
            if (packetType == 1) // Movement Packet
            {
                // Reads time stamp
                int timeStamp = readPackage.Int();

                // Reads position coordinate X
                float posX = readPackage.Float();

                // Reads position coordinate Y
                float posY = readPackage.Float();

                // Sets the position to the read in coordinates
                position = new Vector2(posX, posY);

                // Sends the movement data to all other clients
                sendUDPData.SendMovementData(_clientID, timeStamp, posX, posY, serverSocket);
            }
        }

        // Deletes the UDPendPoint associated with this client
        public void Disconnect()
        {
            UDPendPoint = null;
        }
    }
}
