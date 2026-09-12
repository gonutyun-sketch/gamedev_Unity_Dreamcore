using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace NoDestination.Editor {
[InitializeOnLoad] public static class PrototypeWorkshop {
 const string ScenePath="Assets/NoDestination/Scenes/NoDestination.unity";
 static PrototypeWorkshop(){EditorApplication.update+=Tick;}
 static void Tick(){if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
 if(File.Exists("Artifacts/create.request")&&!EditorApplication.isPlaying){File.Delete("Artifacts/create.request");Create();}
 if(File.Exists("Artifacts/play.request")&&!EditorApplication.isPlaying){File.Delete("Artifacts/play.request");for(int i=0;i<SceneManager.sceneCount;i++){var s=SceneManager.GetSceneAt(i);if(s.isDirty)EditorSceneManager.SaveScene(s,"Assets/NoDestination/Scenes/Preserved-"+i+".unity",true);}EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;}
 if(File.Exists("Artifacts/stop.request")&&EditorApplication.isPlaying){File.Delete("Artifacts/stop.request");EditorApplication.isPlaying=false;}
 if(File.Exists("Artifacts/build.request")&&!EditorApplication.isPlaying){File.Delete("Artifacts/build.request");Build();}
 }
 [MenuItem("NO DESTINATION/Create Prototype Scene")] public static void Create(){
 var prior=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);new GameObject("NO DESTINATION | Runtime world").AddComponent<Prototype>();EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);if(prior.IsValid())SceneManager.SetActiveScene(prior);Debug.Log("ND: Scene created without modifying existing scene");}
 [MenuItem("NO DESTINATION/Play Prototype")] public static void Play(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;}
 [MenuItem("NO DESTINATION/Build Windows Prototype")] public static void Build(){Directory.CreateDirectory("Assets/NoDestination/Resources");foreach(var name in new[]{"Lit","Unlit"}){var path="Assets/NoDestination/Resources/Preserve"+name+".mat";if(!File.Exists(path))AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/"+name)),path);}AssetDatabase.SaveAssets();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/NoDestination/NoDestination.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Artifacts/build-result.txt",report.summary.result+" errors="+report.summary.totalErrors);}
}
}


