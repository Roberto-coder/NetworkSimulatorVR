using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Systems.Auth;
class Program {
 static int checks;
 static void Check(bool test,string reason){checks++;if(!test)throw new Exception(reason);}
 static SaveManager New(){SaveManager.Instance=null;var m=new SaveManager();typeof(SaveManager).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,null);return m;}
 static void Main(){
 var fresh = new PlayerProgress();
 Check(ModuleAccessPolicy.IsUnlocked(fresh,1,false),"M1 disponible para usuario nuevo");
 Check(!ModuleAccessPolicy.IsUnlocked(fresh,2,false)&&!ModuleAccessPolicy.IsUnlocked(fresh,3,false),"M2 y M3 bloqueados sin completitud");
 fresh.modules.Add(new ModuleProgress{moduleId="M1_",completed=false});
 Check(!ModuleAccessPolicy.IsUnlocked(fresh,2,false),"Iniciar M1 no desbloquea M2");
 fresh.completedModuleIds.Add("M1_");
 Check(ModuleAccessPolicy.IsUnlocked(fresh,2,false)&&!ModuleAccessPolicy.IsUnlocked(fresh,3,false),"Completar M1 solo desbloquea M2");
 fresh.modules.Add(new ModuleProgress{moduleId="module_02_rack_installation",completed=true});
 Check(ModuleAccessPolicy.IsUnlocked(fresh,3,false),"ID real de M2 desbloquea M3");
 Check(ModuleAccessPolicy.IsUnlocked(null,2,true)&&ModuleAccessPolicy.IsUnlocked(null,3,true),"Debug desbloqueado sin save");
 Check(!ModuleAccessPolicy.IsUnlocked(null,2,false),"Sin save no se concede debug implícito");
 Check(!ModuleAccessPolicy.CanEnterScene(null,"Modulo3",false)&&ModuleAccessPolicy.CanEnterScene(null,"Lobby",false),"Guard de escenas distingue módulos y Lobby");
 Check(ModuleAccessPolicy.ModuleNumber("invento123")==0,"No se confunden números de otros IDs con módulos");
 Application.persistentDataPath=Path.Combine(Directory.GetCurrentDirectory(),"Temp","SaveSafety",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Application.persistentDataPath);
 var m=New();
 Check(m.IsSlotEmpty(0),"Slot normalizado vacio");
 m.saveFile.slots[1].data=new PlayerProgress();
 Check(m.IsSlotEmpty(1),"data vacio heredado no representa partida");
 m.SaveGame(0,"Lobby",0);Check(m.HasPendingChanges,"Nueva partida pendiente");
 Check(m.saveFile.activeSlotId==0&&SessionContext.ActiveSlotId==0&&!m.IsSlotEmpty(0),"Crear selecciona y ocupa slot");
 m.SyncLocalToFirebase();Check(m.HasPendingChanges,"No limpia pendientes antes del ACK");
 Check(!FirebaseSaveManager.Instance.Sent.slots[0].needsCloudSync,"Snapshot remoto limpio");
 var disk=JsonUtility.FromJson<SaveFile>(File.ReadAllText(Path.Combine(Application.persistentDataPath,"save_account-a.json")));
 Check(disk.slots[0].needsCloudSync,"Cierre durante subida conserva pendiente en disco");
 m.SyncLocalToFirebase();Check(FirebaseSaveManager.Instance.Calls==1,"Evita subidas simultaneas");
 FirebaseSaveManager.Instance.Complete(false,"error");Check(m.HasPendingChanges&&!m.IsSyncInProgress,"Error mantiene pendientes y libera reintento");
 m.SyncLocalToFirebase();m.CompleteTutorialLocally("lobby","Lobby");FirebaseSaveManager.Instance.Complete(true,"ok");Check(m.HasPendingChanges,"ACK no limpia cambios hechos durante subida");
 m.SyncLocalToFirebase();FirebaseSaveManager.Instance.Complete(true,"ok");Check(!m.HasPendingChanges&&m.State==SaveManager.SyncState.Synced,"ACK limpia snapshot confirmado");
 m.DeleteSlot(0);Check(m.HasPendingChanges&&m.saveFile.slots[0].data==null,"Borrado es pendiente durable");
 var remote=new SaveFile();remote.slots.Add(new SaveSlot{slotID=0,data=new PlayerProgress{level=99}});
 m.RestoreSessionData(JsonUtility.ToJson(remote));Check(m.saveFile.slots[0].data==null,"Remoto no resucita borrado pendiente");
 Application.internetReachability=NetworkReachability.NotReachable;int calls=FirebaseSaveManager.Instance.Calls;m.SyncLocalToFirebase();Check(m.State==SaveManager.SyncState.Offline&&calls==FirebaseSaveManager.Instance.Calls,"Sin red no sube");Application.internetReachability=NetworkReachability.ReachableViaLocalAreaNetwork;
 m.SyncLocalToFirebase();var replyA=FirebaseSaveManager.Instance.Complete;SessionContext.UserId="account-b";SessionContext.ActiveSlotId=-1;m.ClearInMemorySession();m.LoadFromLocal();m.SaveGame(0,"Cuenta B",0);replyA(true,"ok");Check(m.saveFile.slots[0].moduleTitle=="Cuenta B"&&m.HasPendingChanges,"ACK cuenta A no modifica B");
 Check(File.Exists(Path.Combine(Application.persistentDataPath,"save_account-a.json"))&&File.Exists(Path.Combine(Application.persistentDataPath,"save_account-b.json")),"Archivos por cuenta");
 m.SyncLocalToFirebase();m.Timer.MoveNext();m.Timer.MoveNext();Check(m.State==SaveManager.SyncState.WaitingForServer&&m.IsSyncInProgress&&m.HasPendingChanges,"Demora no finge cancelacion ni exito");FirebaseSaveManager.Instance.Complete(true,"ok");Check(m.State==SaveManager.SyncState.Synced,"ACK tardio valido confirma");
 Check(!m.HasSeenTutorial("lobby_first_visit"),"Cuenta nueva sin historial del tutorial");
 m.MarkTutorialStarted("lobby_first_visit");
 Check(m.HasSeenTutorial("lobby_first_visit")&&!m.HasCompletedTutorial("lobby_first_visit"),"Inicio y completitud son estados diferentes");
 m.LoadFromLocal();Check(m.HasSeenTutorial("lobby_first_visit"),"Primera visita persiste después de recargar");
 m.SaveGame(1,"Nueva partida",0);Check(m.HasSeenTutorial("lobby_first_visit"),"Nueva partida no reinicia tutorial de la cuenta");
 m.MarkTutorialStarted("lobby_first_visit");Check(m.saveFile.startedTutorialIds.Count==1,"Marcar inicio es idempotente");
 m.CompleteModuleLocally("M1_","M1",null);Check(m.IsModuleUnlocked(2),"Desbloqueo consulta slot activo guardado");
 m.SelectSlot(0);Check(!m.IsModuleUnlocked(2),"No filtra completitud de otra partida");
 SessionContext.IsDebugSession=true;Check(m.IsModuleUnlocked(3),"Manager respeta sesión debug");SessionContext.IsDebugSession=false;
 m.saveFile.startedTutorialIds=null;m.saveFile.slots[0].data.completedTutorialIds.Add("legacy");Check(m.HasSeenTutorial("legacy"),"Compatibilidad con tutorial completado en save antiguo");
 Console.WriteLine($"PASS: {checks} save safety assertions. Test files: {Application.persistentDataPath}");
 }
}
