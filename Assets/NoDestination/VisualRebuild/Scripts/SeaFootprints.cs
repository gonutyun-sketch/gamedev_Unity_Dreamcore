using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class SeaFootprints : MonoBehaviour
    {
        public VisualJourney player;public Material footprintMaterial;
        GameObject[] prints;Vector3 previous;float walked;int next;
        public int Count {get;private set;}
        public bool Noticed {get;private set;}
        public Vector3 Latest => Count>0?prints[(next+prints.Length-1)%prints.Length].transform.position:Vector3.zero;
        void Start()
        {
            prints=new GameObject[180];for(int i=0;i<prints.Length;i++){var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="SEA / persistent footprint";go.transform.SetParent(transform.parent,false);go.GetComponent<Renderer>().sharedMaterial=footprintMaterial;Destroy(go.GetComponent<Collider>());go.SetActive(false);prints[i]=go;}previous=player.transform.position;
        }
        void Update()
        {
            if(player.paused)return;var p=player.transform.position;var delta=p-previous;delta.y=0;previous=p;
            if(player.stage<6)return;
            if(p.x>8&&p.y<-.35f&&player.GetComponent<CharacterController>().isGrounded&&delta.magnitude<.5f)walked+=delta.magnitude;
            if(walked>1.12f)
            {
                walked-=1.12f;var print=prints[next];print.SetActive(true);Vector3 at=p+player.transform.right*(next%2==0?-.095f:.095f);at.y=-.726f;print.transform.SetPositionAndRotation(at,Quaternion.Euler(90,player.transform.eulerAngles.y,0));print.transform.localScale=new Vector3(.15f,.34f,1);next=(next+1)%prints.Length;Count++;
            }
            if(!Noticed&&Count>0&&player.eyes.transform.forward.y<-.35f)
            {
                foreach(var print in prints){if(!print.activeSelf)continue;var offset=print.transform.position-player.eyes.transform.position;if(offset.magnitude<2.8f&&Vector3.Dot(offset.normalized,player.eyes.transform.forward)>.95f){Noticed=true;player.stage=8;player.PresentSubtitle("...발자국?");break;}}
            }
        }
        void OnDestroy(){if(prints!=null)foreach(var p in prints)if(p)Destroy(p);}
    }
}
