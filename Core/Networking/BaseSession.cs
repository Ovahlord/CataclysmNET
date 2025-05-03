using Core.Packets;
using Core.Packets.Opcodes;

namespace Core.Networking
{
    public abstract class BaseSession(BaseSocket socket)
    {
        protected BaseSocket Socket => socket;

        public abstract void HandlePacket(int opcode, byte[] payload);

        /// <summary>
        /// Enqueues the packet in the socket's packet queue which will send it as soon as possible
        /// </summary>
        public void SendPacket(ServerPacket packet)
        {
            Socket.EnqueuePacket(packet);
        }

        /// <summary>
        /// Immediately closes the socket and requests the Tcp stream to be closed
        /// </summary>
        public void Close()
        {
            Socket.Close();
        }

        /// <summary>
        /// Closes the socket after all remaining server packets have been sent out. Should only be used for sending authentication errors
        /// </summary>
        public void DelayedClose()
        {
            Socket.DelayedClose();
        }

        /// <summary>
        /// Suspends the server-to-client communication which prevents the socket from receiving further server packets. Used only for switching between sessions
        /// </summary>
        public void SuspendComms()
        {
            Socket.SuspendComms();
        }
    }
}
