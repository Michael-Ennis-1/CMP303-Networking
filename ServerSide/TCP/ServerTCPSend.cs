using System;
using System.Net.Sockets;

namespace NetworkServer
{
    class ServerTCPSend
    {
        // TCP Packet 1
        // Sends the proper time to sync the Client and Server, sends client ID to client
        public void SendClientIDWelcome(int _clientID, int milliseconds, NetworkStream _stream, TcpClient _socket)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                WritePackage _writePackage = new WritePackage();

                // Packet ID
                _writePackage.Int(1);

                // Client ID
                _writePackage.Int(_clientID);

                // Time int
                _writePackage.Int(milliseconds);

                if (_socket != null)
                {
                    _stream.BeginWrite(_writePackage.assembleData(), 0, _writePackage.returnLength(), null, null);
                }
            });
        }

        // TCP Packet 3
        // Sends a spawn packet to every client to spawn the specific player, and sends all currently spawned players to the original client being spawned
        public void SpawnPlayerMulti(int _clientID, string _username, NetworkStream _stream, TcpClient _socket, int _milliseconds)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                WritePackage _writePackage = new WritePackage();

                // Spawns new clients to all other clients
                for (int i = 0; i < ConnectTCP.TCPclientDict.Count; i++)
                {
                    if (ConnectTCP.TCPclientDict[i].socket != null)
                    {
                        // Packet ID
                        _writePackage.Int(3);

                        // Client ID
                        _writePackage.Int(_clientID);

                        // Client Username
                        _writePackage.String(_username);

                        // Client Spawn X
                        _writePackage.Float(0);

                        // Client Spawn Y
                        _writePackage.Float(0);

                        // Time Stamp
                        _writePackage.Int(_milliseconds);

                        Console.WriteLine($"Spawning player ID {_clientID} to Player Username: {ConnectTCP.TCPclientDict[i].clientUsername}...");

                        ConnectTCP.TCPclientDict[i].stream.BeginWrite(_writePackage.assembleData(), 0, _writePackage.returnLength(), null, null);

                        _writePackage = null;
                        _writePackage = new WritePackage();
                    }
                }

                // Spawns all existing clients to new client
                for (int i = 0; i < ConnectTCP.TCPclientDict.Count; i++)
                {
                    if (ConnectTCP.TCPclientDict[i].socket != null && ConnectTCP.TCPclientDict[i].clientID != _clientID)
                    {
                        // Packet ID
                        _writePackage.Int(3);

                        // Client ID
                        _writePackage.Int(ConnectTCP.TCPclientDict[i].clientID);

                        // Client Username
                        _writePackage.String(ConnectTCP.TCPclientDict[i].clientUsername);

                        // Client Spawn X
                        _writePackage.Float(ConnectUDP.UDPclientDict[i].position.X);

                        // Client Spawn Y
                        _writePackage.Float(ConnectUDP.UDPclientDict[i].position.Y);

                        // Time Stamp
                        _writePackage.Int(_milliseconds);

                        Console.WriteLine($"Spawning Player ID {ConnectTCP.TCPclientDict[i].clientID} to Player Username: {_username}...");

                        _stream.BeginWrite(_writePackage.assembleData(), 0, _writePackage.returnLength(), null, null);

                        _writePackage = null;
                        _writePackage = new WritePackage();
                    }
                }
            });
        }
    }
}
