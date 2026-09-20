using System;
using System.Collections;
using System.Reflection;
using GameData.NPC;
using Modules.Module01_CableMaking.Flow;
using Presentacion.NPC;
using Presentacion.Tutorial;
using Presentation.Tutorial;
using UnityEngine;

static class Program
{
    static int checks;
    static void Check(bool value,string reason){checks++;if(!value)throw new Exception(reason);}
    static void Set(object target,string field,object value)=>target.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,value);
    static void Call(object target,string method)=>target.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,null);
    sealed class Step:TutorialStep
    {
        readonly Action callback;public Step(Action cb)=>callback=cb;
        public override IEnumerator Execute(TutorialDirector d){callback();yield break;}
    }
    sealed class Fixture
    {
        public readonly NPCVoiceController Voice=new();
        public readonly NPCDialogueController Dialogue=new();
        public readonly TutorialDirector Director=new();
        public readonly NPCReactionController Reactions=new();
        public readonly Animator Animator=new();
        public readonly ModuleFlowController Flow=new();
        public readonly AudioSource Source=new();
        public Fixture()
        {
            var go=new GameObject();go.Attach(Voice);go.Attach(Dialogue);go.Attach(Director);go.Attach(Reactions);
            var movement=go.AddComponent<NPCMovementController>();
            Set(Voice,"audioSource",Source);Call(Voice,"Awake");
            Set(Dialogue,"dialoguePanel",new GameObject());Set(Dialogue,"speakerName",new TMPro.TMP_Text());
            Set(Dialogue,"textAnimator",new DialogueTextAnimator());
            Set(Director,"dialogueController",Dialogue);Set(Director,"movementController",movement);Call(Director,"Awake");
            Set(Reactions,"dialogueController",Dialogue);Set(Reactions,"animator",Animator);
            Set(Reactions,"useProceduralReactions",false);
            Reactions.Configure(Flow,Director);Director.SetFlowController(Flow);
        }
        public void Start(bool withNext,Action next=null)
        {
            var sequence=new TutorialSequence();
            sequence.AddStep(new GuidedObjectiveStep("test","Instrucción",new DialogueAudio{clip=new AudioClip{length=20}}));
            if(withNext)sequence.AddStep(new Step(next));
            Director.SetSequence(sequence);Director.StartTutorial();
        }
    }
    static Fixture New(){Clock.Reset();return new Fixture();}
    static void Main()
    {
        var f=New();
        var old=Clock.Start(f.Dialogue,f.Dialogue.ShowDialogueUntilConfirmed("Anterior",audio:new(){clip=new(){length=20}}));
        Clock.Tick(3);Check(f.Source.isPlaying,"La voz anterior debe estar activa");
        var newer=new AudioClip{length=20};
        var current=Clock.Start(f.Dialogue,f.Dialogue.ShowDialogueUntilConfirmed("Nuevo",audio:new(){clip=newer}));
        Clock.Tick(3);
        Check(old.Done,"El diálogo reemplazado no debe quedar esperando confirmación");
        Check(f.Source.clip==newer&&f.Source.isPlaying,"La limpieza del diálogo anterior no puede detener el audio nuevo");
        f.Dialogue.Confirm();Check(!f.Source.isPlaying,"Confirmar corta el audio inmediatamente");Clock.Tick();Check(current.Done,"La confirmación termina el diálogo actual");

        f=New();current=Clock.Start(f.Dialogue,f.Dialogue.ShowDialogueUntilConfirmed("Escribiendo",audio:new(){clip=new(){length=20}}));
        f.Dialogue.Confirm();Check(!f.Source.isPlaying,"Saltar la escritura debe cortar la voz");Clock.Tick(3);
        Check(f.Dialogue.IsDialogueActive,"La primera confirmación mantiene el texto completo visible");
        f.Dialogue.Confirm();Clock.Tick();Check(current.Done&&!f.Dialogue.IsDialogueActive,"La segunda confirmación cierra el panel");

        f=New();current=Clock.Start(f.Dialogue,f.Dialogue.ShowDialogueUntilConfirmed("Ocultar",audio:new(){clip=new(){length=20}}));Clock.Tick(3);
        f.Dialogue.HideImmediate();Clock.Tick();Check(current.Done&&!f.Source.isPlaying,"Ocultar cancela panel, espera y audio");

        f=New();old=Clock.Start(f.Dialogue,f.Dialogue.ShowTransientDialogue("Recordatorio",0.1f,clip:new(){length=2}));Clock.Tick(6);
        Check(!old.Done&&f.Source.isPlaying,"Un mensaje transitorio no corta un clip largo al vencer su tiempo de texto");
        Clock.Tick(20);Check(old.Done&&!f.Source.isPlaying,"El mensaje transitorio finaliza al terminar su audio");
        old=Clock.Start(f.Dialogue,f.Dialogue.ShowTransientDialogue("Recordatorio",20,clip:new(){length=20}));Clock.Tick(3);
        current=Clock.Start(f.Dialogue,f.Dialogue.ShowDialogueUntilConfirmed("Principal",audio:new(){clip=newer}));Clock.Tick(3);
        Check(old.Done&&f.Source.clip==newer&&f.Source.isPlaying,"El principal reemplaza recordatorio sin que su limpieza corte la voz nueva");

        f=New();bool next=false;Set(f.Reactions,"completedClip",new AudioClip{length=3});f.Start(true,()=>next=true);
        Clock.Tick(3);f.Flow.Complete();Check(f.Animator.State=="Feliz","Completar inicia felicidad");
        Check(f.Source.clip.length==3,"La felicidad reemplaza el audio de la instrucción");Clock.Tick(17);
        Check(!next&&f.Director.IsRunning,"Esperar el audio aunque la animación ya terminó");Clock.Tick(20);
        Check(next&&!f.Reactions.IsBusy,"Avanzar únicamente cuando terminan animación y audio");

        f=New();next=false;f.Start(true,()=>next=true);Clock.Tick(3);f.Dialogue.Confirm();Clock.Tick();
        f.Reactions.NotifyActionRejected("Error");Call(f.Reactions,"Update");Clock.Tick(3);
        Check(f.Animator.State=="Triste","El error inicia tristeza");f.Dialogue.Confirm();f.Flow.Complete();Clock.Tick(4);
        Check(!next&&f.Animator.State=="Triste","Saltar el texto no recorta la reacción de tristeza");Clock.Tick(12);
        Check(!next&&f.Animator.State=="Feliz","La felicidad espera a que termine la tristeza");Clock.Tick(25);
        Check(next,"La secuencia continúa después de ambas reacciones");

        f=New();bool finished=false;f.Director.TutorialCompleted+=()=>finished=true;f.Start(false);Clock.Tick(3);f.Flow.Complete();Clock.Tick(3);
        Check(!finished&&f.Director.IsRunning,"El último objetivo también espera su reacción antes de completar el tutorial");Clock.Tick(20);
        Check(finished,"El tutorial finaliza al acabar la reacción final");

        f=New();f.Start(false);Clock.Tick(3);f.Flow.Complete();f.Director.StopTutorial();
        Check(!f.Reactions.IsBusy&&!f.Source.isPlaying&&f.Animator.State=="Idle","Cancelar limpia cola, audio y animación");
        Clock.Tick(30);Check(!f.Director.IsRunning,"Cancelar no reanuda pasos pendientes");

        f=New();f.Animator.IgnoreTrigger=true;next=false;f.Start(true,()=>next=true);Clock.Tick(3);f.Flow.Complete();Clock.Tick(20);
        Check(next&&!f.Reactions.IsBusy,"Un trigger que no entra a su estado no bloquea para siempre");
        f=New();f.Animator.NeverExit=true;Set(f.Reactions,"animationTimeout",1f);next=false;f.Start(true,()=>next=true);Clock.Tick(3);f.Flow.Complete();Clock.Tick(20);
        Check(next&&f.Animator.State=="Idle","Un estado atascado se recupera por tiempo límite");
        f=New();f.Start(false);Clock.Tick(3);f.Dialogue.Confirm();Clock.Tick();
        f.Flow.Objective.Data.ReminderInterval=0.1f;Call(f.Reactions,"Update");Clock.Tick(3);
        f.Reactions.NotifyActionRejected("Error pendiente");
        var gate=Clock.Start(f.Director,f.Reactions.WaitForReactions());
        Check(f.Animator.State=="Triste"&&!gate.Done,"Reemplazar un recordatorio conserva la alerta pendiente");
        Clock.Tick(3);f.Dialogue.Confirm();Clock.Tick(20);Check(gate.Done,"La alerta pendiente termina antes de liberar el siguiente paso");

        f=New();f.Voice.PlayClip(new(){length=20});int oldVersion=f.Voice.Version;
        f.Voice.PlayClip(new(){length=20});f.Voice.Stop(oldVersion);
        Check(f.Source.isPlaying,"Una cancelación con token anterior no detiene la sesión nueva");
        f.Voice.PlayClip(null);Check(!f.Source.isPlaying,"Un diálogo sin clip también detiene el audio anterior");
        f=New();Set(f.Reactions,"audioSource",new AudioSource{Missing=true});
        f.Reactions.Configure(f.Flow,f.Director);f.Reactions.CancelReactions();Call(f.Reactions,"OnDisable");
        Check(!f.Reactions.IsBusy,"Referencias Unity vacías no lanzan al configurar/cancelar/desactivar");
        f=New();var motion=new NPCProceduralIdle();var model=new Transform{localPosition=new Vector3{y=2}};
        f.Reactions.gameObject.Attach(motion);Set(motion,"floatingRoot",model);Set(motion,"animator",f.Animator);Call(motion,"Awake");
        var happy=Clock.Start(motion,motion.PlayHappy());Clock.Tick(4);
        Check(motion.IsReacting&&model.localPosition.y>2&&model.localRotation.Yaw>0,"Felicidad rebota y gira el modelo");
        Clock.Tick(20);Check(happy.Done&&!motion.IsReacting&&model.localPosition.y==2&&model.localRotation.Yaw==0&&f.Animator.enabled,"La pose y Animator se restauran al terminar");
        var sad=Clock.Start(motion,motion.PlaySad());Clock.Tick(4);
        Check(model.localRotation.Pitch>0&&Math.Abs(model.localRotation.Yaw)>0,"Tristeza inclina y niega con el cuerpo");
        motion.CancelReaction();Clock.Tick();Check(sad.Done&&model.localRotation.Pitch==0&&!motion.IsReacting,"Cancelar tristeza restaura la pose");
        Set(f.Reactions,"useProceduralReactions",true);Set(f.Reactions,"proceduralMotion",motion);
        next=false;f.Start(true,()=>next=true);Clock.Tick(3);f.Flow.Complete();Clock.Tick(3);
        Check(motion.IsReacting&&!next,"El tutorial espera la reacción procedural sin clips");
        Clock.Tick(25);Check(next&&!motion.IsReacting,"Continúa al terminar la reacción procedural");
        Console.WriteLine($"PASS: {checks} NPC audio, animation, cancellation and sequence assertions.");
    }
}
