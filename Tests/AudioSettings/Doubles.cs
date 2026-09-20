using System;
using System.Collections.Generic;
namespace UnityEngine {
 public class Object {
  public static GameObject Persistent; public static Func<GameObject,GameObject> InstantiateHandler;
  public static T Instantiate<T>(T source) where T:Object => (T)(Object)InstantiateHandler((GameObject)(Object)source);
  public static void Destroy(Object target) {} public static void DontDestroyOnLoad(GameObject root) => Persistent=root;
 }
 public class ScriptableObject:Object {}
 public class MonoBehaviour:Object { public GameObject gameObject=new(); public Transform transform=>gameObject.transform; }
 public class GameObject:Object {
  public string name; public bool activeSelf=true; public Transform transform;
  public List<UI.Slider> Sliders=new(); public GameObject() { transform=new Transform(this); }
  public void SetActive(bool value)=>activeSelf=value;
  public T[] GetComponentsInChildren<T>(bool inactive) => Sliders.ConvertAll(s=>(T)(object)s).ToArray();
 }
 public class Transform(GameObject owner) { public Transform parent; public GameObject gameObject=owner; public string name=>gameObject.name; public void SetParent(Transform t)=>parent=t; }
 public class SerializeField:Attribute {}
 public class CreateAssetMenuAttribute:Attribute { public string fileName; public string menuName; }
 public enum RuntimeInitializeLoadType { SubsystemRegistration,BeforeSceneLoad }
 public class RuntimeInitializeOnLoadMethodAttribute:Attribute { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) {} }
 public static class Resources { public static Object Configuration; public static T Load<T>(string key) where T:Object => (T)Configuration; }
 public static class Debug { public static void LogError(string message)=>throw new Exception(message); }
 public static class Mathf { public static float Clamp01(float n)=>Math.Clamp(n,0,1); public static float Log10(float n)=>MathF.Log10(n); public static bool Approximately(float a,float b)=>Math.Abs(a-b)<0.00001f; }
 public static class PlayerPrefs { public static string Saved=""; public static string GetString(string key,string fallback)=>Saved; public static void SetString(string key,string value)=>Saved=value; public static void Save() {} }
 public static class JsonUtility {
  static readonly System.Text.Json.JsonSerializerOptions options=new(){IncludeFields=true};
  public static T FromJson<T>(string text)=>System.Text.Json.JsonSerializer.Deserialize<T>(text,options);
  public static string ToJson(object value)=>System.Text.Json.JsonSerializer.Serialize(value,options);
 }
}
namespace UnityEngine.Audio { public class AudioMixer { public Dictionary<string,float> Values=new(); public bool SetFloat(string key,float value) { Values[key]=value; return true; } } }
namespace UnityEngine.UI {
 public class Slider { public string name="SliderMusic"; public float minValue,maxValue,value; public bool wholeNumbers; public ValueEvent onValueChanged=new(); public void SetValueWithoutNotify(float v)=>value=v; }
 public class ValueEvent { private event Action<float> Changed; public void AddListener(Action<float> a)=>Changed+=a; public void RemoveListener(Action<float> a)=>Changed-=a; public void Invoke(float v)=>Changed?.Invoke(v); }
}
namespace UnityEngine.SceneManagement {
 public enum LoadSceneMode { Single,Additive }
 public struct Scene { public UnityEngine.GameObject[] Roots; public UnityEngine.GameObject[] GetRootGameObjects()=>Roots??Array.Empty<UnityEngine.GameObject>(); }
 public static class SceneManager { public static event Action<Scene,LoadSceneMode> sceneLoaded; public static Scene Current; public static Scene GetActiveScene()=>Current; public static void Load(Scene scene) { Current=scene;sceneLoaded?.Invoke(scene,LoadSceneMode.Single); } }
}
