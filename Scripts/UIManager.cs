using UnityEngine;
using UnityEngine.UI;
using System;

class UIManager : MonoBehaviour
{
    // Stores references to both the Client's TCP and UDP information
    public ClientTCP clientTCP;
    public ClientUDP clientUDP;

    // Stores references to the Canvas, and the two Input Fields
    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipPort;

    // Stores boolean information on if the client is connected
    bool IsConnected = false;

    // Makes sure that the application runs whilst not being directly controlled
    private void Awake()
    {
        Application.runInBackground = true;
    }

    // Connects the client to the server
    public void ConnectToServer()
    {
        startMenu.SetActive(false);

        usernameField.interactable = false;
        ipPort.interactable = false;

        // Splits the input field's text so it can be used to connect to the server
        string[] splitAddress = ipPort.text.Split(":");

        // Connects the client to the server
        clientTCP.ConnectToServer(splitAddress[0], Convert.ToInt32(splitAddress[1]), usernameField.text);
        IsConnected = true;
    }

    // Disconnects the client when the application is closed
    public void OnApplicationQuit()
    {
        DisconnectClient();
    }

    // Diconnects both UDP and TCP from the Server
    public void DisconnectClient()
    {
        if (IsConnected)
        {
            IsConnected = false;
            clientTCP.socket.Close();
            clientUDP.socket.Close();

            Debug.Log($"Disconnected from the Server");
        }
    }

    
    
}
