using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad] public static class SeaVisualVerification
    {
        static VisualJourney game;static int step=-1;static double began;
        static SeaVisualVerification(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlaying){step=-1;return;}
            if(step<0){if(!File.Exists("Artifacts/VisualRebuild/verify-sea-visual.request"))return;File.Delete("Artifacts/VisualRebuild/verify-sea-visual.request");game=UnityEngine.Object.FindFirstObjectByType<VisualJourney>();game.BeginGame();began=EditorApplication.timeSinceStartup;step=0;}
            if(step==0){if(EditorApplication.timeSinceStartup-began<9)return;game.stage=5;game.train.elapsed=43;game.train.Depart();step++;}
            var t=game.GetComponent<RealityTransit>();
            if(step==1&&t.Age>23){VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(-.7f,1.62f,-3.8f),Quaternion.LookRotation(Vector3.right),"15-different-window-realities");step++;}
            if(step==2&&t.Age>30){VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(0,1.6f,-5),Quaternion.LookRotation(new Vector3(0,-1.5f,2.6f)),"16-sea-before-arrival");step++;}
            if(step==3&&t.Age>47)
            {
                VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(18,.96f,30),Quaternion.identity,"18-sea-horizon");
                string report="";
                // A visible FIELD remnant must be identified from the real camera ray, not hidden by cropping.
                var origin=new Vector3(18,.96f,30);var ray=Quaternion.identity*new Vector3(-.9f,-.085f,1).normalized;
                foreach(var hit in Physics.RaycastAll(origin,ray,3000).OrderBy(h=>h.distance))report+=hit.collider.name+" @ "+hit.point+"\n";
                foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.enabled&&r.gameObject.activeInHierarchy&&r.bounds.size.x>8)report+="WIDE "+r.name+" "+r.bounds+"\n";
                File.WriteAllText("Artifacts/VisualRebuild/sea-render-audit.txt",report);game.ResetPose(new Vector3(18,-.65f,30));game.AimAt(new Vector3(18,1,50));game.scriptedMove=Vector2.up;began=EditorApplication.timeSinceStartup;step++;
            }
            if(step==4&&EditorApplication.timeSinceStartup-began>8){game.scriptedMove=Vector2.zero;var prints=game.GetComponent<SeaFootprints>();var last=prints.Latest;VisualWorkshop.CaptureEvidence(game.eyes,last+new Vector3(.6f,2,-2),Quaternion.LookRotation(new Vector3(-.6f,-2,2)),"19-water-footprints");File.AppendAllText("Artifacts/VisualRebuild/sea-render-audit.txt","Footprints after real walking: "+prints.Count+"\n");step=-1;}
        }
    }
}
