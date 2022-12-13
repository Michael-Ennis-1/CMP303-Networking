using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Stores all the Player Managers attached to all the Players in the game
    public Dictionary<int, PlayerManager> allPlayers = new Dictionary<int, PlayerManager>();

    // Stores the Prefabs of the Red local player and Blue other player
    public GameObject playerPrefab1;
    public GameObject playerPrefab2;

    // Stores the player ID and if the localplayer has spawned
    int localPlayerID;
    bool localPlayerSpawned = false;

    // Stores the client's UDP socket and functions for structuring the UDP Packet
    ClientUDPSend sendUDPData = new ClientUDPSend();
    UdpClient socket;

    // Stores the tick rate of the client's FixedUpdate loop, the Server time and whether or not the time is synced with the server
    static int TICKS_PER_SEC = 10;
    int MS_PER_TICK = 1000 / TICKS_PER_SEC;
    int serverMilliseconds;
    bool timeSynced = false;

    // Stores the time for how long it took a packet to be sent and received back from the Server
    int timedTime = 0;
    public bool timerOn = false;

    // Stores the text for the amount of missed packets
    public GameObject missedPacketCount;

    // Stores whether the Player's should reset their Lerp time, and how many players lerp time have been reset
    bool resetLerpTime = false;
    int lerpCount = 0;

    // Sets the socket for sending UDP data
    public void SetSocket(UdpClient _socket)
    {
        socket = _socket;
    }

    // Spawns a player to this client, with it's specific ID, username and spawn position
    public void SpawnPlayer(int _clientID, string _username, int thisClientID, Vector2 _spawnPos, int _timeStamp)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            GameObject _tempPlayer;

            // Checks if the ClientID of the player to be spawned is the local player
            if (_clientID == thisClientID)
            {
                // Creates the local player, and sets the local player's ID as well as setting the boolean
                _tempPlayer = Instantiate(playerPrefab1);
                localPlayerSpawned = true;
                localPlayerID = thisClientID;
            }
            else
            {
                // Creates another player that isn't the local one
                _tempPlayer = Instantiate(playerPrefab2);
            }

            // Sets the player's position to the spawn position
            _tempPlayer.transform.position = _spawnPos;

            // Sets the player's latest position to it's spawn position
            _tempPlayer.GetComponent<PlayerManager>().latestPos.position = _spawnPos;
            _tempPlayer.GetComponent<PlayerManager>().latestPos.timeStamp = _timeStamp;

            // Adds the player's latest position to the list, and makes it's next prediction
            _tempPlayer.GetComponent<PlayerManager>().AddPosition(_tempPlayer.GetComponent<PlayerManager>().latestPos);
            _tempPlayer.GetComponent<PlayerManager>().NextMovePrediction();
            
            // Sets the player's ID and Username
            _tempPlayer.GetComponent<PlayerManager>().id = _clientID;
            _tempPlayer.GetComponent<PlayerManager>().username = _username;

            // Adds the Player to the dictionary
            _tempPlayer.GetComponent<PlayerManager>().spawned = true;
            allPlayers.Add(_clientID, _tempPlayer.GetComponent<PlayerManager>());
        });
    }

    // Despawns a specific player from this client
    public void DespawnPlayer(int _clientID)
    {
        allPlayers[_clientID].DestroyMe();

        allPlayers.Remove(_clientID);
    }

    // Updates every Server Tick (100ms)
    private void FixedUpdate()
    {
        // Resets all Player's lerp time
        resetLerpTime = true;

        // Updates the Packets Missed text in the top right corner of the client
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (i != localPlayerID)
            {
                missedPacketCount.GetComponent<Text>().text = allPlayers[i].missedPacketCount.ToString();
            }
        }

        // Updates the Server time
        serverMilliseconds += MS_PER_TICK;

        // Sends position data of the local player to all other clients and checks if it's received a packet of information in the one Server Tick time
        if (localPlayerSpawned && timeSynced)
        {
            // Sends position data
            sendUDPData.SendPosition(localPlayerID, serverMilliseconds, allPlayers[localPlayerID].GetPosition(), socket);

            // Checks all players currently existing
            for (int i = 0; i < allPlayers.Count; i++)
            {
                if (i != localPlayerID)
                {
                    // If position data has been updated on time, adds another position to the player's list and makes a new prediction, otherwise adds a missed packet to
                    // it's missed packet count.
                    if (allPlayers[i].updatedInfo == true)
                    {
                        allPlayers[i].missedPacketCount = 0;
                        allPlayers[i].AddPosition(allPlayers[i].latestPos);
                        allPlayers[i].NextMovePrediction();
                    }
                    else if (allPlayers[i].updatedInfo == false)
                    {
                        allPlayers[i].missedPacketCount++;

                        // Disconnects this specific player if it's missed packet count is over 50
                        if (allPlayers[i].missedPacketCount > 50)
                        {
                            DespawnPlayer(i);
                            return;
                        }
                    }

                    // Resets this specific player's updated info to false, so it can check for any more packets that arrive
                    allPlayers[i].updatedInfo = false;
                }
            }
        }

        // Updates the timer
        if (timerOn == true)
        {
            timedTime += MS_PER_TICK;
        }
    }

    private void Update()
    {
        if (localPlayerSpawned && timeSynced)
        {
            // Updates the local player
            allPlayers[localPlayerID].LocalPlayerInputMove();

            for (int i = 0; i < allPlayers.Count; i++)
            {
                try
                {
                    // Lerps any other players if lerp time doesn't need to be reset, otherwise resets that player's lerp time
                    if (i != localPlayerID && allPlayers[i].spawned == true)
                    {
                        if (resetLerpTime == false)
                        {
                            allPlayers[i].LerpToPredict();
                        }

                        if (resetLerpTime == true)
                        {
                            allPlayers[i].lerpTime = 0;
                            lerpCount++;
                        }
                    }
                }
                catch
                {
                    // Returns in case anything goes wrong, instead of crashing the application
                    return;
                }
            }
        }

        // Checks if all players have had their lerpTime reset
        if (lerpCount == allPlayers.Count)
        {
            resetLerpTime = false;
            lerpCount = 0;
        }
    }

    // Syncs the Server Timer with the Client
    public void SyncTimer(int _serverMilliseconds)
    {
        serverMilliseconds = _serverMilliseconds;

        // Adds half the time taken to send a message to and from server
        serverMilliseconds += timedTime / 2;

        timeSynced = true;
    }

    // Takes in movement data from other clients and applies it to the correct player
    public void HandleMovementData(int _clientID, int _timeStamp, Vector2 _pos)
    {
        // Overrides the latest packet data with the most recent packet
        if (allPlayers.Count >= _clientID + 1)
        {
            allPlayers[_clientID].latestPos.position = _pos;
            allPlayers[_clientID].latestPos.timeStamp = _timeStamp;
            allPlayers[_clientID].updatedInfo = true;
        }
    }


}

