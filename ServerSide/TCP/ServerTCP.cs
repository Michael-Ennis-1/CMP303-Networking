using System;
using System.Net.Sockets;

namespace NetworkServer
{
    class ServerTCP
    {
        // Stores Client Information
        public int clientID;
        public string clientUsername;

        // Stores received bytes from the Client, and size of buffer
        static int bufferSize = 128;
        byte[] receivedBytes = new byte[bufferSize];

        // Stores server-socket and the TCP data stream
        public NetworkStream stream;
        public TcpClient socket;

        // Stores functions that set up the Packet Structure appropriately
        ServerTCPSend sendTCPData = new ServerTCPSend();

        // Stores server time
        int milliseconds;

        public ServerTCP(int _id)
        {
            clientID = _id;
        }

        // Connects the socket to a client and finds the appropriate stream
        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = bufferSize;
            socket.SendBufferSize = bufferSize;

            stream = socket.GetStream();

            // Begins searching for information that is sent over the stream, calls TCPReceive if it finds any
            stream.BeginRead(receivedBytes, 0, bufferSize, TCPReceive, null);
            
            Console.WriteLine($"Client {clientID} has connected");

            // Sends the first packet to the client, given 
            Console.WriteLine($"Sending Packet 1 : Client ID");
            sendTCPData.SendClientIDWelcome(clientID, milliseconds, stream, socket);
        }

        // Takes in any information sent to the Server from the Client
        public void TCPReceive(IAsyncResult _result)
        {
            try
            {
                // Takes in the length of the bytes on the stream
                int _byteLength = stream.EndRead(_result);

                // Disconnects if there is no information being sent
                if (_byteLength <= 0)
                {
                    ConnectTCP.Disconnect(clientID);
                    return;
                }

                // Copies the data to a temporary buffer
                byte[] _data = new byte[_byteLength];
                Array.Copy(receivedBytes, _data, _byteLength);

                // Sends the data to the HandleData function
                HandleData(_data);

                // Resets the receivedBytes buffer to be reused
                receivedBytes = null;
                receivedBytes = new byte[bufferSize];

                // Continues searching for information on the stream
                stream.BeginRead(receivedBytes, 0, bufferSize, TCPReceive, null);
            }
            catch
            {
                // Disconnects if an issue is found in this process
                ConnectTCP.Disconnect(clientID);
            }
        }

        // Handles any data packets that are sent via TCP to the Server
        public void HandleData(byte[] _data)
        {
            ReadPackage readPackage = new ReadPackage(_data);

            // Reads Client ID
            int _clientID = readPackage.Int();

            // Reads Packet Type
            int packetType = readPackage.Int();

            
            if (packetType == 2) // Client Username Packet
            {
                // Reads Client Username
                clientUsername = readPackage.String();

                // Spawns player to all clients
                sendTCPData.SpawnPlayerMulti(clientID, clientUsername, stream, socket, milliseconds);
            }
        }

        // Updates the server time, so it can send packets with a timestamp
        public void UpdateTime(int _ms)
        {
            milliseconds = _ms;
        }

        // Disconnects the TCP client that the server is attached to
        public void Disconnect()
        {
            socket.Close();
            stream = null;
            socket = null;
        }
    }
}
