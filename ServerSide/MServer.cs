using System;
using System.Threading;

namespace NetworkServer
{
    class MServer
    {
        // Stores port number and max amount of clients
        static int Portnum;
        static int maxClients;

        // Stores whether the server is running or not
        static bool isRunning = false;

        // Creates a static version of all of these scripts, that can be edited anywhere BUT only one of them can exist
        public static ConnectTCP connectTCP = new ConnectTCP();
        public static ConnectUDP connectUDP = new ConnectUDP();
        public static GameLogic gameLogic = new GameLogic();

        // Stores the server tickrate information
        public const int TICKS_PER_SEC = 10;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;

        // Starts the Server with the Port number
        static void Main(string[] args)
        {
            Start(26950);
        }

        // Starts the whole server, with the appropriate port number
        public static void Start(int _portNum)
        {
            Portnum = _portNum;
            maxClients = 2;

            Console.WriteLine($"Starting Server...");
            Console.WriteLine($"Server is running on {Portnum}");

            // Starts both of the Connect functions to look for other clients attempting to connect and send information to the server
            connectTCP.Start(Portnum, maxClients);
            connectUDP.Start(Portnum, maxClients);

            // Creates a new thread, which is the main thread, so that the server can keep running. Allows other functions to be run on it
            Thread mainThread = new Thread(new ThreadStart(Update));
            mainThread.Start();

            isRunning = true;
        }

        // Updates the server based on the tickrate
        private static void Update()
        {
            Console.WriteLine($"Main thread started. Running at {TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    // Updates the tickrate of the game, and the thread manager
                    gameLogic.Update(MS_PER_TICK);
                    connectTCP.UpdateTimer(gameLogic.ms);

                    // Keeps adding milliseconds to the next loop timer, so when it runs a tick it will then wait another 100ms before another tick occurs
                    _nextLoop = _nextLoop.AddMilliseconds(MS_PER_TICK);
                    
                    // Puts the thread to sleep until the next tick
                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
