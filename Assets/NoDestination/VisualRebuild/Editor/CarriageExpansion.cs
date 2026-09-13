using System.Linq;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class CarriageExpansion
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        static Transform coach;
        static GameObject Box(string n,Vector3 p,Vector3 s,string m,bool collider=false)=>VisualWorkshop.Cube(n,p,s,VisualWorkshop.GetMaterial(m),coach,collider);
        public static void Apply(Transform root,VisualJourney player)
        {
            var prior=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Car 01 / dining car");Object.DestroyImmediate(prior.gameObject);
            coach=new GameObject("Train / accessible dining carriage").transform;coach.SetParent(root,false);
            var shell=VisualWorkshop.Import(Root+"/Models/NoDestination_Carriage.fbx",new Vector3(0,0,19.55f),coach);shell.name="Dining / detailed carriage shell";
            foreach(var t in shell.GetComponentsInChildren<Transform>().ToArray())if(t&&t.name.StartsWith("Seat bay"))Object.DestroyImmediate(t.gameObject);
            var rearDoor=new GameObject("Dining / sliding connection door").transform;rearDoor.SetParent(coach,false);
            foreach(var r in shell.GetComponentsInChildren<Renderer>().ToArray())
            {
                string n=r.name;
                if(n.StartsWith("Rack /")||n.StartsWith("Suitcase /")||n.StartsWith("Cup /")||n.StartsWith("Book /")||n.StartsWith("Ticket /")||n.StartsWith("Table /")){Object.DestroyImmediate(r.gameObject);continue;}
                if(n.StartsWith("Door /")&&r.bounds.center.z<14)r.transform.SetParent(rearDoor,true);
                if(n.StartsWith("Wall / lower")||n.StartsWith("Bulkhead /")||n.StartsWith("Door / inset"))r.gameObject.AddComponent<BoxCollider>();
            }
            Box("Dining / physical oak floor",new Vector3(0,-.08f,19.55f),new Vector3(4.42f,.12f,18),"Walnut",true);
            for(int side=-1;side<=1;side+=2)
            {
                var wall=Box("Dining / glazing collision",new Vector3(side*2.2f,1.7f,19.55f),new Vector3(.06f,1.5f,18),"WindowGlass",true);
                Box("Dining / enamel skirting",new Vector3(side*2.32f,.46f,19.55f),new Vector3(.1f,.90f,18),"Velvet");
                Box("Dining / cream pinstripe",new Vector3(side*2.38f,.82f,19.55f),new Vector3(.016f,.035f,18),"Ivory");
                foreach(float z in new[]{13.35f,14.75f,24.35f,25.75f})
                {
                    var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Dining / steel wheel";wheel.transform.SetParent(coach,false);wheel.transform.position=new Vector3(side*1.22f,-.31f,z);wheel.transform.rotation=Quaternion.Euler(0,0,90);wheel.transform.localScale=new Vector3(.38f,.085f,.38f);wheel.GetComponent<Renderer>().sharedMaterial=VisualWorkshop.GetMaterial("Ink");Object.DestroyImmediate(wheel.GetComponent<Collider>());
                }
            }
            Box("Dining / sealed exterior door",new Vector3(2.2f,1.31f,27.05f),new Vector3(.1f,2.62f,1.5f),"Walnut",true);
            var furniture=VisualWorkshop.Import(Root+"/Models/DiningFurnishings.fbx",new Vector3(0,0,19.55f),coach);
            foreach(var r in furniture.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Dining / rounded walnut tabletop")||r.name.StartsWith("Dining / seat plinth")||r.name.StartsWith("Dining / wood back")||r.name.StartsWith("Dining / service counter"))r.gameObject.AddComponent<BoxCollider>();
            StaticBatchingUtility.Combine(furniture);
            for(int i=0;i<7;i++){var go=new GameObject("Dining / warm pendant light");go.transform.SetParent(coach,false);go.transform.position=new Vector3(0,2.5f,12.05f+i*2.5f);var lamp=go.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=4.1f;lamp.intensity=4.5f;lamp.color=new Color(1,.82f,.59f);}
            foreach(var t in root.GetComponentsInChildren<Transform>(true).ToArray())if(t.name=="Gangway / enclosed bellows"&&t.position.z>0)Object.DestroyImmediate(t.gameObject);
            for(int side=-1;side<=1;side+=2)Box("Gangway / inner flexible sidewall",new Vector3(side*.78f,1.22f,9.75f),new Vector3(.085f,2.44f,1.85f),"Ink",true);
            Box("Gangway / interior canopy",new Vector3(0,2.46f,9.75f),new Vector3(1.65f,.08f,1.85f),"Ink",true);
            Box("Gangway / continuous walkable bridge",new Vector3(0,.005f,9.75f),new Vector3(1.40f,.07f,1.95f),"Brass",true);
            var connection=player.gameObject.AddComponent<CarriageConnection>();connection.player=player;connection.firstDoor=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Sliding vestibule assembly");connection.secondDoor=rearDoor;
            foreach(float z in new[]{8.65f,10.90f})
            {
                var go=new GameObject("Dining connection handle");go.transform.SetParent(coach,false);go.transform.position=new Vector3(.44f,1.12f,z);go.AddComponent<BoxCollider>().size=new Vector3(.15f,.36f,.12f);var detail=go.AddComponent<JourneyDetail>();detail.key=z<9?"coach-door":"coach-return-door";detail.label="식당칸 연결문";go.transform.SetParent(z<9?connection.firstDoor:rearDoor,true);
            }
            var locked=new GameObject("Locked sleeping carriage");locked.transform.SetParent(root,false);locked.transform.position=new Vector3(.44f,1.12f,-8.65f);locked.AddComponent<BoxCollider>().size=new Vector3(.15f,.36f,.12f);var d=locked.AddComponent<JourneyDetail>();d.key="locked-coach";d.label="잠긴 침대칸";d.text="문은 잠겨 있다.\n안쪽에서는 아무 소리도 들리지 않는다.";
            new GameObject("Train / reserved later spatial disturbances").transform.SetParent(root,false);
        }
    }
}
