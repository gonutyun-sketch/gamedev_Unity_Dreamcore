using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class SeaChapterBuilder
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        static Material Material(string name,string shader)
        {
            string p=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,p);}m.shader=Shader.Find(shader);EditorUtility.SetDirty(m);return m;
        }
        public static void Apply(Transform root,VisualJourney player)
        {
            var sea=new GameObject("SEA / an impossible flat world");sea.transform.SetParent(root,false);
            var plane=GameObject.CreatePrimitive(PrimitiveType.Plane);plane.name="SEA / water that supports footsteps";plane.transform.SetParent(sea.transform,false);plane.transform.position=new Vector3(0,-.735f,0);plane.transform.localScale=new Vector3(400,1,400);plane.GetComponent<Renderer>().sharedMaterial=Material("SeaSurface","NoDestination/SeaSurface");plane.GetComponent<Collider>().enabled=false;
            var station=new GameObject("SEA / a platform without a shore");station.transform.SetParent(root,false);
            VisualWorkshop.Cube("SEA / pier beneath the train",new Vector3(0,-.6f,8),new Vector3(4,.14f,83),VisualWorkshop.GetMaterial("Ivory"),station.transform,true);
            VisualWorkshop.Cube("SEA / pale landing",new Vector3(5,-.045f,5),new Vector3(5.5f,.08f,18),VisualWorkshop.GetMaterial("Ivory"),station.transform,true);
            for(int i=0;i<4;i++)VisualWorkshop.Cube("SEA / steps into water",new Vector3(7.94f+i*.37f,-.1f-i*.18f,12.7f),new Vector3(.4f,.2f,2.5f),VisualWorkshop.GetMaterial("Ivory"),station.transform,true);
            station.SetActive(false);
            var transit=player.gameObject.AddComponent<RealityTransit>();transit.player=player;transit.seaStation=station;transit.seaFloor=plane.GetComponent<Collider>();
            transit.fieldOnly=root.GetComponentsInChildren<Transform>(true).Where(t=>new[]{"02 / Gold beneath a blue sky","FIELD / distant wheat terrain to the horizon","Railway / continuous line"}.Contains(t.name)).Select(t=>t.gameObject).ToArray();
            transit.carriageLights=root.GetComponentsInChildren<Light>().Where(l=>l.name=="Pendant light"||l.name=="Dining / warm pendant light").ToArray();
            var windows=new System.Collections.Generic.List<Renderer>();
            // Two windows remain FIELD. Neighbouring panes reveal empty space and SEA simultaneously.
            foreach(var spec in new[]{new Vector3(1,-5,1),new Vector3(1,-2.5f,2),new Vector3(-1,0,2),new Vector3(-1,5,1)})
            {
                var pane=GameObject.CreatePrimitive(PrimitiveType.Quad);pane.name="Transit / a different reality in this window";pane.transform.SetParent(root,false);pane.transform.position=new Vector3(spec.x*2.225f,1.72f,spec.y);pane.transform.rotation=Quaternion.Euler(0,90,0);pane.transform.localScale=new Vector3(2.4f,1.32f,1);Object.DestroyImmediate(pane.GetComponent<Collider>());
                var m=Material(spec.z==1?"WindowNowhere":"WindowSea","NoDestination/ImpossibleWindow");m.SetFloat("_Mode",spec.z);m.SetFloat("_Delay",spec.z==1?0:.2f);pane.GetComponent<Renderer>().sharedMaterial=m;windows.Add(pane.GetComponent<Renderer>());
            }
            transit.impossibleWindows=windows.ToArray();
            var puddle=GameObject.CreatePrimitive(PrimitiveType.Quad);puddle.name="Transit / the sea reflected before arrival";puddle.transform.SetParent(root,false);puddle.transform.position=new Vector3(0,.034f,0);puddle.transform.rotation=Quaternion.Euler(90,0,0);puddle.transform.localScale=new Vector3(.94f,17,1);puddle.GetComponent<Renderer>().sharedMaterial=Material("SeaPremonition","NoDestination/SeaPremonition");Object.DestroyImmediate(puddle.GetComponent<Collider>());
            var footprints=player.gameObject.AddComponent<SeaFootprints>();footprints.player=player;footprints.footprintMaterial=Material("WaterFootprint","NoDestination/WaterFootprint");
            player.objectives=player.objectives.Concat(new[]{"열차 밖의 바다를 살펴보세요.","수면 위에 남은 흔적을 살펴보세요.","이곳에도 기억이 남아 있을까."}).ToArray();
            player.soundscape.seaMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Redesign/SeaStillness.wav");player.soundscape.seaWaves=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Redesign/SeaLapping.wav");player.soundscape.waterSteps=Enumerable.Range(0,5).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Redesign/Step_water_"+i+".wav")).ToArray();
        }
    }
}
