using System.Collections.Concurrent;

namespace Core.Networking
{
    public static class GameSessionManager
    {
        private static readonly ConcurrentDictionary<long, GameSession> _activeSessions = [];
        private static readonly ConcurrentDictionary<long, GameSession> _pendingSessions = [];

        /*
         * The client acts as following: when a new session has been established it is inactive at the moment.
         * We have to send SMSG_SUSPEND_COMMS to the currently active session so its socket will be disconnected.
         * Before it disconnects, we get a final CMSG_SUSPEND_COMMS_ACK packet, which will be our hint to switch over
         * to the new socket
         */

        public static void InitiateSessionJump(long gameAccountId, GameSession newSession)
        {
            bool success = _pendingSessions.TryAdd(gameAccountId, newSession);
            if (!success)
            {
                Console.WriteLine("[{GameSessionManager] Could not prepare a new session because there already was a pending session!");
                return;
            }

            // Sending SMSG_SUSPEND_COMMS to the currently active socket so it can start the disconnecting process
            if (_activeSessions.TryGetValue(gameAccountId, out GameSession? currentSession))
                currentSession.SendSuspendComms();
        }

        public static void FinalizeSessionJump(long gameAccountId)
        {
            if (_activeSessions.TryGetValue(gameAccountId, out GameSession? currentSession))
            {
                // Make sure that the session and socket are closed. Should be triggered by the client but just in case...
                currentSession.Close();

                if (_pendingSessions.TryGetValue(gameAccountId, out GameSession? pendingSession))
                {
                    currentSession = pendingSession;
                    // Sending SMSG_RESUME_COMMS to the new socket to take over the sending and receiving
                    currentSession.SendResumeComms();

                    _pendingSessions.Remove(gameAccountId, out _);
                }
            }
        }
    }
}
