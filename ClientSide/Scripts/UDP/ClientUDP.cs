using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class ClientUDP : MonoBehaviour
{
    // Stores data buffer size
    static int bufferSize = 128;

    // Stores the Client's socket and the Server's endpoint
    public UdpClient socket;
    IPEndPoint endPoint;
    
    // Stores GameManager reference
    public GameManager gameManager;

    // Connects the endpoint to the Server's end point, given the appropriate IP and Port Number
    public void ConnectUDP(string _ip, int _port, int _clientPort)
    {
        // Set's the endpoint to the Server, based on the IP adress and Port number
        endPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);

        socket = new UdpClient(_clientPort);
        socket.Connect(endPoint);

        socket.Client.ReceiveBufferSize = bufferSize;
        socket.Client.SendBufferSize = bufferSize;

        Debug.Log($"Connected UDP to Server with IP: {_ip} and Port number: {_port}, with local port: {_clientPort}");

        // Sets the socket in the Game Manager
        gameManager.SetSocket(socket);

        // Looks for information from the Server (Explicitly only the server)
        socket.BeginReceive(UDPCallback, null);
    }

    // Callback runs itself, keeps looking for information sent from the Server to Handle.
    public void UDPCallback(IAsyncResult _result)
    {
        byte[] _data = socket.EndReceive(_result, ref endPoint);
        socket.BeginReceive(UDPCallback, null);

        if (_data.Length < 4)
        {
            return;
        }
        ThreadManager.ExecuteOnMainThread(() =>
        {
            HandleData(_data);
        }
        );
    }

    // Handles any data packets that are sent via UDP to the Client
    public void HandleData(byte[] _data)
    {
        ReadPackage readPackage = new ReadPackage(_data);

        // Check Client ID
        int clientID = readPackage.Int();

        // Check Packet Type
        int packetType = readPackage.Int();

        if (packetType == 2)
        {
            // Check Timestamp
            int timeStamp = readPackage.Int();

            // Check posX
            float posX = readPackage.Float();

            // Check posY
            float posY = readPackage.Float();

            // Send data to Game Manager to handle appropriately
            gameManager.HandleMovementData(clientID, timeStamp, new Vector2(posX, posY));
        }
    }
}
