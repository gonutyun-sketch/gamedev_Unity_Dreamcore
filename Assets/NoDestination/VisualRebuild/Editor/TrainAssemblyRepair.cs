using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class TrainAssemblyRepair
    {
        static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,string material)=>VisualWorkshop.Cube(name,p,size,VisualWorkshop.GetMaterial(material),parent,false);
        static void Rod(Transform parent,string name,Vector3 a,Vector3 b,float radius,string material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=(a+b)*.5f;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);go.transform.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);go.GetComponent<Renderer>().sharedMaterial=VisualWorkshop.GetMaterial(material);Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        public static void Apply(Transform root,GameObject craft)
        {
            foreach(var r in root.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Rack / cast bracket"))r.transform.position+=Vector3.back*1.22f;
            // Mount the pocket against the back shell and the number plate against the seat frame.
            foreach(var r in craft.GetComponentsInChildren<Renderer>())
            {
                if(r.name.StartsWith("Seat / document pocket")||r.name.StartsWith("Pocket /"))r.transform.position+=Vector3.back*.10f;
                if(r.name.StartsWith("Seat / reservation plate")||r.name.StartsWith("Seat / engraved number"))r.transform.position+=Vector3.forward*.30f;
            }
            var assembly=new GameObject("Train / connected mounting hardware").transform;assembly.SetParent(root,false);
            for(int side=-1;side<=1;side+=2)
            {
                foreach(float x in new[]{1.69f,1.81f,1.93f,2.05f}){Rod(assembly,"Rack / rail end extension",new Vector3(side*x,2.38f,-8.74f),new Vector3(side*x,2.38f,-8.49f),.013f,"Brass");Rod(assembly,"Rack / rail end extension",new Vector3(side*x,2.38f,8.49f),new Vector3(side*x,2.38f,8.80f),.013f,"Brass");}
                foreach(float z in new[]{-8.72f,-6.22f,-3.72f,-1.22f,1.28f,3.78f,6.28f,8.78f})
                {
                    Rod(assembly,"Rack / transverse bearing",new Vector3(side*1.66f,2.365f,z),new Vector3(side*2.12f,2.365f,z),.017f,"Brass");
                    Box(assembly,"Rack / wall anchor plate",new Vector3(side*2.135f,2.19f,z),new Vector3(.035f,.22f,.09f),"Brass");
                    Rod(assembly,"Rack / continuous wall strut",new Vector3(side*2.09f,2.12f,z),new Vector3(side*1.67f,2.365f,z),.018f,"Brass");
                }
                for(int i=0;i<7;i++)
                {
                    float z=-7.5f+i*2.5f;if(side==1&&i==6)continue;
                    foreach(int end in new[]{-1,1})
                    {
                        float anchor=z+end*1.12f;
                        Box(assembly,"Curtain / wall rosette",new Vector3(side*2.09f,1.51f,anchor),new Vector3(.035f,.09f,.07f),"Brass");
                        Rod(assembly,"Curtain / tieback returning to wall",new Vector3(side*1.91f,1.505f,anchor),new Vector3(side*2.10f,1.51f,anchor),.011f,"Brass");
                    }
                }
            }
            foreach(int end in new[]{-1,1})
            {
                // Bellows bridge the vestibule gap; the coupler carries the connection underneath.
                Box(assembly,"Gangway / enclosed bellows",new Vector3(0,1.22f,end*9.68f),new Vector3(1.58f,2.44f,1.38f),"Ink");
                for(int i=0;i<9;i++)
                {
                    float z=end*(9.1f+i*.145f);
                    foreach(int side in new[]{-1,1})Box(assembly,"Gangway / concertina fold",new Vector3(side*.82f,1.22f,z),new Vector3(.065f,2.46f,.038f),"Walnut");
                    Box(assembly,"Gangway / upper fold",new Vector3(0,2.45f,z),new Vector3(1.68f,.06f,.038f),"Walnut");
                }
                Box(assembly,"Coupler / drawbar joining cars",new Vector3(0,-.20f,end*9.7f),new Vector3(.23f,.20f,1.75f),"Ink");
                Box(assembly,"Gangway / threshold bridge",new Vector3(0,.025f,end*9.7f),new Vector3(1.30f,.06f,1.62f),"Brass");
            }
        }
    }
}


