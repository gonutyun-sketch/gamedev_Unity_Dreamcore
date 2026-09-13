using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class ScarecrowField : MonoBehaviour
    {
        public VisualJourney player;
        public Transform[] crows;
        public float Threat {get;private set;}
        public bool AnyMoved {get;private set;}
        public bool Active => player.stage>=2&&player.stage<5&&player.deadline.Danger>.22f;
        Plane[] planes;
        public bool Observed(Transform crow)
        {
            var bounds=new Bounds(crow.position+Vector3.up*1.25f,new Vector3(1.9f,2.7f,.7f));
            if(!GeometryUtility.TestPlanesAABB(planes??GeometryUtility.CalculateFrustumPlanes(player.eyes),bounds))return false;
            var origin=player.eyes.transform.position;var offset=bounds.center-origin;
            foreach(var hit in Physics.RaycastAll(origin,offset.normalized,offset.magnitude-.3f))
                if(!hit.transform.IsChildOf(player.transform)&&!hit.transform.IsChildOf(crow))return false;
            return true;
        }
        void Update()
        {
            if(player.paused)return;planes=GeometryUtility.CalculateFrustumPlanes(player.eyes);float nearest=1000;
            bool aboard=Mathf.Abs(player.transform.position.x)<3&&Mathf.Abs(player.transform.position.z)<34;
            float houseZ=player.GetComponent<FieldSpatialRules>()?.HouseZ??65;
            foreach(var crow in crows)
            {
                if(!crow||!crow.gameObject.activeInHierarchy)continue;
                var delta=player.transform.position-crow.position;delta.y=0;nearest=Mathf.Min(nearest,delta.magnitude);
                if(!Active||aboard||Observed(crow)||delta.magnitude<6.5f)continue;
                var next=crow.position+delta.normalized*Mathf.Lerp(.15f,.85f,player.deadline.Danger)*Time.deltaTime;
                if(Mathf.Abs(next.x)<4.7f)continue;
                if(Mathf.Abs(next.x-22)<6&&Mathf.Abs(next.z-houseZ)<6)
                {
                    next=crow.position+Vector3.Cross(Vector3.up,delta.normalized)*Time.deltaTime*.5f;
                    if(Mathf.Abs(next.x-22)<5.7f&&Mathf.Abs(next.z-houseZ)<5.7f)continue;
                }
                crow.position=next;AnyMoved=true;
            }
            Threat=Mathf.MoveTowards(Threat,Active&&!aboard?Mathf.InverseLerp(24,6,nearest)*player.deadline.Danger:0,Time.deltaTime*.35f);
        }
    }
}
