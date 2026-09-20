using System;
using System.Reflection;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module01_CableMaking;
using Modules.Module01_CableMaking.Flow;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Presentation;
using UnityEngine;
static class Program {
 static void Set(object target,string name,object value) => target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
 static void Check(bool value,string message) { if(!value) throw new Exception(message); }
 static void Main() {
  var definition=new ModuleDefinition();
  foreach(var id in new[]{"select_cable","strip_left_end","order_left_t568b"}) { var data=new ObjectiveData(); Set(data,"id",id); definition.Objectives.Add(data); }
  var flow=new ModuleFlowController(definition);
  SimulationManager.Instance=new SimulationManager { FlowController=flow };
  var panel=new CableSelectionPanel(); var cable=new GameObject(); cable.SetActive(false); var root=new GameObject();
  Set(panel,"cableToEnable",cable); Set(panel,"panelRoot",root);
  Set(panel,"options",new[]{new CableSelectionPanel.CableOption { displayName="Cat6" }});
  panel.SelectCurrentCable(); Check(!panel.HasSpawned && !cable.activeSelf,"Selection before Begin must not consume the panel");
  flow.Begin(); Check(flow.CurrentObjectiveData.Id=="select_cable","Selection must be the first objective");
  CableEvents.RaiseCablePeeled(CableEnd.Left); Check(flow.CurrentObjectiveData.Id=="select_cable","Peeling must not skip selection");
  int completed=0; flow.ObjectiveCompleted += _ => completed++;
  panel.ShowNext(); Check(completed==0,"Browsing must not complete selection");
  Set(panel,"options",Array.Empty<CableSelectionPanel.CableOption>());
  panel.SelectCurrentCable(); Check(!panel.HasSpawned,"Empty options must not complete selection");
  Set(panel,"options",new[]{new CableSelectionPanel.CableOption { displayName="Cat6" }});
  panel.SelectCurrentCable();
  Check(panel.HasSpawned && cable.activeSelf && !root.activeSelf,"Confirmation must activate cable and hide selector");
  Check(completed==1 && flow.Objectives[0].IsCompleted && flow.CurrentObjectiveData.Id=="strip_left_end","Confirmation must complete selection and advance to peeling");
  panel.SelectCurrentCable(); flow.RegisterCableSelection(); Check(completed==1 && flow.CurrentObjectiveData.Id=="strip_left_end","Repeated selection must not skip peeling");
  CableEvents.RaiseCablePeeled(CableEnd.Left); Check(completed==2 && flow.CurrentObjectiveData.Id=="order_left_t568b","Existing cable progression must continue");
  Console.WriteLine("PASS: selection timing, browsing, empty options, confirmation, duplicate protection, and subsequent progression.");
 }
}
