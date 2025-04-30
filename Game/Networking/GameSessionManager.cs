using Core.Networking;
using System.Collections.Concurrent;

namespace Game.Networking
{
    public sealed class GameSessionManager
    {
        private static GameSessionManager? _instance;
        private static GameSessionManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameSessionManager();

                return _instance;
            }
        }

        private readonly ConcurrentDictionary<int, WeakReference<GameSession>> _activeSessions = new();
        private readonly ConcurrentDictionary<int, WeakReference<GameSession>> _targetSessions = new();

        public static void SetActiveSession(int gameAccountId, GameSession session)
        {
            if (Instance._activeSessions.TryGetValue(gameAccountId, out WeakReference<GameSession>? activeSessionRef))
                activeSessionRef.SetTarget(session);
            else
                Instance._activeSessions.TryAdd(gameAccountId, new WeakReference<GameSession>(session));
        }

        public static GameSession? GetActiveSession(int gameAccountId)
        {
            if (!Instance._activeSessions.TryGetValue(gameAccountId, out WeakReference<GameSession>? activeSessionRef))
                return null;

            if (!activeSessionRef.TryGetTarget(out GameSession? activeSession))
                return null;

            return activeSession;
        }

        public static void SetTargetSession(int gameAccountId, GameSession session)
        {
            if (Instance._targetSessions.TryGetValue(gameAccountId, out WeakReference<GameSession>? activeSessionRef))
                activeSessionRef.SetTarget(session);
            else
                Instance._targetSessions.TryAdd(gameAccountId, new WeakReference<GameSession>(session));
        }

        public static GameSession? GetTargetSession(int gameAccountId)
        {
            if (!Instance._targetSessions.TryGetValue(gameAccountId, out WeakReference<GameSession>? activeSessionRef))
                return null;

            if (!activeSessionRef.TryGetTarget(out GameSession? activeSession))
                return null;

            return activeSession;
        }
    }
}
