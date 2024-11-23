namespace XNetTools.TCP.Backend.Server.CustomEventArgs
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public ClientConnection Client { get; private set; }
        public ClientDisconnectedEventArgs(ClientConnection connector)
        {
            Client = connector;
        }
    }
}
