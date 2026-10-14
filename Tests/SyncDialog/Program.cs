using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Systems.Save;

class Program
{
    static int checks;
    static void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
    static void Set(object o,string name,object value)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    static object Call(object o,string name)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,null);
    static void Main()
    {
        var host=new UnityEngine.Transform();
        var previous=new UnityEngine.RectTransform(); host.children.Add(previous);
        var inactive=new UnityEngine.RectTransform();inactive.gameObject.SetActive(false);host.children.Add(inactive);
        var interaction=new UnityEngine.Transform();host.children.Add(interaction);
        var metaInteraction=new UnityEngine.RectTransform();
        metaInteraction.component=new Oculus.Interaction.PointableCanvas();host.children.Add(metaInteraction);
        var dialog=new SyncStatusDialog();UnityEngine.MonoBehaviour.Found=dialog;dialog.transform.parent=host;host.children.Add(dialog.transform);
        var panel=new UnityEngine.GameObject();var retry=new UnityEngine.UI.Button();var close=new UnityEngine.UI.Button();
        Set(dialog,"panel",panel);Set(dialog,"title",new TMPro.TMP_Text());Set(dialog,"message",new TMPro.TMP_Text());Set(dialog,"retryButton",retry);Set(dialog,"closeButton",close);Set(dialog,"retryLabel",new TMPro.TMP_Text());
        Call(dialog,"Awake");Check(!panel.activeSelf,"Aviso oculto inicialmente");
        bool retried=false;
        SyncStatusDialog.ShowNotice("Sin red","Conecta el equipo",()=>{retried=true;Check(previous.gameObject.activeSelf,"Reintento recupera campos originales antes de ejecutar login");});
        Check(panel.activeSelf&&!previous.gameObject.activeSelf,"Aviso reemplaza contenido anterior");
        Check(interaction.gameObject.activeSelf,"No apaga superficie de interacción VR");
        Check(metaInteraction.gameObject.activeSelf,"RectTransform de PointableCanvas permanece activo");
        previous.gameObject.SetActive(true);Call(dialog,"LateUpdate");Check(!previous.gameObject.activeSelf,"Navegación externa no reactiva contenido durante aviso");
        Check(metaInteraction.gameObject.activeSelf,"LateUpdate tampoco apaga la superficie Meta");
        Call(dialog,"Retry");Check(retried&&!panel.activeSelf,"Reintentar ejecuta acción correcta");Check(!inactive.gameObject.activeSelf,"No activa pantallas previamente ocultas");
        var binding=(IEnumerator)Call(dialog,"BindManager");binding.MoveNext();binding.MoveNext();
        SaveManager.Instance.State=SaveManager.SyncState.Offline;SaveManager.Instance.Notify();Check(panel.activeSelf&&!previous.gameObject.activeSelf,"Sincronización también es modal");
        SaveManager.Instance.IsSyncInProgress=true;SaveManager.Instance.State=SaveManager.SyncState.WaitingForServer;SaveManager.Instance.Notify();Call(dialog,"Retry");Check(!retry.interactable&&SaveManager.Instance.Uploads==0,"No duplica subida pendiente");
        SaveManager.Instance.State=SaveManager.SyncState.Synced;SaveManager.Instance.Notify();Check(!panel.activeSelf&&previous.gameObject.activeSelf,"Confirmación restaura pantalla anterior");
        SyncStatusDialog.ShowNotice("Error","Mensaje",()=>{});Call(dialog,"OnDisable");Check(previous.gameObject.activeSelf&&!panel.activeSelf,"Cambio de canvas restaura estado sin aviso huérfano");
        Console.WriteLine($"PASS: {checks} comprobaciones de aviso modal.");
    }
}
namespace UnityEngine
{
    public class SerializeField:Attribute{}
    public class GameObject{public bool activeSelf=true;public void SetActive(bool v)=>activeSelf=v;}
    public class Transform:IEnumerable<Transform>{public GameObject gameObject=new();public Transform parent;public object component;public T GetComponent<T>() where T:class=>component as T;public List<Transform> children=new();public IEnumerator<Transform> GetEnumerator()=>children.GetEnumerator();IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();}
    public class RectTransform:Transform{}
    public class MonoBehaviour{public Transform transform=new RectTransform();public static object Found;public static T FindFirstObjectByType<T>()=> (T)Found;public void StartCoroutine(IEnumerator e){}public void StopAllCoroutines(){}}
    public static class Debug{public static void LogWarning(object o){}}
}
namespace UnityEngine.UI
{
    public class Button{public bool interactable;public Click onClick=new();}
    public class Click{public void AddListener(Action a){}public void RemoveListener(Action a){}}
}
namespace TMPro{public class TMP_Text{public string text;}}
namespace Oculus.Interaction{public class PointableCanvas{}}
namespace Systems.Auth{public static class SessionContext{public static bool CanSyncToFirebase=true;}}
namespace Systems.Scenes{public static class SceneTransitionManager{public static bool IsLoading;}}
public class SaveManager
{
    public static SaveManager Instance=new();public enum SyncState{Local,Offline,Error,WaitingForServer,Synced}
    public SyncState State;public bool IsSyncInProgress;public string StatusMessage="Estado";public event Action StatusChanged;public int Uploads;
    public void Notify()=>StatusChanged?.Invoke();public void SyncLocalToFirebase()=>Uploads++;
}
