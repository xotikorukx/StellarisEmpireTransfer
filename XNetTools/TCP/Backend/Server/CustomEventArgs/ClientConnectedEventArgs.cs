namespace XNetTools.TCP.Backend.Server.CustomEventArgs
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnection Client { get; private set; }
        public ClientConnectedEventArgs(ClientConnection connector)
        {
            Client = connector;
        }
    }
}
