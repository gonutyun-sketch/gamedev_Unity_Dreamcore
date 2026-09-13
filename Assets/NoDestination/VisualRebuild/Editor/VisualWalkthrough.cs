using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad]
    public static class VisualWalkthrough
    {
        static int step=-1;
        static int wakeFrame;static bool graceChecked,seaRun;static int transitShot;
        static double started,lastTime,stepTime;
        static VisualJourney game;
        static CharacterController controller;
        static string log="";static Vector3 crowBefore;
        static readonly Vector3[] route={new Vector3(0f,0,7.5f),new Vector3(0f,0,11.6f),new Vector3(0f,0,16f),new Vector3(0f,0,11.6f),new Vector3(0f,0,7.5f),new Vector3(5f,0,7.5f),new Vector3(6.6f,0,12.7f),new Vector3(10f,0,12.7f),new Vector3(22f,0,59.1f),new Vector3(22f,0,63f),new Vector3(20.3f,0,64.4f),new Vector3(20.3f,0,62.1f),new Vector3(19.3f,0,62.1f),new Vector3(20.1f,0,62.1f),new Vector3(20.1f,0,63.9f),new Vector3(22f,0,61.38f),new Vector3(25.25f,0,61.38f),new Vector3(25.25f,0,68.1f),new Vector3(22f,0,67.4f),new Vector3(25.25f,0,68.1f),new Vector3(25.25f,0,61.38f),new Vector3(22f,0,61.38f),new Vector3(20.3f,0,64.4f),new Vector3(22f,0,61.38f),new Vector3(25.25f,0,61.38f),new Vector3(25.25f,0,68.1f),new Vector3(23.4f,0,67.4f),new Vector3(25.25f,0,68.1f),new Vector3(25.25f,0,61.38f),new Vector3(22f,0,61.38f),new Vector3(22f,0,59.1f),new Vector3(10f,0,12.7f),new Vector3(6.6f,0,12.7f),new Vector3(5f,0,7.5f),new Vector3(0f,0,7.5f)};
        static VisualWalkthrough(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(step<0)
            {
                if(!EditorApplication.isPlaying||!File.Exists("Artifacts/VisualRebuild/verify.request"))return;
                game=UnityEngine.Object.FindFirstObjectByType<VisualJourney>();if(!game)return;
                File.Delete("Artifacts/VisualRebuild/verify.request");game.BeginGame();graceChecked=false;seaRun=File.Exists("Artifacts/VisualRebuild/verify-sea.request");if(seaRun)File.Delete("Artifacts/VisualRebuild/verify-sea.request");transitShot=0;controller=game.GetComponent<CharacterController>();step=0;wakeFrame=0;started=lastTime=stepTime=EditorApplication.timeSinceStartup;log="Runtime walkthrough using CharacterController and scene raycasts.\n";
            }
            if(!EditorApplication.isPlaying){step=-1;return;}
            double now=EditorApplication.timeSinceStartup;float dt=Mathf.Clamp((float)(now-lastTime),.001f,.035f);lastTime=now;
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            try
            {
                if(now-stepTime>75&&step>=3||now-started>420)throw new Exception("Walkthrough timed out at step "+step+" position "+game.transform.position);
                if(step==0)
                {
                    if(wakeFrame<3&&now-stepTime>2+wakeFrame*2){ScreenCapture.CaptureScreenshot("Artifacts/VisualRebuild/prologue-"+wakeFrame+".png");wakeFrame++;}
                    if(now-stepTime<9)return;
                    Check(!game.IsWaking,"wake-up animation hands control back after eight seconds");Check(!game.SubtitlesBusy,"opening gives time to see the carriage before subtitles");
                    Check(game.stage==0,"initial ticket objective");Check(!game.MapRevealed&&!game.soundscape.FieldMusicStarted&&!game.soundscape.MusicPlaying,"music remains silent aboard initial train");Check(game.train.speed>5,"train moving at start");Check(game.soundscape&&!game.soundscape.arrival&&!game.soundscape.radioMemory&&!game.soundscape.departure,"human voice clips removed from runtime");Check(game.soundscape.memoryMusic&&game.soundscape.subtitleSound,"original background music and subtitle feedback assigned");Check(game.soundscape.carpetSteps.Length==5&&game.soundscape.concreteSteps.Length==5&&game.soundscape.earthSteps.Length==5&&game.soundscape.woodSteps.Length==5,"twenty recorded footfall variants");CheckWheat();game.scriptedMove=Vector2.zero;game.scriptedZoom=true;Next();return;
                }
                if(step==1){if(now-stepTime<2)return;Check(game.eyes.fieldOfView<42,"smooth zoom reaches close view");game.scriptedZoom=false;Interact("ticket");Check(game.stage==1,"ticket reveals destination");Check(!game.train.arrived,"door waits for arrival");Next();return;}
                if(step==2){if(!game.train.arrived||game.slidingDoor.localPosition.z<1.3f)return;Check(game.eyes.fieldOfView>60,"zoom restores view");Check(!game.train.announcement,"arrival uses subtitles only");Next();return;}
                int waypoint=step-3;
                if(waypoint>=0&&waypoint<route.Length)
                {
                    var target=route[waypoint];if(game.GetComponent<FieldSpatialRules>()?.Extended==true&&target.z>59)target.z+=45;
                    var delta=target-game.transform.position;delta.y=0;
                    if(delta.magnitude>.19f){var local=game.transform.InverseTransformDirection(delta.normalized);game.scriptedMove=new Vector2(local.x,local.z);game.scriptedHurry=true;return;}game.scriptedMove=Vector2.zero;
                    Check(true,"walked to "+target);
                    if(waypoint==0)Interact("coach-door");
                    if(waypoint==2)Check(game.GetComponent<CarriageConnection>().Opened&&game.transform.position.y>-.10f,"walked through both doors and continuous gangway into furnished dining car");
                    if(waypoint==5){Check(game.MapRevealed&&game.soundscape.FieldMusicStarted,"disembarking triggers map title and arrival cue");ScreenCapture.CaptureScreenshot("Artifacts/VisualRebuild/map-title-proof.png");}
                    if(waypoint==7){Check(game.stage==2,"house objective activates outdoors");Check(game.soundscape.footfalls>15,"footsteps triggered by actual travelled distance");}
                    if(waypoint==10){Check(game.soundscape.MusicPlaying,"ambient score fades in after field arrival cue");Interact("radio");Check(game.stage==2&&!game.housePuzzle.Powered,"unpowered radio does not skip the puzzle");}
                    if(waypoint==12){Interact("house-clock");Check(game.housePuzzle.ClockRead,"clock clue reachable downstairs");}
                    if(waypoint==14){Interact("house-fuse");Check(game.housePuzzle.FuseFound,"physical fuse can be retrieved from drawer");}
                    if(waypoint==17){Check(game.transform.position.y>2.65f,"walked up all eighteen physical stairs with headroom");}
                    if(waypoint==18){Interact("house-power");Check(game.housePuzzle.FuseInstalled,"fuse fits the empty power socket");Interact("house-power");Check(!game.housePuzzle.Powered,"broken circuit cannot power the radio");for(int d=0;d<3;d++)for(int n=0;n<new[]{2,0,1}[d];n++)Interact("house-dial-"+d);Interact("house-power");Check(game.housePuzzle.Powered&&game.soundscape.RadioPowered,"aligned circuit restores lights and positional radio static");}
                    if(waypoint==20){Check(game.transform.position.y<0,"walked downstairs without falling through geometry");}
                    if(waypoint==22){Interact("radio");Check(game.stage==3&&game.soundscape.RadioPlaying,"powered radio tunes and advances to upstairs photograph");}
                    if(waypoint==26){Interact("photo");Check(game.stage==4,"photograph advances to return");}
                    if(waypoint==34){Check(game.stage==5,"returned with memory");Check(!game.changedSeat,"opening aisle remains clear after retrieving memory");Check(game.train.departing,"train departs after return");}
                    Next();return;
                }
                if(seaRun)
                {
                    var transit=game.GetComponent<RealityTransit>();
                    if(step==3+route.Length)
                    {
                        if(transit.Age>7&&transitShot==0){Check(transit.Blend==0&&game.train.stationObjects.Any(o=>o&&o.activeSelf)&&game.train.DepartureDistance>1,"FIELD remains present and recedes naturally after departure");transitShot++;}
                        if(transit.Age>23&&transitShot==1){VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(-.7f,1.62f,-3.8f),Quaternion.LookRotation(new Vector3(3,0,0)),"15-different-window-realities");Check(Shader.GetGlobalFloat("_WindowFracture")>.8f,"neighbouring panes reveal different worlds");transitShot++;}
                        if(transit.Age>30&&transitShot==2){VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(0,1.6f,-5),Quaternion.LookRotation(new Vector3(0,-1.5f,2.6f)),"16-sea-before-arrival");Check(Shader.GetGlobalFloat("_PuddleReveal")>.5f,"SEA reflection appears on the carriage floor before arrival");transitShot++;}
                        if(transit.Age>36&&transitShot==3){VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(4,1.4f,-8),Quaternion.LookRotation(new Vector3(14,-.5f,4)),"17-field-becoming-sea");Check(transit.Blend>.8f,"wheat and horizon progressively reconstruct into water");transitShot++;}
                        if(transit.Age<46.5f||game.slidingDoor.localPosition.z<1.3f)return;
                        Check(transit.SeaArrived&&game.train.AtSea&&game.train.speed==0&&game.stage==6,"SEA arrival stops train and opens exit after whiteout");
                        Check(transit.fieldOnly.All(o=>!o.activeSelf)&&transit.seaFloor.enabled,"FIELD geometry retired only after reconstruction; water becomes walkable");Next();return;
                    }
                    var seaRoute=new[]{new Vector3(5,0,7.5f),new Vector3(6.6f,0,12.7f),new Vector3(10,0,12.7f),new Vector3(16,0,20),new Vector3(18,0,30)};
                    int seaWaypoint=step-(4+route.Length);
                    if(seaWaypoint>=0&&seaWaypoint<seaRoute.Length)
                    {
                        var delta=seaRoute[seaWaypoint]-game.transform.position;delta.y=0;
                        if(delta.magnitude>.19f){var local=game.transform.InverseTransformDirection(delta.normalized);game.scriptedMove=new Vector2(local.x,local.z);return;}
                        game.scriptedMove=Vector2.zero;Check(true,"walked across SEA to "+seaRoute[seaWaypoint]);Next();return;
                    }
                    var footprints=game.GetComponent<SeaFootprints>();
                    if(step==9+route.Length)
                    {
                        Check(footprints.Count>8&&game.soundscape.lastSurface==JourneySurface.Water,"walking on water leaves persistent footprints with wet footfalls");
                        var last=footprints.Latest;game.ResetPose(new Vector3(last.x,-.65f,last.z-.9f));game.AimAt(last);Next();return;
                    }
                    if(step==10+route.Length)
                    {
                        if(now-stepTime<1)return;Check(footprints.Noticed&&game.stage==8,"looking at actual footprints triggers the short discovery response");
                        VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(18,.96f,30),Quaternion.identity,"18-sea-horizon");var last=footprints.Latest;VisualWorkshop.CaptureEvidence(game.eyes,last+new Vector3(.3f,1.65f,-1.4f),Quaternion.LookRotation(new Vector3(-.3f,-1.65f,1.4f)),"19-water-footprints");Next();return;
                    }
                    game.scriptedMove=Vector2.zero;game.scriptedZoom=null;game.scriptedHurry=false;File.WriteAllText("Artifacts/VisualRebuild/play-verification.txt","PASS\n"+log);Debug.Log("ND VISUAL: FIELD TO SEA WALKTHROUGH PASS");step=-1;return;
                }
                if(step==3+route.Length){Check(game.GetComponent<FieldSpatialRules>().Extended,"house and return path changed out of sight after power restoration");game.stage=4;game.train.arrived=true;game.train.departing=false;foreach(var o in game.train.stationObjects)if(o)o.SetActive(true);game.deadline.visitElapsed=game.deadline.allowedSeconds*.78f;game.ResetPose(new Vector3(10,-.65f,12.7f));game.scriptedMove=Vector2.zero;var crow=game.GetComponent<ScarecrowField>().crows[0];crow.position=new Vector3(10,-.75f,25);crowBefore=crow.position;Next();return;}
                if(step==4+route.Length){if(now-stepTime<1.5)return;Check(Vector3.Distance(game.GetComponent<ScarecrowField>().crows[0].position,crowBefore)<.01f,"observed scarecrow remains completely still");game.ResetPose(game.transform.position,180);Next();return;}
                if(step==5+route.Length){if(now-stepTime<1.5)return;Check(game.GetComponent<ScarecrowField>().crows[0].position.z<crowBefore.z-.1f,"unobserved scarecrow approaches continuously");game.deadline.visitElapsed=game.deadline.allowedSeconds-2;Next();return;}
                if(step==6+route.Length){if(now-stepTime<5)return;if(!graceChecked){Check(game.deadline.LastCall&&!game.deadline.missedTrain&&game.deadline.HorrorSeen,"red sky creates horror and a playable last-call grace period");graceChecked=true;}if(now-stepTime<19)return;Check(game.deadline.missedTrain,"late return locks the train and loses this station memory");Check(game.deadline.Danger>.99f,"red sky reaches final warning state without clock HUD");ScreenCapture.CaptureScreenshot("Artifacts/VisualRebuild/penalty-proof.png");Next();return;}
                game.scriptedMove=null;game.scriptedZoom=null;game.scriptedHurry=false;File.WriteAllText("Artifacts/VisualRebuild/play-verification.txt","PASS\n"+log);Debug.Log("ND VISUAL: WALKTHROUGH PASS");step=-1;
            }
            catch(Exception ex){File.WriteAllText("Artifacts/VisualRebuild/play-verification.txt","FAIL\n"+log+ex);Debug.LogException(ex);step=-1;}
        }
        static void Next(){step++;stepTime=EditorApplication.timeSinceStartup;}
        static void Interact(string key)
        {
            var detail=UnityEngine.Object.FindObjectsByType<JourneyDetail>(FindObjectsSortMode.None).First(t=>t.key==key);
            Physics.SyncTransforms();Vector3 dir=detail.transform.position-game.eyes.transform.position;
            bool hit=Physics.Raycast(game.eyes.transform.position,dir.normalized,out var result,2.6f)&&result.collider.GetComponentInParent<JourneyDetail>()==detail;
            Check(hit,key+" reachable by interaction ray");game.eyes.transform.rotation=Quaternion.LookRotation(dir);Check(game.FindInteraction()==detail,key+" acquired by widened click targeting");game.Examine(detail);
        }
        static void CheckWheat()
        {
            float nearest=float.MaxValue;
            foreach(var patch in game.train.wheatPatches)
            {
                var filter=patch.GetComponent<MeshFilter>();if(!filter)continue;
                foreach(var v in filter.sharedMesh.vertices)nearest=Mathf.Min(nearest,Mathf.Abs(patch.TransformPoint(v).x));
            }
            Check(nearest>2.95f,"wheat cards clear train corridor throughout longitudinal travel and wrapping; nearest edge="+nearest);
        }
        static void Check(bool condition,string message){if(!condition)throw new Exception(message);log+="PASS: "+message+"\n";File.WriteAllText("Artifacts/VisualRebuild/runtime-progress.txt",log);}
    }
}
