// Dobles mínimos de Unity/UI, solo para ejecutar las corutinas reales fuera del Editor.
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Objectives;

namespace UnityEngine
{
    public class ScriptableObject { }
    public sealed class SerializeField : Attribute { }
    public sealed class TextAreaAttribute : Attribute { public TextAreaAttribute(int min, int max) { } }
    public sealed class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Debug { public static void Log(object _) { } public static void LogError(object _) { } }
}
namespace Presentacion.Tutorial
{
    public sealed class TutorialDirector
    {
        public IObjectiveFlow FlowController;
        public TestDialogue DialogueController = new();
    }
    public sealed class TestDialogue
    {
        public readonly List<string> Shown = new();
        public bool Confirm;
        public IEnumerator ShowDialogueUntilConfirmed(string text, string speaker, Func<bool> completed)
        {
            Shown.Add(text);
            while (!Confirm && !completed()) yield return null;
            Confirm = false;
        }
    }
}
