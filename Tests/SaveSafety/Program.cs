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
 Application.persistentDataPath=Path.Combine(Directory.GetCurrentDirectory(),"Temp","SaveSafety",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Application.persistentDataPath);
 var m=New();m.SaveGame(0,"Lobby",0);Check(m.HasPendingChanges,"Nueva partida pendiente");
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
 Console.WriteLine($"PASS: {checks} save safety assertions. Test files: {Application.persistentDataPath}");
 }
}
