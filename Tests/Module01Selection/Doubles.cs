using System;
using System.Collections.Generic;
namespace UnityEngine {
 public class SerializeField : Attribute {} public class HeaderAttribute(string s) : Attribute {}
 public class TextAreaAttribute(int a,int b) : Attribute {}
 public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
 public class ScriptableObject {} public class MonoBehaviour {}
 public class GameObject { public bool activeSelf = true; public void SetActive(bool v) => activeSelf=v; }
 public class Sprite {} public struct Color { public static Color white => default; }
 public static class Debug { public static void Log(object o, object ctx=null) {} }
}
namespace UnityEngine.UI {
 public class Image { public UnityEngine.Sprite sprite; public bool preserveAspect; public UnityEngine.Color color; }
 public class Button { public bool interactable=true; public ClickEvent onClick=new(); }
 public class ClickEvent { public void AddListener(Action a) {} public void RemoveListener(Action a) {} }
}
namespace TMPro { public class TMP_Text { public bool richText; public string text; } }
namespace Framework.Interaction.Tools { public class ToolData {} }
namespace Framework.Interaction.Tools.Interfaces { public interface Placeholder {} }
namespace GameData.Modules {
 public class ModuleDefinition { public List<GameData.Objectives.ObjectiveData> Objectives = new(); public List<Framework.Interaction.Tools.ToolData> availableTools=new(); }
}
namespace Modules.Module01_CableMaking {
 public class SimulationManager { public static SimulationManager Instance; public Flow.ModuleFlowController FlowController; }
}
public class TesterDockerController { public bool AreBothConnected; public event Action BothEndsConnected; }
