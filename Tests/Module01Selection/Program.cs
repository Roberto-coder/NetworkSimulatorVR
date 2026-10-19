using System;
using System.Reflection;
using GameData.Modules;
using GameData.Objectives;
using Modules.Module01_CableMaking;
using Modules.Module01_CableMaking.Flow;
using Modules.Module01_CableMaking.Domain.Cable;
using Modules.Module01_CableMaking.Presentation;
using Modules.Module01_CableMaking.Flow.Validation;
using UnityEngine;
static class Program {
 interface ITarget {}
 sealed class Target : ITarget {}
 sealed class Probe : Framework.Interaction.Tools.InteractionTool<ITarget> {
  public bool Detect(out ITarget target) => TryGetTarget(out target);
 }
 static void Set(object target,string name,object value) => target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
 static void Check(bool value,string message) { if(!value) throw new Exception(message); }
 static void Main() {
  var probe=new Probe();
  typeof(Framework.Interaction.Tools.InteractionTool<ITarget>).GetField("toolTip",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(probe,new Transform());
  var target=new Target();
  Physics.Hits=new[]{new Collider { ParentTarget=target }};
  Check(probe.Detect(out var detected) && ReferenceEquals(detected,target),"A connector collider must resolve the crimpable action on its parent");
  Physics.Hits=new[]{new Collider { ChildTarget=target }};
  Check(probe.Detect(out detected) && ReferenceEquals(detected,target),"Existing child target detection must continue working");
  Physics.Hits=new[]{new Collider()};
  Check(!probe.Detect(out detected),"Unrelated colliders must not produce a target");
  var definition=new ModuleDefinition();
  foreach(var id in new[]{"select_cable","strip_left_end","order_left_t568b","crimp_left_end","strip_right_end","order_right_t568b","crimp_right_end","connect_tester","validate_cable"}) { var data=new ObjectiveData(); Set(data,"id",id); definition.Objectives.Add(data); }
  var flow=new ModuleFlowController(definition);
  SimulationManager.Instance=new SimulationManager { FlowController=flow };
  var panel=new CableSelectionPanel(); var cable=new GameObject(); cable.SetActive(false); var root=new GameObject(); var select=new UnityEngine.UI.Button(); Set(panel,"selectButton",select);
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
  Check(panel.HasSpawned && cable.activeSelf && root.activeSelf && !select.interactable,"Confirmation must activate cable, preserve selector and disable selection");
  Check(completed==1 && flow.Objectives[0].IsCompleted && flow.CurrentObjectiveData.Id=="strip_left_end","Confirmation must complete selection and advance to peeling");
  panel.SelectCurrentCable(); flow.RegisterCableSelection(); Check(completed==1 && flow.CurrentObjectiveData.Id=="strip_left_end","Repeated selection must not skip peeling");
  CableEvents.RaiseCablePeeled(CableEnd.Left); Check(completed==2 && flow.CurrentObjectiveData.Id=="order_left_t568b","Existing cable progression must continue");
  CableEvents.RaiseCableOrdered(CableEnd.Left);
  Check(flow.CurrentObjectiveData.Id=="crimp_left_end" && flow.ActionValidator.TryValidate(ModuleActionType.Crimp,CableEnd.Left),"Ordering before the NPC instruction must immediately enable left crimping");
  Check(!flow.ActionValidator.TryValidate(ModuleActionType.Crimp,CableEnd.Right),"Crimping the opposite end must remain blocked");
  CableEvents.RaiseCableCrimped(CableEnd.Left);
  CableEvents.RaiseCablePeeled(CableEnd.Right);
  CableEvents.RaiseCableOrdered(CableEnd.Right);
  Check(flow.CurrentObjectiveData.Id=="crimp_right_end" && flow.ActionValidator.TryValidate(ModuleActionType.Crimp,CableEnd.Right),"Ordering must immediately enable right crimping without waiting for NPC dialogue");
  var docker=new TesterDockerController { AreBothConnected=true }; flow.RegisterTesterDocker(docker);
  CableEvents.RaiseCableCrimped(CableEnd.Right);
  Check(flow.CurrentObjectiveData.Id=="validate_cable" && flow.Objectives[7].IsCompleted,"Second crimp must advance through an already connected tester");
  CableEvents.RaiseCableValidated(); Check(flow.ArePracticalObjectivesCompleted,"Validation must finish practical objectives");
  Console.WriteLine("PASS: collider hierarchy detection, selection, both crimp transitions before NPC instructions, wrong-end rejection, and tester progression.");
 }
}
