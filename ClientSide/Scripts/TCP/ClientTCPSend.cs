using System.Net.Sockets;

public class ClientTCPSend
{
    // Packet 2 Username Packet
    // Sends the username, that the player has input
    public void SendUsername(int _clientID, string _username, NetworkStream _stream, TcpClient _socket)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            WritePackage _writePackage = new WritePackage();

            // Client ID
            _writePackage.Int(_clientID);

            // Packet ID
            _writePackage.Int(2);

            // Username
            _writePackage.String(_username);

            if (_socket != null)
            {
                _stream.BeginWrite(_writePackage.assembleData(), 0, _writePackage.returnLength(), null, null);
            }
        });
    }


}
