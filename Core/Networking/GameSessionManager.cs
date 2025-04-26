using System.Collections.Concurrent;

namespace Core.Networking
{
    public static class GameSessionManager
    {
        private static readonly ConcurrentDictionary<long, GameSession> _activeSessions = [];
        private static readonly ConcurrentDictionary<long, GameSession> _pendingSessions = [];

        public static void SetActiveSession(long gameAccountId, GameSession? activeSession)
        {
            if (activeSession == null)
            {
                _activeSessions.TryRemove(gameAccountId, out _);
            }
            else
            {
                _activeSessions.TryRemove(gameAccountId, out _);
                _activeSessions.TryAdd(gameAccountId, activeSession);
            }
        }

        /*
         * The client acts as following: when a new session has been established it is inactive at the moment.
         * We have to send SMSG_SUSPEND_COMMS to the currently active session so its socket will be disconnected.
         * Before it disconnects, we get a final CMSG_SUSPEND_COMMS_ACK packet, which will be our hint to switch over
         * to the new socket
         */

        public static void InitiateSessionJump(long gameAccountId, GameSession newSession)
        {
            if (!_pendingSessions.TryAdd(gameAccountId, newSession))
            {
                Console.WriteLine("[GameSessionManager] Could not prepare a new session because there already was a pending session!");
                return;
            }

            // Sending SMSG_SUSPEND_COMMS to the currently active socket so it can start the disconnecting process
            if (_activeSessions.TryGetValue(gameAccountId, out GameSession? currentSession))
                currentSession.SendSuspendComms();
        }

        public static void FinalizeSessionJump(long gameAccountId)
        {
            _activeSessions.TryGetValue(gameAccountId, out GameSession? currentSession);
            if (!_pendingSessions.TryGetValue(gameAccountId, out GameSession? pendingSession))
                return;

            if (currentSession == null)
            {
                // The current session does not exist and the client attempts to fall back to a new session
                // @todo
                return;
            }

            _activeSessions.TryRemove(gameAccountId, out _);
            _activeSessions.TryAdd(gameAccountId, pendingSession);
            _pendingSessions.TryRemove(gameAccountId, out _);

            pendingSession.SendResumeComms();
        }
    }
}
