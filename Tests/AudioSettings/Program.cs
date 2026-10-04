using System;
using System.Reflection;
using Systems.Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
static class Program {
 static void Call(object target,string method) => target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
 static void Static(Type type,string method) => type.GetMethod(method,BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
 static void Set(object target,string field,object value) => target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
 static void Check(bool ok,string message) { if(!ok)throw new Exception(message); }
 static GlobalSettingsManager Create(string rootName,AudioMixer mixer) {
  var manager=new GlobalSettingsManager(); var root=new GameObject { name=rootName };
  manager.transform.SetParent(root.transform); Set(manager,"audioMixer",mixer); Call(manager,"Awake"); return manager;
 }
 static void Main() {
  var config=new GlobalAudioBootstrap(); var prefab=new GameObject(); Set(config,"globalSystemsPrefab",prefab); Resources.Configuration=config;
  int creations=0; var mixer=new AudioMixer(); GlobalSettingsManager manager=null;
  UnityEngine.Object.InstantiateHandler=_=>{ creations++;manager=Create("GlobalSystems(Clone)",mixer);return manager.transform.parent.gameObject; };
  Static(typeof(GlobalSettingsManager),"ResetStatics"); Static(typeof(GlobalAudioBootstrap),"Initialize");
  Check(creations==1 && GlobalSettingsManager.Instance==manager,"Direct scene start must create the audio system");
  Check(UnityEngine.Object.Persistent==manager.transform.parent.gameObject,"Persist the whole cloned prefab including music");
  Check(mixer.Values.Count==0,"Mixer settings must wait until Start");
  Call(manager,"OnEnable");Call(manager,"Start");
  Check(Math.Abs(mixer.Values["MusicVolume"]-20*MathF.Log10(0.7f))<0.001,"Initial mixer volume");
  Static(typeof(GlobalAudioBootstrap),"Initialize");Check(creations==1,"Do not instantiate twice");
  var duplicate=Create("GlobalSystems",new AudioMixer());Call(duplicate,"OnEnable");Call(duplicate,"Start");
  Check(GlobalSettingsManager.Instance==manager && !duplicate.transform.parent.gameObject.activeSelf,"Scene duplicate must be disabled");
  Call(duplicate,"OnDestroy");Check(GlobalSettingsManager.Instance==manager,"Destroying a duplicate preserves the singleton");
  var menuSlider=new Slider();var pauseSlider=new Slider();int notifications=0;manager.SettingsChanged+=_=>notifications++;
  manager.BindMusicSlider(menuSlider);manager.BindMusicSlider(pauseSlider);manager.BindMusicSlider(pauseSlider);
  pauseSlider.onValueChanged.Invoke(0.25f);
  Check(notifications==1 && menuSlider.value==0.25f && manager.Current.musicVolume==0.25f,"Late pause slider and idempotent binding must synchronize");
  Check(Math.Abs(mixer.Values["MusicVolume"]-20*MathF.Log10(0.25f))<0.001,"Live logarithmic mixer adjustment");
  Check(mixer.Values["VoiceVolume"]==6,"Music slider must not change voices");
  pauseSlider.onValueChanged.Invoke(0);Check(mixer.Values["MusicVolume"]==-80 && !manager.Current.musicEnabled,"Zero must mute");
  manager.UnbindMusicSlider(pauseSlider);int count=notifications;pauseSlider.onValueChanged.Invoke(0.9f);Check(count==notifications,"Closed panel listener removed");
  manager.BindMusicSlider(pauseSlider);pauseSlider.onValueChanged.Invoke(0.4f);
  var lobby=new GameObject();var lobbySlider=new Slider();lobby.Sliders.Add(lobbySlider);
  SceneManager.Load(new Scene { Roots=new[]{lobby} });Check(lobbySlider.value==0.4f,"New scene slider restores current volume");
  count=notifications;pauseSlider.onValueChanged.Invoke(0.9f);Check(count==notifications,"Old scene listeners removed");
  Call(manager,"OnDisable");Call(manager,"OnDestroy");
  var restored=Create("GlobalSystems(Clone)",new AudioMixer());Call(restored,"Start");Check(restored.Current.musicVolume==0.4f,"Volume survives a new session");
  restored.SetVoiceVolume(0);
  Check(restored.Current.voiceVolume == 0, "Voice mute is saved");
  var voiceMixer = new AudioMixer();
  Set(restored, "audioMixer", voiceMixer);
  restored.SetVoiceVolume(0);
  Check(voiceMixer.Values["VoiceVolume"] == -80, "Voice boost must preserve mute");
  restored.SetVoiceVolume(1);
  Check(voiceMixer.Values["VoiceVolume"] == 6, "Full voice volume includes gain");
  restored.SetVoiceVolume(0.5f);
  Check(Math.Abs(voiceMixer.Values["VoiceVolume"] - (20*MathF.Log10(0.5f)+6)) < 0.001, "Voice gain follows slider");
  var sfxA = new Slider(); var sfxB = new Slider();
  restored.BindSfxSlider(sfxA); restored.BindSfxSlider(sfxA); restored.BindSfxSlider(sfxB);
  int sfxChanges = 0; restored.SettingsChanged += _ => sfxChanges++;
  sfxA.onValueChanged.Invoke(0.25f);
  Check(sfxChanges == 1 && sfxB.value == 0.25f, "SFX sliders synchronize without duplicate listeners");
  Check(Math.Abs(voiceMixer.Values["SFXVolume"] - 20*MathF.Log10(0.25f)) < 0.001, "SFX adjusts its mixer channel");
  Check(restored.Current.voiceVolume == 0.5f && restored.Current.musicVolume == 0.4f, "SFX preserves music and voice");
  Check(JsonUtility.FromJson<UserSettings>(PlayerPrefs.Saved).sfxVolume == 0.25f, "SFX is persisted");
  sfxA.onValueChanged.Invoke(0);
  Check(voiceMixer.Values["SFXVolume"] == -80, "SFX zero mutes");
  restored.UnbindSfxSlider(sfxA); int before = sfxChanges; sfxA.onValueChanged.Invoke(1);
  Check(sfxChanges == before, "SFX listener removed when panel closes");
  var effect = new AudioSource(); restored.RouteSfx(effect);
  Check(effect.outputAudioMixerGroup?.name == "SFX", "Effects route to SFX");
  Console.WriteLine("PASS: direct startup, duplicate prevention, persistent music root, mixer timing, late slider binding, synchronization, mute, scene changes, and saved volume.");
 }
}
