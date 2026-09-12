using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public enum JourneySurface { Carpet, Concrete, Earth, Wood }
    public sealed class JourneySoundscape : MonoBehaviour
    {
        public TrainDreamMotion train;
        public VisualJourney player;
        public AudioClip rolling,railJoint,creak,wind,brake,doorOpen,doorClose,latch,chime,arrival,departure,radioMemory;
        public AudioClip[] carpetSteps,concreteSteps,earthSteps,woodSteps;
        public int footfalls {get;private set;}
        public JourneySurface lastSurface {get;private set;}
        public bool DialoguePlaying => false;
        AudioSource wheels,joints,cabin,outdoor,voice,radio,door,feet,music,dread,subtitle;
        public AudioClip memoryMusic,lateDrone,subtitleSound,fieldArrival,radioStatic,radioTune,radioMelody,clockTick,houseKnock;
        AudioSource arrivalCue,clock,mechanism,knocking;
        public bool FieldMusicStarted {get;private set;}
        public bool RadioPowered {get;private set;}
        public bool RadioPlaying => radio&&radio.isPlaying;
        public bool MusicPlaying => music&&music.isPlaying;
        float fieldAge;
        AudioLowPassFilter rollingFilter;
        Vector3 previousFoot;
        float distanceSinceStep,jointDistance,nextCreak;
        int lastVariant=-1;
        bool opened,braked,saidArrival,saidDeparture;
        AudioSource Source(string title,Transform parent,Vector3 position,float spatial,float volume,bool loop=false,AudioClip clip=null)
        {
            var go=new GameObject(title);go.transform.SetParent(parent);go.transform.position=position;
            var a=go.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=spatial;a.volume=volume;a.loop=loop;a.clip=clip;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Logarithmic;a.minDistance=2;a.maxDistance=35;
            if(loop&&clip)a.Play();return a;
        }
        void Start()
        {
            previousFoot=player.transform.position;
            music=Source("Music / the things we remember",transform,Vector3.zero,0,0,false,memoryMusic);music.loop=true;
            arrivalCue=Source("Music / first sight of THE FIELD",transform,Vector3.zero,0,.48f);
            radio=Source("House / radio speaker",transform,new Vector3(20,.73f,66.3f),1,.2f);radio.maxDistance=10;radio.minDistance=.8f;
            clock=Source("House / clock mechanism",transform,new Vector3(19.3f,1.3f,61.1f),1,.12f);clock.clip=clockTick;clock.loop=true;clock.maxDistance=6;clock.minDistance=.6f;
            mechanism=Source("House / physical switch",transform,new Vector3(22,3.9f,68.7f),1,.4f);
            knocking=Source("House / knocking beyond the wall",transform,new Vector3(27,1.4f,67),1,.5f);
            dread=Source("Music / missed train undertone",transform,Vector3.zero,0,0,true,lateDrone);
            subtitle=Source("Subtitles / soft paper tick",transform,Vector3.zero,0,.24f);
            wheels=Source("Recorded rolling / underframe",transform,new Vector3(0,-.4f,0),0,.16f,true,rolling);rollingFilter=wheels.gameObject.AddComponent<AudioLowPassFilter>();
            joints=Source("Wheel joints",transform,new Vector3(0,-.4f,0),0,.09f);
            cabin=Source("Carriage wood and fittings",transform,new Vector3(1.5f,1.4f,-2),.35f,.055f);
            outdoor=Source("Wind beyond windows",transform,Vector3.zero,0,.03f,true,wind);

            door=Source("Vestibule door mechanism",transform,new Vector3(2.1f,1.1f,7.5f),1,.38f);
            feet=Source("Player footfalls",player.transform,player.transform.position,0,.32f);nextCreak=Time.time+7;
        }
        public void SubtitleTick(){if(subtitle&&subtitleSound)subtitle.PlayOneShot(subtitleSound,.8f);}
        public void EnterField(){if(FieldMusicStarted)return;FieldMusicStarted=true;fieldAge=0;arrivalCue.PlayOneShot(fieldArrival);clock.Play();}
        public void PowerRadio(){RadioPowered=true;radio.clip=radioStatic;radio.loop=true;radio.Play();PlaySwitch();}
        public void PlaySwitch(){if(mechanism&&latch)mechanism.PlayOneShot(latch,.55f);}
        public void Knock(){if(knocking&&houseKnock)knocking.PlayOneShot(houseKnock);}
        public void PlayRadio(){if(RadioPowered)StartCoroutine(TuneRadio());}
        System.Collections.IEnumerator TuneRadio(){radio.Stop();radio.PlayOneShot(radioTune);yield return new WaitForSeconds(2);radio.clip=radioMelody;radio.loop=true;radio.Play();}
        public void PlayArrival(){if(!saidArrival){saidArrival=true;player.PresentSubtitle("[차내 안내] 잠시 후 밀밭 역에 도착합니다.\n내리실 문은 오른쪽입니다.",9);}}
        public void SetDoor(bool value)
        {
            if(value==opened||!door)return;opened=value;door.PlayOneShot(value?doorOpen:doorClose);
            if(!value)StartCoroutine(LatchAfterClose());
        }
        System.Collections.IEnumerator LatchAfterClose(){yield return new WaitForSeconds(.95f);if(door&&latch)door.PlayOneShot(latch,.65f);}
        JourneySurface Surface()
        {
            if(Physics.Raycast(player.transform.position+Vector3.up*.3f,Vector3.down,out var hit,2,~0,QueryTriggerInteraction.Ignore))
            {
                string n=hit.collider.name;
                if(n.Contains("Carriage")||n.Contains("oak boards")||n.Contains("carpet"))return JourneySurface.Carpet;
                if(n.Contains("Platform"))return JourneySurface.Concrete;
                if(n.Contains("House")||n.Contains("Porch")||n.Contains("Front /"))return JourneySurface.Wood;
            }
            return JourneySurface.Earth;
        }
        void Footstep()
        {
            lastSurface=Surface();var set=lastSurface switch{JourneySurface.Carpet=>carpetSteps,JourneySurface.Concrete=>concreteSteps,JourneySurface.Wood=>woodSteps,_=>earthSteps};
            if(set==null||set.Length==0)return;
            int pick=Random.Range(0,set.Length);if(set.Length>1&&pick==lastVariant)pick=(pick+1)%set.Length;lastVariant=pick;
            feet.pitch=Random.Range(.97f,1.03f);feet.PlayOneShot(set[pick],lastSurface==JourneySurface.Carpet?.60f:.85f);footfalls++;
        }
        void Update()
        {
            if(!player||!train||player.paused)return;
            if(FieldMusicStarted){fieldAge+=Time.deltaTime;if(fieldAge>=2f&&!music.isPlaying)music.Play();}
            float danger=player.deadline&&player.stage<5?player.deadline.Danger:0;music.volume=Mathf.Lerp(music.volume,FieldMusicStarted&&fieldAge>=2f&&!(player.deadline&&player.deadline.missedTrain)?Mathf.Lerp(.23f,.07f,danger):0,Time.deltaTime);dread.volume=Mathf.Lerp(dread.volume,danger*.19f,Time.deltaTime);
            if(train.departing){radio.volume=Mathf.MoveTowards(radio.volume,0,Time.deltaTime*.1f);clock.volume=Mathf.MoveTowards(clock.volume,0,Time.deltaTime*.1f);}
            var p=player.transform.position;bool inside=Mathf.Abs(p.x)<2.5f&&Mathf.Abs(p.z)<9.5f;
            float speed=Mathf.Clamp01(train.speed/13),duck=DialoguePlaying?.48f:1;
            float proximity=inside?1:Mathf.Clamp01(1-new Vector2(Mathf.Max(0,Mathf.Abs(p.x)-3),Mathf.Max(0,Mathf.Abs(p.z)-30)).magnitude/22);
            wheels.volume=Mathf.Lerp(wheels.volume,speed*.15f*duck*proximity,Time.deltaTime*3);wheels.pitch=Mathf.Lerp(.80f,1,speed);rollingFilter.cutoffFrequency=inside?1100:4500;
            joints.volume=(inside?.055f:.09f)*duck*proximity;cabin.volume=.025f*speed*duck*(inside?1:proximity*.25f);
            outdoor.volume=Mathf.Lerp(outdoor.volume,inside?.012f:.075f,Time.deltaTime*2);
            jointDistance+=train.speed*Time.deltaTime;
            if(jointDistance>12.5f){jointDistance-=12.5f;joints.pitch=Random.Range(.97f,1.03f);joints.PlayOneShot(railJoint);}
            if(speed>.3f&&Time.time>nextCreak){cabin.pitch=Random.Range(.9f,1.1f);cabin.PlayOneShot(creak);nextCreak=Time.time+Random.Range(18,30);}
            if(!braked&&train.elapsed>train.journeySeconds-2){braked=true;door.PlayOneShot(brake,.65f);}
            if(train.departing&&!saidDeparture){saidDeparture=true;player.PresentSubtitle("[차내 안내] 문이 닫힙니다.\n열차가 다음 역으로 출발합니다.",8);}
            Vector3 delta=p-previousFoot;delta.y=0;previousFoot=p;
            if(!player.IsWaking&&delta.magnitude<.5f&&player.GetComponent<CharacterController>().isGrounded)distanceSinceStep+=delta.magnitude;
            if(distanceSinceStep>1.12f){distanceSinceStep-=1.12f;Footstep();}
        }
    }
}
