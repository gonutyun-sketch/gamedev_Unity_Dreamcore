using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class CraftAndAudio
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        static AudioClip Clip(string name)
        {
            string path=Root+"/Audio/Redesign/"+name+".wav";
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if(!clip)throw new Exception("Missing redesigned audio: "+name);
            return clip;
        }
        static AudioClip[] Steps(string surface)=>Enumerable.Range(0,5).Select(i=>Clip("Step_"+surface+"_"+i)).ToArray();
        public static void Apply(Transform root,VisualJourney journey)
        {
            foreach(var text in root.GetComponentsInChildren<TextMesh>(true))UnityEngine.Object.DestroyImmediate(text.gameObject);
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if(r.name.StartsWith("Destination lettering"))r.gameObject.SetActive(false);
                if(r.name.StartsWith("Door / etched number"))r.transform.localScale*=.55f;
            }
            var craft=VisualWorkshop.Import(Root+"/Models/CarriageCraft.fbx",Vector3.zero,root);craft.name="Train / manufactured fittings and upholstery details";
            TrainAssemblyRepair.Apply(root,craft);
            StaticBatchingUtility.Combine(craft);
            VisualWorkshop.Import(Root+"/Models/CarriagePrintedDetails.fbx",Vector3.zero,root).name="Train / printed route and ticket";
            var stationText=VisualWorkshop.Import(Root+"/Models/FieldSignLetters.fbx",Vector3.zero,root);stationText.name="Station / enamel letters attached to sign";
            // Enamel remains readable in shade; no screen-overlay text or z-fighting with posts.
            var enamel=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/SignEnamel.mat");
            if(!enamel){enamel=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(enamel,Root+"/Materials/SignEnamel.mat");}
            enamel.shader=Shader.Find("Universal Render Pipeline/Unlit");enamel.SetInt("_Cull",0);enamel.SetColor("_BaseColor",new Color(.9f,.86f,.72f));enamel.SetFloat("_Smoothness",.24f);enamel.EnableKeyword("_EMISSION");enamel.SetColor("_EmissionColor",new Color(.9f,.86f,.72f)*.45f);EditorUtility.SetDirty(enamel);
            foreach(var r in stationText.GetComponentsInChildren<Renderer>())r.sharedMaterial=enamel;
            foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Sign / post")t.localPosition+=Vector3.right*.10f;
            journey.train.stationObjects=journey.train.stationObjects.Append(stationText).ToArray();
            var audio=root.gameObject.AddComponent<JourneySoundscape>();audio.train=journey.train;audio.player=journey;journey.soundscape=audio;journey.train.soundscape=audio;
            audio.rolling=Clip("RailRolling");audio.railJoint=Clip("RailJoint");audio.creak=Clip("CabinCreak");audio.wind=Clip("FieldWind");audio.brake=Clip("BrakeRelease");audio.doorOpen=Clip("DoorOpen");audio.doorClose=Clip("DoorClose");audio.latch=Clip("DoorLatch");audio.chime=Clip("PAChime");
            audio.arrival=null;audio.departure=null;audio.radioMemory=null;audio.carpetSteps=Steps("carpet");audio.concreteSteps=Steps("concrete");audio.earthSteps=Steps("grass");audio.woodSteps=Steps("wood");
            audio.memoryMusic=Clip("MemoryMusic");audio.lateDrone=Clip("LateTrainDrone");audio.subtitleSound=Clip("SubtitleTick");
            audio.fieldArrival=Clip("FieldArrival");audio.radioStatic=Clip("RadioStatic");audio.radioTune=Clip("RadioTune");audio.radioMelody=Clip("RadioMelody");audio.clockTick=Clip("ClockTick");audio.houseKnock=Clip("HouseKnock");
            var deadline=root.gameObject.AddComponent<FieldDeadline>();deadline.player=journey;journey.deadline=deadline;
            journey.train.announcement=audio.arrival;journey.radioClip=audio.radioMemory;
        }
    }
}

