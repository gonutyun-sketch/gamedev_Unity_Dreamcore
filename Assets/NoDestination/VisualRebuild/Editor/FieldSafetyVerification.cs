using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad]
    public static class FieldSafetyVerification
    {
        static VisualJourney game;static int step=-1;static double at;static string log="";static Color outsideSky;static Vector3 crowPosition;
        static FieldSafetyVerification(){EditorApplication.update+=Tick;}
        static void Check(bool ok,string text){if(!ok)throw new Exception(text);log+="PASS: "+text+"\n";File.WriteAllText("Artifacts/VisualRebuild/safety-progress.txt",log);}
        static void Next(){step++;at=EditorApplication.timeSinceStartup;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(step<0)
            {
                if(!EditorApplication.isPlaying||!File.Exists("Artifacts/VisualRebuild/verify-safety.request"))return;
                game=UnityEngine.Object.FindFirstObjectByType<VisualJourney>();if(!game)return;File.Delete("Artifacts/VisualRebuild/verify-safety.request");game.BeginGame();game.scriptedMove=Vector2.zero;step=0;at=EditorApplication.timeSinceStartup;log="Focused regression checks: FIELD boundary, view state, interactions, threat and shaders.\n";
            }
            if(!EditorApplication.isPlaying){step=-1;return;}
            double age=EditorApplication.timeSinceStartup-at;
            try
            {
                if(age>45)throw new Exception("Safety check timed out at "+step);
                if(step==0)
                {
                    if(age<9)return;Check(!game.SubtitlesBusy&&!game.GetComponent<JourneyInterface>().MissionVisible,"opening leaves the view clear before first story and objective");
                    var ticket=UnityEngine.Object.FindObjectsByType<JourneyDetail>(FindObjectsSortMode.None).First(d=>d.key=="ticket");game.ResetPose(new Vector3(0,0,-7.7f));game.AimAt(ticket.transform.position);Physics.SyncTransforms();Check(game.FindInteraction()==ticket,"small real ticket can be deliberately targeted");game.AimAt(ticket.transform.position+Vector3.right*1.2f);Check(game.FindInteraction()!=ticket,"looking beside the ticket does not activate it");game.ResetPose(new Vector3(0,0,-5));game.AimAt(ticket.transform.position);Check(game.FindInteraction()!=ticket,"interaction rejects objects outside 2.4 metre reach");
                    game.train.elapsed=43;game.stage=2;game.deadline.visitElapsed=game.deadline.allowedSeconds*.8f;game.ResetPose(new Vector3(10,-.65f,12.7f));Next();return;
                }
                if(step==1){if(age<1)return;outsideSky=RenderSettings.skybox.GetColor("_Zenith");game.ResetPose(new Vector3(0,0,7.5f));Next();return;}
                if(step==2){if(age<1)return;var insideSky=RenderSettings.skybox.GetColor("_Zenith");Check(Vector4.Distance(outsideSky,insideSky)<.02f,"boarding preserves the same red environment seen outside");game.deadline.visitElapsed=0;game.ResetPose(new Vector3(90,-.65f,60));Next();return;}
                if(step==3){if(age<.9)return;Check(game.GetComponent<FieldSpatialRules>().BoundaryFade>.3f,"leaving FIELD increases haze rather than hitting a boundary wall");Next();return;}
                if(step==4){if(age<3)return;Check(Vector2.Distance(new Vector2(game.transform.position.x,game.transform.position.z),new Vector2(12,26))<.5f,"boundary returns to the familiar path under the fade");game.deadline.visitElapsed=game.deadline.allowedSeconds*.8f;game.ResetPose(new Vector3(10,-.65f,12.7f));var crow=game.GetComponent<ScarecrowField>().crows[0];crow.position=new Vector3(10,-.75f,25);crowPosition=crow.position;game.AimAt(crow.position+Vector3.up*1.3f);Next();return;}
                if(step==5){if(age<1.2)return;Check(Vector3.Distance(game.GetComponent<ScarecrowField>().crows[0].position,crowPosition)<.01f,"watched scarecrow stays fixed");game.AimAt(new Vector3(10,1,0));Next();return;}
                if(step==6){if(age<1.2)return;Check(game.GetComponent<ScarecrowField>().crows[0].position.z<crowPosition.z-.1f,"unwatched scarecrow approaches without teleporting");game.deadline.visitElapsed=game.deadline.allowedSeconds-.3f;Next();return;}
                if(step==7){if(age<3)return;Check(game.deadline.LastCall&&!game.deadline.missedTrain,"final warning grants playable return time");game.ResetPose(new Vector3(0,0,7.5f));Next();return;}
                if(step==8){if(age<1)return;Check(!game.deadline.LastCall&&!game.deadline.missedTrain,"boarding during last call averts the loss");game.deadline.visitElapsed=game.deadline.allowedSeconds-.2f;game.ResetPose(new Vector3(10,-.65f,12.7f));Next();return;}
                if(step==9){if(age<17)return;Check(game.deadline.missedTrain,"staying outside beyond grace enters the delayed failure sequence");Next();return;}
                if(step==10)
                {
                    if(age<11)return;ScreenCapture.CaptureScreenshot("Artifacts/VisualRebuild/20-last-train-proof.png");
                    foreach(var shader in Resources.FindObjectsOfTypeAll<Shader>().Where(s=>s.name.StartsWith("NoDestination/")))Check(!ShaderUtil.GetShaderMessages(shader).Any(m=>m.severity.ToString()=="Error"),"shader renders without compile errors: "+shader.name);
                    Check(game.soundscape.waterSteps.Length==5&&game.soundscape.seaMusic&&game.soundscape.seaWaves,"SEA sound bank assigned and non-vocal");
                    File.WriteAllText("Artifacts/VisualRebuild/safety-verification.txt","PASS\n"+log);Debug.Log("ND VISUAL: SAFETY CHECKS PASS");step=-1;
                }
            }
            catch(Exception e){File.WriteAllText("Artifacts/VisualRebuild/safety-verification.txt","FAIL\n"+log+e);Debug.LogException(e);step=-1;}
        }
    }
}
