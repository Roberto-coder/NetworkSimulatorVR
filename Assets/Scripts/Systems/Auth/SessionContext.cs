using Firebase.Auth;

namespace Systems.Auth
{
    /// <summary>
    /// Estado mínimo de la sesión actual. Firebase conserva la autenticación;
    /// este contexto solo expone identidad, modo de prueba y slot activo.
    /// </summary>
    public static class SessionContext
    {
        public const string DebugUserId = "debug-local";
        private const string CachedUserIdKey = "session.firebase.uid";
        private const string CachedUsernameKey = "session.firebase.username";

        public static string Username { get; private set; }
        public static bool IsDebugSession { get; private set; }
        public static int ActiveSlotId { get; private set; } = -1;

        public static string UserId => IsDebugSession
            ? DebugUserId
            : FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public static bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);
        public static bool CanSyncToFirebase => IsAuthenticated && !IsDebugSession;
        public static string LocalStorageKey => IsAuthenticated ? UserId : DebugUserId;

        public static void BeginFirebaseSession(string username)
        {
            Username = username;
            IsDebugSession = false;
            ActiveSlotId = -1;
            CacheFirebaseProfile();
        }

        public static void BeginDebugSession(string username = "userdebug")
        {
            Username = username;
            IsDebugSession = true;
            ActiveSlotId = 0;
        }

        public static void SelectSlot(int slotId) => ActiveSlotId = slotId;

        public static void Clear()
        {
            Username = null;
            IsDebugSession = false;
            ActiveSlotId = -1;
            UnityEngine.PlayerPrefs.DeleteKey(CachedUserIdKey);
            UnityEngine.PlayerPrefs.DeleteKey(CachedUsernameKey);
            UnityEngine.PlayerPrefs.Save();
        }

        public static bool RestorePersistedFirebaseSession()
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null)
                return false;

            string cachedUid = UnityEngine.PlayerPrefs.GetString(CachedUserIdKey, string.Empty);
            string cachedUsername = cachedUid == user.UserId
                ? UnityEngine.PlayerPrefs.GetString(CachedUsernameKey, string.Empty)
                : string.Empty;

            Username = string.IsNullOrWhiteSpace(cachedUsername)
                ? GetFallbackUsername(user)
                : cachedUsername;
            IsDebugSession = false;
            ActiveSlotId = -1;
            CacheFirebaseProfile();
            return true;
        }

        private static void CacheFirebaseProfile()
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null || string.IsNullOrWhiteSpace(Username))
                return;

            UnityEngine.PlayerPrefs.SetString(CachedUserIdKey, user.UserId);
            UnityEngine.PlayerPrefs.SetString(CachedUsernameKey, Username);
            UnityEngine.PlayerPrefs.Save();
        }

        private static string GetFallbackUsername(FirebaseUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.DisplayName))
                return user.DisplayName;
            if (!string.IsNullOrWhiteSpace(user.Email))
                return user.Email.Split('@')[0];
            return "Usuario";
        }
    }
}
