namespace XNetTools.TCP.Backend.Server.CustomEventArgs
{
    public class ClientMessageEventArgs : EventArgs
    {
        public ClientConnection Client { get; private set; }
        public ushort MessageId { get; private set; }
        public byte Version { get; private set; }
        public BinaryReader Reader { get; private set; }
        public int Length { get; private set; }

        public ClientMessageEventArgs(ClientConnection triggeringClient, ushort messageId, byte version, int length, BinaryReader reader)
        {
            Client = triggeringClient;
            MessageId = messageId;
            Version = version;
            Reader = reader;
            Length = length;
        }
    }
}
