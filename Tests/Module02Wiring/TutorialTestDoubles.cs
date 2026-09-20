// Dobles mínimos de Unity/UI, solo para ejecutar las corutinas reales fuera del Editor.
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Objectives;

namespace UnityEngine
{
    public class ScriptableObject { public string name; }
    public class AudioClip { }
    public sealed class TooltipAttribute : Attribute { public TooltipAttribute(string text) { } }
    public static class Resources { public static T Load<T>(string path) where T : class => null; }
    public sealed class SerializeField : Attribute { }
    public sealed class RangeAttribute : Attribute { public RangeAttribute(int min, int max) { } }
    public sealed class MinAttribute : Attribute { public MinAttribute(int min) { } }
    public sealed class TextAreaAttribute : Attribute { public TextAreaAttribute(int min, int max) { } }
    public sealed class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Debug { public static void Log(object _) { } public static void LogError(object _) { } }
}
namespace Presentacion.Tutorial
{
    public sealed class TutorialDirector
    {
        public IEnumerator WaitForReactions() { yield break; }
        public TestVoice VoiceController = new();
        public IObjectiveFlow FlowController;
        public TestDialogue DialogueController = new();
    }
    public sealed class TestVoice
    {
        public void PlayAudio(GameData.NPC.DialogueAudio audio, string legacyVoiceId = null) { }
        public void Stop() { }
    }
    public sealed class TestDialogue
    {
        public readonly List<string> Shown = new();
        public bool Confirm;
        public IEnumerator ShowDialogueUntilConfirmed(string text, string speaker, Func<bool> completed, GameData.NPC.DialogueAudio audio = null, string legacyVoiceId = null)
        {
            Shown.Add(text);
            while (!Confirm && !completed()) yield return null;
            Confirm = false;
        }
    }
}
