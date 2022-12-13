namespace NetworkServer
{
    class GameLogic
    {
        public int ms = 0;
        public void Update(int MS_PER_TICK)
        {
            // Updates the server's clock
            ms += MS_PER_TICK;

            // Updates the Thread manager
            ThreadManager.UpdateMain();
        }
    }
}
