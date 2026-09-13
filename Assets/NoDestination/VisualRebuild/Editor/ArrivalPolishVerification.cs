using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad] public static class ArrivalPolishVerification
    {
        static VisualJourney game;static int shot=-1;static bool initialReviewed;static Vector3 lastStation;static float previousTime,previousTravel;static string report="";
        static ArrivalPolishVerification(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlaying){shot=-1;return;}
            if(shot<0){if(!File.Exists("Artifacts/TargetedPolish/verify.request"))return;File.Delete("Artifacts/TargetedPolish/verify.request");game=UnityEngine.Object.FindFirstObjectByType<VisualJourney>();game.BeginGame();shot=0;report="Arrival / visual regression\n";previousTime=0;initialReviewed=false;}
            var t=game.train;if(!game.GameStarted)return;
            try
            {
                if(t.elapsed>1&&!initialReviewed){initialReviewed=true;Check(t.stationObjects.All(o=>!o||o.activeSelf),"station objects exist throughout approach");Check(t.ApproachDistance>300,"station starts far ahead of train");}
                if(t.elapsed>1&&!t.arrived&&!t.departing&&previousTime>0)
                {
                    float dz=t.stationObjects[0].transform.position.z-lastStation.z;
                    CheckSilent(dz<=.001f,"station never moves away during arrival");CheckSilent(Mathf.Abs(dz+(t.WorldTravel-previousTravel))<.015f,"station and wheat/ground share one distance");
                }
                if(!t.departing){lastStation=t.stationObjects[0].transform.position;previousTime=t.elapsed;previousTravel=t.WorldTravel;}
                float[] times={10,30,40,42.3f};
                if(shot<4&&t.elapsed>times[shot])
                {
                    VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(2.10f,1.65f,5),Quaternion.LookRotation(new Vector3(.5f,0,1)),"22-arrival-"+shot);
                    report+="Captured approach at "+t.elapsed+" s, remaining "+t.ApproachDistance+" m\n";shot++;
                }
                if(shot==4&&t.arrived)
                {
                    Check(t.ApproachDistance==0,"station reaches original final position without activation pop");
                    var soil=game.transform.root.GetComponentsInChildren<Renderer>().Single(r=>r.name=="Field earth");var mesh=soil.GetComponent<MeshFilter>().sharedMesh;float slope=0;
                    for(int i=0;i<mesh.vertexCount;i++)for(int j=i+1;j<mesh.vertexCount;j++)
                    {
                        if(mesh.normals[i].y<.9||mesh.normals[j].y<.9)continue;var a=soil.transform.TransformPoint(mesh.vertices[i]);var b=soil.transform.TransformPoint(mesh.vertices[j]);
                        if(Mathf.Abs(a.x-b.x)<.01f&&Mathf.Abs(a.z-b.z)>1)slope=(mesh.uv[j].y-mesh.uv[i].y)/(b.z-a.z)*soil.sharedMaterial.GetTextureScale("_BaseMap").y;
                    }
                    Check(slope<0,"ground top-face UV reverses world Z; negative texture offset now moves toward train rear");
                    VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(10,1,15),Quaternion.LookRotation(new Vector3(12,-.12f,47)),"23-natural-house-approach");
                    File.WriteAllText("Artifacts/TargetedPolish/arrival-verification.txt","PASS\n"+report);shot=5;
                }
            }
            catch(Exception e){File.WriteAllText("Artifacts/TargetedPolish/arrival-verification.txt","FAIL\n"+report+e);shot=99;}
        }
        static void CheckSilent(bool ok,string text){if(!ok)throw new Exception(text);}
        static void Check(bool ok,string text){CheckSilent(ok,text);report+="PASS: "+text+"\n";}
    }
}

