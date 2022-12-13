using UnityEngine;
using System.Net.Sockets;

public class ClientUDPSend
{
    // UDP Packet 1 : Send Position
    // Sends the position of this client to the Server, to be sent to all other clients
    public void SendPosition(int _clientID, int _timeStamp, Vector3 position, UdpClient _socket)
    {
        WritePackage _writePackage = new WritePackage();

        // Client ID
        _writePackage.Int(_clientID);

        // Packet ID
        _writePackage.Int(1);

        // Timestamp
        _writePackage.Int(_timeStamp);

        // Client's X position
        _writePackage.Float(position.x);

        // Client's Y position
        _writePackage.Float(position.y);

        if (_socket != null)
        {
            _socket.BeginSend(_writePackage.assembleData(), _writePackage.returnLength(), null, null);
        }
    }
}
