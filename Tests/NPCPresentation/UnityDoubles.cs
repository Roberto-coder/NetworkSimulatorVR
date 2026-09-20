// Simulación determinista del reloj, Animator y AudioSource. Los controladores bajo prueba son los reales.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Clock
{
    static readonly List<Coroutine> jobs = new();
    public static readonly List<AudioSource> Audio = new();
    public static readonly List<Animator> Animators = new();
    public static Coroutine Start(MonoBehaviour owner, IEnumerator iterator)
    {
        var job = new Coroutine(owner, iterator); jobs.Add(job); Advance(job); return job;
    }
    static void Advance(Coroutine job)
    {
        while (!job.Done && job.Stack.Count > 0)
        {
            var current = job.Stack.Peek();
            if (!current.MoveNext()) { job.Stack.Pop(); (current as IDisposable)?.Dispose(); continue; }
            if (current.Current is IEnumerator nested) { job.Stack.Push(nested); continue; }
            return;
        }
        job.Done = true;
    }
    public static void Stop(Coroutine job)
    {
        if (job == null || job.Done) return;
        job.Done = true;
        while (job.Stack.Count > 0) (job.Stack.Pop() as IDisposable)?.Dispose();
    }
    public static void Stop(MonoBehaviour owner) { foreach (var job in jobs.ToArray()) if(job.Owner==owner) Stop(job); }
    public static void Tick(int frames = 1)
    {
        for (int i=0;i<frames;i++)
        {
            foreach(var a in Audio.ToArray()) a.Tick();
            foreach(var a in Animators.ToArray()) a.Tick();
            foreach(var job in jobs.ToArray()) if(!job.Done) Advance(job);
            jobs.RemoveAll(j=>j.Done);
        }
    }
    public static void Reset() { foreach(var j in jobs.ToArray()) Stop(j); jobs.Clear(); Audio.Clear(); Animators.Clear(); }
}
namespace UnityEngine
{
    public class Object
    {
        public string name;
        public bool Missing;
        public static bool operator ==(Object a,Object b) => (ReferenceEquals(a,null)||a.Missing) ? (ReferenceEquals(b,null)||b.Missing) : ReferenceEquals(a,b);
        public static bool operator !=(Object a,Object b) => !(a==b);
        public override bool Equals(object other)=>ReferenceEquals(this,other);
        public override int GetHashCode()=>base.GetHashCode();
    }
    public class ScriptableObject : Object { }
    public class GameObject : Object
    {
        public bool activeSelf;
        readonly Dictionary<Type,Component> components=new();
        public void SetActive(bool active) => activeSelf=active;
        public T AddComponent<T>() where T:Component,new() { var c=new T(); Attach(c); return c; }
        public void Attach(Component c) { c.gameObject=this; components[c.GetType()]=c; }
        public T GetComponent<T>() where T:class => components.Values.OfType<T>().FirstOrDefault();
        public Transform transform=new();
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform => gameObject?.transform;
        public T GetComponent<T>() where T:class => gameObject?.GetComponent<T>();
        public T GetComponentInChildren<T>(bool inactive=false) where T:class => GetComponent<T>();
    }
    public class MonoBehaviour : Component
    {
        public bool isActiveAndEnabled=true;
        public Coroutine StartCoroutine(IEnumerator value)=>Clock.Start(this,value);
        public void StopCoroutine(Coroutine c)=>Clock.Stop(c);
        public void StopAllCoroutines()=>Clock.Stop(this);
    }
    public class Coroutine
    {
        public readonly MonoBehaviour Owner;
        public readonly Stack<IEnumerator> Stack=new();
        public bool Done;
        public Coroutine(MonoBehaviour owner,IEnumerator iterator){Owner=owner;Stack.Push(iterator);}
    }
    public class Transform : Object { public Vector3 position, localPosition; public Quaternion rotation, localRotation; }
    public struct Vector3
    {
        public float x,y,z;
        public float sqrMagnitude=>x*x+y*y+z*z;
        public Vector3 normalized=>this;
        public static Vector3 up=>new(){y=1};
        public static Vector3 forward=>new(){z=1};
        public static Vector3 operator +(Vector3 a,Vector3 b)=>new(){x=a.x+b.x,y=a.y+b.y,z=a.z+b.z};
        public static Vector3 operator -(Vector3 a,Vector3 b)=>new(){x=a.x-b.x,y=a.y-b.y,z=a.z-b.z};
        public static Vector3 operator *(Vector3 a,float b)=>new(){x=a.x*b,y=a.y*b,z=a.z*b};
    }
    public struct Quaternion
    {
        public float Pitch,Yaw;
        public static Quaternion LookRotation(Vector3 a,Vector3 b)=>new();
        public static Quaternion Euler(float x,float y,float z)=>new(){Pitch=x,Yaw=y};
        public static Quaternion AngleAxis(float angle,Vector3 axis)=>new(){Yaw=angle};
        public static Quaternion operator *(Quaternion a,Quaternion b)=>new(){Pitch=a.Pitch+b.Pitch,Yaw=a.Yaw+b.Yaw};
    }
    public static class Mathf
    {
        public const float PI=(float)Math.PI;
        public static float Max(float a,float b)=>Math.Max(a,b);
        public static float Clamp01(float v)=>Math.Clamp(v,0,1);
        public static float Sin(float v)=>(float)Math.Sin(v);
        public static float Abs(float v)=>Math.Abs(v);
        public static float Repeat(float v,float length)=>v%length;
        public static float SmoothStep(float a,float b,float t)=>a+(b-a)*t*t*(3-2*t);
    }
    public static class Time { public static float deltaTime=0.1f; public static float unscaledDeltaTime=0.1f; }
    public class AudioClip { public float length=1; }
    public class AudioSource : Component
    {
        public bool playOnAwake,loop,isPlaying;
        public float spatialBlend;
        public AudioClip clip;
        float elapsed;
        public AudioSource(){Clock.Audio.Add(this);}
        public void Stop(){if(Missing)throw new Exception("UnassignedReferenceException");isPlaying=false;}
        public void Play(){isPlaying=clip!=null;elapsed=0;}
        public void Tick(){ if(isPlaying && !loop && (elapsed+=Time.deltaTime)>=clip.length) isPlaying=false; }
    }
    public enum AnimatorControllerParameterType { Trigger }
    public class AnimatorControllerParameter { public string name; public AnimatorControllerParameterType type; }
    public struct AnimatorStateInfo { public string Name; public bool IsName(string name)=>Name==name; }
    public class Animator : Component
    {
        public bool enabled=true;
        public bool isActiveAndEnabled=true;
        public object runtimeAnimatorController=new();
        public int layerCount=1;
        public string State="Idle";
        public float Duration=1.5f;
        public bool IgnoreTrigger, NeverExit;
        float elapsed;
        public int Triggers;
        public Animator(){Clock.Animators.Add(this);}
        public AnimatorControllerParameter[] parameters=>new[]{new AnimatorControllerParameter{name="Feliz"},new AnimatorControllerParameter{name="Triste"}};
        public static int StringToHash(string value)=>value.GetHashCode();
        public bool HasState(int layer,int hash)=>new[]{"Idle","Feliz","Triste"}.Any(s=>StringToHash(s)==hash);
        public void SetTrigger(string value){Triggers++; if(!IgnoreTrigger){State=value;elapsed=0;}}
        public void ResetTrigger(string value){}
        public bool IsInTransition(int layer)=>false;
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer)=>new(){Name=State};
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layer)=>new(){Name=""};
        public void Play(int hash,int layer,float time){State="Idle";}
        public void Tick(){if(State!="Idle"&&!NeverExit&&(elapsed+=Time.deltaTime)>=Duration)State="Idle";}
    }
    public sealed class DefaultExecutionOrder:Attribute{public DefaultExecutionOrder(int x){}}
    public sealed class ContextMenu:Attribute{public ContextMenu(string x){}}
    public sealed class RangeAttribute:Attribute{public RangeAttribute(float a,float b){}}
    public sealed class SerializeField:Attribute{}
    public sealed class HideInInspector:Attribute{}
    public sealed class HeaderAttribute:Attribute{public HeaderAttribute(string x){}}
    public sealed class TooltipAttribute:Attribute{public TooltipAttribute(string x){}}
    public sealed class MinAttribute:Attribute{public MinAttribute(float x){}}
    public sealed class TextAreaAttribute:Attribute{public TextAreaAttribute(int x,int y){}}
    public sealed class CreateAssetMenuAttribute:Attribute{public string fileName,menuName;}
    public static class Resources{ public static T Load<T>(string path) where T:class=>null; }
    public static class Debug
    {
        public static readonly List<string> Warnings=new();
        public static void LogError(object value,object context=null){}
        public static void LogWarning(object value,object context=null)=>Warnings.Add(value.ToString());
    }
}
namespace TMPro { public class TMP_Text { public string text; } }
namespace Systems.Input
{
    public class VRInputManager { public static VRInputManager Instance; public event Action ConfirmPressedEvent; }
}
namespace Presentation.Tutorial
{
    public class DialogueTextAnimator
    {
        public bool IsPlaying {get;private set;}
        public void Clear()=>IsPlaying=false;
        public void Skip()=>IsPlaying=false;
        public IEnumerator Play(string text){IsPlaying=true;yield return null;yield return null;IsPlaying=false;}
    }
}
namespace GameData.Objectives
{
    public class ObjectiveData {public string Id="test",Title="Objetivo",Description="Instrucción",ReminderDialogue="Recordatorio";public float ReminderInterval=20;}
}
namespace Core.Objectives
{
    public class ObjectiveBase{public bool IsCompleted; public GameData.Objectives.ObjectiveData Data=new();}
    public interface IObjectiveFlow
    {
        event Action<GameData.Objectives.ObjectiveData> CurrentObjectiveChanged;
        event Action<GameData.Objectives.ObjectiveData> ObjectiveCompleted;
        GameData.Objectives.ObjectiveData CurrentObjectiveData{get;}
        IReadOnlyList<ObjectiveBase> Objectives{get;}
    }
}
namespace Modules.Module01_CableMaking.Flow.Validation
{
    public enum ModuleInteractionErrorType{WrongAction}
    public class ModuleInteractionError{public string Message;public ModuleInteractionErrorType Type;}
    public class ModuleActionValidator{public event Action<ModuleInteractionError> ActionRejected;}
}
namespace Modules.Module01_CableMaking.Flow
{
    public class ModuleFlowController:Core.Objectives.IObjectiveFlow
    {
        public Validation.ModuleActionValidator ActionValidator=new();
        public event Action<GameData.Objectives.ObjectiveData> CurrentObjectiveChanged;
        public event Action<GameData.Objectives.ObjectiveData> ObjectiveCompleted;
        public Core.Objectives.ObjectiveBase Objective=new();
        public GameData.Objectives.ObjectiveData CurrentObjectiveData=>Objective.Data;
        public IReadOnlyList<Core.Objectives.ObjectiveBase> Objectives=>new[]{Objective};
        public void Complete(){Objective.IsCompleted=true;ObjectiveCompleted?.Invoke(Objective.Data);}
    }
}
namespace Presentacion.NPC
{
    public class NPCMovementController:MonoBehaviour { public void Stop(){} }
    public class NPCPlayerLookController:MonoBehaviour{}
}
