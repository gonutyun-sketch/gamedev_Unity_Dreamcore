using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class HouseChapterBuilder
    {
        static Transform room;
        static GameObject Box(string name,Vector3 p,Vector3 s,string mat,bool collision=false)=>VisualWorkshop.Cube("House / "+name,p,s,VisualWorkshop.GetMaterial(mat),room,collision);
        static JourneyDetail Detail(string key,string label,Vector3 p,Vector3 size,string text="")
        {
            var go=new GameObject(label);go.transform.SetParent(room,false);go.transform.position=p;go.AddComponent<BoxCollider>().size=size;
            var d=go.AddComponent<JourneyDetail>();d.key=key;d.label=label;d.text=text;return d;
        }
        static void Rod(string name,Vector3 a,Vector3 b,float thickness,string mat)
        {
            var go=Box(name,(a+b)*.5f,new Vector3(thickness,thickness,Vector3.Distance(a,b)),mat);go.transform.rotation=Quaternion.LookRotation(b-a);
        }
        public static void Apply(Transform root,VisualJourney journey)
        {
            var house=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="03 / House 317");
            var extension=new GameObject("House / second floor and connected staircase");room=extension.transform;room.SetParent(root,false);
            // Reuse the carefully modelled clapboards, apertures and casing of the existing house.
            foreach(var r in house.GetComponentsInChildren<MeshRenderer>(true).ToArray())
            {
                string n=r.name;
                if(n.StartsWith("Roof /")||n.StartsWith("House / gable")){r.transform.position+=Vector3.up*3.2f;continue;}
                bool wall=n.StartsWith("House / side")||n.StartsWith("House / beneath")||n.StartsWith("House / back")||n.StartsWith("House / corner")||n.StartsWith("House / door")||n.StartsWith("Siding /")||n.StartsWith("Window /")||n.StartsWith("Front /")||n.StartsWith("Front window /")||n.StartsWith("Interior / moulding");
                if(wall){var copy=Object.Instantiate(r.gameObject,room,true);copy.name="House / upper / "+n;copy.transform.position+=Vector3.up*3.2f;}
                if(n.StartsWith("Photograph /")||n=="Memory / aged family print")r.transform.position+=Vector3.up*3.2f;
            }
            root.GetComponentsInChildren<JourneyDetail>(true).First(d=>d.key=="photo").transform.position+=Vector3.up*3.2f;
            Box("upper front centre panel",new Vector3(22,4.3f,60.9f),new Vector3(2.16f,3,.2f),"Ivory",true);
            // A real floor opening leaves headroom above the entire stair flight.
            Box("upper load bearing floor",new Vector3(21.05f,2.65f,65),new Vector3(6.5f,.18f,8.16f),"Walnut",true);
            Box("upper rear landing",new Vector3(25.3f,2.65f,68.32f),new Vector3(2,.18f,1.52f),"Walnut",true);
            for(int i=0;i<25;i++)Box("upper individually jointed boards",new Vector3(21.05f,2.753f,61.02f+i*.33f),new Vector3(6.48f,.026f,.322f),"Panel");
            for(int i=0;i<18;i++)
            {
                float top=-.46f+(i+1)*(3.2f/18),z=61.9f+i*.32f;
                Box("stair solid riser "+i,new Vector3(25.25f,(top-.46f)/2,z),new Vector3(1.65f,top+.46f,.32f),"Ivory",true);
                Box("stair oak tread "+i,new Vector3(25.25f,top-.017f,z-.015f),new Vector3(1.72f,.034f,.35f),"Walnut");
                Box("stair brass nosing "+i,new Vector3(25.25f,top+.002f,z-.166f),new Vector3(1.68f,.014f,.022f),"Brass");
                if(i%2==0)for(int side=0;side<2;side++)Box("stair baluster",new Vector3(24.4f+side*1.7f,top+.48f,z),new Vector3(.045f,.96f,.045f),"Walnut");
            }
            for(int side=0;side<2;side++)Rod("continuous stair handrail",new Vector3(24.4f+side*1.7f,.61f,61.9f),new Vector3(24.4f+side*1.7f,3.63f,67.34f),.075f,"Walnut");
            // Upper guard protects the floor opening while leaving the rear landing accessible.
            for(int i=0;i<19;i++)Box("upper guard baluster",new Vector3(24.23f,3.22f,61.1f+i*.34f),new Vector3(.05f,.95f,.05f),"Walnut");
            Box("upper guard handrail",new Vector3(24.23f,3.72f,64.16f),new Vector3(.085f,.08f,6.3f),"Walnut");
            var guard=Box("upper guard collision",new Vector3(24.23f,3.25f,64.16f),new Vector3(.08f,1,6.3f),"Walnut",true);guard.GetComponent<Renderer>().enabled=false;
            for(int side=-1;side<=1;side+=2)Box("storey belt moulding",new Vector3(22+side*4.5f,2.64f,65),new Vector3(.18f,.2f,8.35f),"Paper");
            Box("front storey belt",new Vector3(22,2.64f,60.79f),new Vector3(9.1f,.2f,.18f),"Paper");
            // Upstairs is a furnished memory room, with legs, fabric borders and backed bookcase.
            Box("woven upstairs rug",new Vector3(21.4f,2.78f,65.5f),new Vector3(3.6f,.025f,3.3f),"Velvet");
            for(int side=-1;side<=1;side+=2)Box("rug stitched border",new Vector3(21.4f+side*1.72f,2.797f,65.5f),new Vector3(.04f,.008f,3.15f),"Ivory");
            Box("bed frame",new Vector3(18.6f,3.06f,64.6f),new Vector3(1.45f,.22f,2.3f),"Walnut",true);
            Box("bed quilt",new Vector3(18.6f,3.25f,64.5f),new Vector3(1.4f,.20f,2.15f),"Paper");
            Box("folded blanket",new Vector3(18.6f,3.37f,63.95f),new Vector3(1.42f,.07f,.75f),"Velvet");
            Box("pillow",new Vector3(18.6f,3.41f,65.25f),new Vector3(.95f,.20f,.42f),"Ivory");
            Box("bed headboard",new Vector3(18.6f,3.43f,65.78f),new Vector3(1.52f,1.25f,.09f),"Walnut");
            foreach(float x in new[]{17.98f,19.22f})foreach(float z in new[]{63.62f,65.57f})Box("bed leg",new Vector3(x,2.93f,z),new Vector3(.10f,.36f,.10f),"Walnut");
            Box("bookcase back",new Vector3(19.1f,3.7f,68.91f),new Vector3(2.2f,1.9f,.1f),"Panel");
            foreach(float x in new[]{18f,20.2f})Box("bookcase side",new Vector3(x,3.7f,68.64f),new Vector3(.10f,1.9f,.6f),"Walnut",true);
            for(int s=0;s<4;s++)
            {
                Box("bookcase shelf",new Vector3(19.1f,2.83f+s*.55f,68.64f),new Vector3(2.2f,.075f,.6f),"Walnut");
                for(int b=0;b<8;b++)Box("cloth bound book",new Vector3(18.23f+b*.23f,3.05f+s*.55f,68.68f),new Vector3(.12f+.02f*(b%3),.35f,.34f),b%3==0?"Velvet":b%3==1?"Ivory":"Panel");
            }
            var puzzle=extension.AddComponent<HouseMemoryPuzzle>();puzzle.player=journey;journey.housePuzzle=puzzle;
            // Analogue clue: clock body against the inside front wall; hands point at 03:17.
            Box("clock walnut case",new Vector3(19.3f,1.4f,61.06f),new Vector3(.70f,.9f,.15f),"Walnut");
            Box("clock ivory dial",new Vector3(19.3f,1.5f,61.15f),new Vector3(.57f,.57f,.025f),"Paper");
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Box("clock hour mark",new Vector3(19.3f+Mathf.Sin(a)*.24f,1.5f+Mathf.Cos(a)*.24f,61.17f),new Vector3(.025f,.025f,.01f),"Ink");}
            Rod("clock minute hand",new Vector3(19.3f,1.5f,61.19f),new Vector3(19.51f,1.455f,61.19f),.014f,"Ink");
            Rod("clock hour hand",new Vector3(19.3f,1.5f,61.19f),new Vector3(19.43f,1.47f,61.19f),.023f,"Ink");
            Detail("house-clock","멈춘 벽시계",new Vector3(19.3f,1.43f,61.23f),new Vector3(.75f,.95f,.10f));
            Detail("house-journal","침대 위의 일기",new Vector3(18.8f,3.48f,64),new Vector3(.34f,.08f,.42f),"마지막 기록 — 세 시 십칠 분.\n라디오가 멈추고, 집 안의 불이 꺼졌다.\n사진만은 이층에 두고 왔다.");
            Box("open diary cover",new Vector3(18.8f,3.46f,64),new Vector3(.36f,.04f,.44f),"Panel");
            Box("diary paper",new Vector3(18.8f,3.486f,64),new Vector3(.32f,.015f,.4f),"Paper");
            for(int i=0;i<7;i++)Box("diary ink line",new Vector3(18.8f,3.496f,63.86f+i*.045f),new Vector3(.24f,.003f,.003f),"Ink");
            Box("power cabinet attached to wall",new Vector3(21.9f,4.06f,68.87f),new Vector3(2.5f,1.1f,.25f),"Velvet");
            for(int side=-1;side<=1;side+=2)foreach(float y in new[]{3.58f,4.54f})Box("panel screw",new Vector3(21.9f+side*1.17f,y,68.735f),new Vector3(.035f,.035f,.012f),"Brass");
            var segs=new List<GameObject>();
            for(int d=0;d<3;d++)
            {
                float x=21.1f+d*.62f;Box("dial ivory wheel",new Vector3(x,4.17f,68.69f),new Vector3(.44f,.57f,.08f),"Ivory");
                var offsets=new[]{new Vector2(0,.19f),new Vector2(.13f,.095f),new Vector2(.13f,-.095f),new Vector2(0,-.19f),new Vector2(-.13f,-.095f),new Vector2(-.13f,.095f),Vector2.zero};
                for(int s=0;s<7;s++){bool horizontal=s==0||s==3||s==6;segs.Add(Box("mechanical numeral",new Vector3(x+offsets[s].x,4.17f+offsets[s].y,68.641f),horizontal?new Vector3(.23f,.027f,.014f):new Vector3(.027f,.17f,.014f),"Ink"));}
                Detail("house-dial-"+d,(d+1)+"번째 숫자 돌리기",new Vector3(x,4.17f,68.62f),new Vector3(.45f,.58f,.07f));
            }
            puzzle.segments=segs.ToArray();
            Box("power lever base",new Vector3(21.9f,3.73f,68.65f),new Vector3(.55f,.20f,.13f),"Ink");
            Box("power lever grip",new Vector3(21.9f,3.73f,68.53f),new Vector3(.40f,.08f,.18f),"Brass");
            Detail("house-power","전원 스위치 올리기",new Vector3(21.9f,3.7f,68.42f),new Vector3(.62f,.25f,.15f));
            puzzle.powerLamp=Box("power indicator",new Vector3(22.93f,3.74f,68.72f),new Vector3(.12f,.12f,.04f),"Ink").GetComponent<Renderer>();puzzle.litMaterial=VisualWorkshop.GetMaterial("MilkGlass");
            Rod("electrical conduit",new Vector3(20.61f,4.1f,68.86f),new Vector3(20.61f,.68f,68.86f),.035f,"Brass");
            Rod("radio cable along skirting",new Vector3(20.61f,.68f,68.86f),new Vector3(20,.68f,66.44f),.018f,"Ink");
            var lamps=new List<Light>();
            foreach(var p in new[]{new Vector3(21,5.25f,65),new Vector3(21,1.9f,65)})
            {
                var go=new GameObject("House / restored warm lamplight");go.transform.SetParent(room,false);go.transform.position=p;var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.77f,.46f);light.range=7;light.intensity=2.2f;lamps.Add(light);
            }
            Rod("upper pendant cord",new Vector3(21,5.65f,65),new Vector3(21,5.3f,65),.012f,"Ink");
            Box("upper pendant shade",new Vector3(21,5.3f,65),new Vector3(.42f,.12f,.42f),"MilkGlass");puzzle.lamps=lamps.ToArray();
            journey.objectives[2]="집 안의 시계에서 단서를 찾아, 이층 전원함을 복구하세요.";
            journey.objectives[3]="멜로디가 떠올린 이층 방의 가족사진을 조사하세요.";
            journey.deadline.allowedSeconds=420;
            journey.train.stationObjects=journey.train.stationObjects.Append(extension).ToArray();
        }
    }
}
