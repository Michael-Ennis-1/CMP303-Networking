using System.Net.Sockets;

namespace NetworkServer
{
    class ServerUDPSend
    {
        // UDP Packet 2 Movement Data
        // Sends movement data of a specific client to all other clients that exist
        public void SendMovementData(int _localClientID, int _timeStamp, float _posX, float _posY, UdpClient _socket)
        {
            WritePackage _writePackage = new WritePackage();

            for (int i = 0; i < ConnectUDP.UDPclientDict.Count; i++)
            {
                if (ConnectUDP.UDPclientDict[i].UDPendPoint != null && ConnectUDP.UDPclientDict[i].clientID != _localClientID)
                {
                    // Client ID
                    _writePackage.Int(_localClientID);

                    // Packet ID
                    _writePackage.Int(2);

                    // Timestamp
                    _writePackage.Int(_timeStamp);

                    // Client's X position
                    _writePackage.Float(_posX);

                    // Client's Y position
                    _writePackage.Float(_posY);
  
                    ConnectUDP.UDPclientDict[i].serverSocket.BeginSend(_writePackage.assembleData(), _writePackage.returnLength(), ConnectUDP.UDPclientDict[i].UDPendPoint, null, null);

                    _writePackage = null;
                    _writePackage = new WritePackage();
                }
            }
        }
    }
}
