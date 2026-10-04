using System;
using System.Collections;
using System.Text.Json;
namespace UnityEngine {
 public class GameObject {}
 public class MonoBehaviour {
  public GameObject gameObject = new();
  public IEnumerator Timer;
  public void StartCoroutine(IEnumerator e) { Timer=e; }
  public static void DontDestroyOnLoad(object o) {}
  public static void Destroy(object o) {}
 }
 public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float f) {} }
 public static class Application { public static string persistentDataPath; public static NetworkReachability internetReachability=NetworkReachability.ReachableViaLocalAreaNetwork; }
 public enum NetworkReachability { NotReachable, ReachableViaLocalAreaNetwork }
 public static class JsonUtility {
  static JsonSerializerOptions options = new(){IncludeFields=true};
  public static string ToJson(object o,bool pretty=false)=>JsonSerializer.Serialize(o,o.GetType(),options);
  public static T FromJson<T>(string s)=>JsonSerializer.Deserialize<T>(s,options);
 }
 public static class Debug { public static void Log(object o){} public static void LogWarning(object o){} public static void LogError(object o,object context=null){} }
 public static class Mathf { public static int Max(int a,int b)=>Math.Max(a,b); public static float Max(float a,float b)=>Math.Max(a,b); }
}
namespace Systems.Auth { public static class SessionContext {
 public static string UserId="account-a"; public static int ActiveSlotId=-1;
 public static string LocalStorageKey=>UserId??"debug-local";
 public static bool CanSyncToFirebase=>UserId!=null;
 public static void SelectSlot(int id)=>ActiveSlotId=id;
} }
namespace GameData.Achievements { public class AchievementDefinition { public string AchievementId; } }
public class FirebaseSaveManager {
 public static FirebaseSaveManager Instance=new();
 public SaveFile Sent; public Action<bool,string> Complete; public int Calls;
 public void UploadSave(SaveFile f, Action<bool,string> callback) {Sent=f;Complete=callback;Calls++;}
}
