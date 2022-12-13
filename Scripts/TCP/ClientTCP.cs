using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class ClientTCP : MonoBehaviour
{
    // Stores the Client's username and ID
    int assignedID;
    string username;

    // Stores received bytes from the Client, and size of buffer
    static int bufferSize = 128;
    byte[] receivedBytes = new byte[bufferSize];

    // Stores client socket and the TCP stream
    NetworkStream stream;
    public TcpClient socket;

    // Stores reference to the Game Manager and the UDP client
    public GameManager gameManager;
    public ClientUDP clientUDP;

    // IP and Port number of the server
    string ip;
    int port;

    // Stores functions that set up the Packet Structure appropriately
    public static ClientTCPSend sendTCPData = new ClientTCPSend();

    // Connects Client to the Server using the Server's IP and Port
    public void ConnectToServer(string _ip, int _port, string _username)
    {
        Debug.Log($"Connecting To Server...");

        ip = _ip;
        port = _port;
        username = _username;

        socket = new TcpClient();

        socket.ReceiveBufferSize = bufferSize;
        socket.SendBufferSize = bufferSize;

        // Attempts to connect the Client to the Server
        socket.BeginConnect(ip, port, AttemptConnection, null);
    }

    // Attempts to connect the Client to the server
    public void AttemptConnection(IAsyncResult _result)
    {
        socket.EndConnect(_result);

        if (!socket.Connected)
        {
            return;
        }

        Debug.Log($"Connected to Server successfully");

        stream = socket.GetStream();

        // Begins searching for data on the stream
        stream.BeginRead(receivedBytes, 0, bufferSize, TCPRecieve, null);
    }

    // Searches for data on the stream
    public void TCPRecieve(IAsyncResult _result)
    {
        // Takes in the length of the bytes on the stream
        int _byteLength = stream.EndRead(_result);

        // Returns if there is no information being sent
        if (_byteLength <= 0)
        {
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
        stream.BeginRead(receivedBytes, 0, bufferSize, TCPRecieve, null);
    }

    // Handles any data packets that are sent via TCP to the Client
    public void HandleData(byte[] _data)
    {
        ReadPackage readPackage = new ReadPackage(_data);

        // Check Packet Type
        int packetType = readPackage.Int();

        if (packetType == 1) // Assign ID packet
        {
            // Assign ID to client
            assignedID = readPackage.Int();

            Debug.Log($"Assigned ID {assignedID} from Server");

            // Sync server and client time
            int serverMilliseconds = readPackage.Int();
            gameManager.SyncTimer(serverMilliseconds);

            // Connect a UDP link, now that a localEndPoint has been detected
            clientUDP.ConnectUDP(ip, port, ((IPEndPoint)socket.Client.LocalEndPoint).Port);

            Debug.Log($"Sending TCP Packet 2 : Username");

            // Send Username Packet to server
            sendTCPData.SendUsername(assignedID, username, stream, socket);

            // Starts the Timer
            gameManager.timerOn = true;
        }
        if (packetType == 3) // Spawn Player Packet
        {
            // Check Client ID
            int _clientID = readPackage.Int();

            // Check Client Username
            string _clientUsername = readPackage.String();

            // Check Client X position
            float spawnX = readPackage.Float();

            // Check Client Y position
            float spawnY = readPackage.Float();

            // Check Packet Tick
            int milliseconds = readPackage.Int();

            // Spawns a specific player to this client
            gameManager.SpawnPlayer(_clientID, _clientUsername, assignedID, new Vector2(spawnX, spawnY), milliseconds);

            Debug.Log($"Spawned {_clientUsername}, ID: {_clientID}");

            // Stops the Timer
            gameManager.timerOn = false;
        }
    }
}
