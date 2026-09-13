using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public enum JourneySurface { Carpet, Concrete, Earth, Wood, Water }
    public sealed class JourneySoundscape : MonoBehaviour
    {
        public TrainDreamMotion train;
        public VisualJourney player;
        public AudioClip rolling,railJoint,creak,wind,brake,doorOpen,doorClose,latch,chime,arrival,departure,radioMemory;
        public AudioClip[] carpetSteps,concreteSteps,earthSteps,woodSteps,waterSteps;
        public AudioClip seaMusic,seaWaves;AudioSource seaWater;
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
        float fieldAge; AudioSource pa,coachDoor;
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
            seaWater=Source("SEA / near-silent water",transform,Vector3.zero,0,0,true,seaWaves);
            music=Source("Music / the things we remember",transform,Vector3.zero,0,0,false,memoryMusic);music.loop=true;
            arrivalCue=Source("Music / first sight of THE FIELD",transform,Vector3.zero,0,0);
            radio=Source("House / radio speaker",transform,new Vector3(20,.73f,66.3f),1,.28f);radio.maxDistance=10;radio.minDistance=.8f;
            clock=Source("House / clock mechanism",transform,new Vector3(19.3f,1.3f,61.1f),1,.22f);clock.clip=clockTick;clock.loop=true;clock.maxDistance=6;clock.minDistance=.6f;
            mechanism=Source("House / physical switch",transform,new Vector3(22,3.9f,68.7f),1,.4f);
            knocking=Source("House / knocking beyond the wall",transform,new Vector3(27,1.4f,67),1,.5f);
            dread=Source("Music / missed train undertone",transform,Vector3.zero,0,0,true,lateDrone);
            coachDoor=Source("Train / dining connection mechanism",transform,new Vector3(0,1.2f,9.75f),1,.3f);
            pa=Source("Carriage / old announcement speaker",transform,new Vector3(0,2.5f,0),.25f,.24f);
            subtitle=Source("Subtitles / soft paper tick",transform,Vector3.zero,0,.24f);
            wheels=Source("Recorded rolling / underframe",transform,new Vector3(0,-.4f,0),0,.16f,true,rolling);rollingFilter=wheels.gameObject.AddComponent<AudioLowPassFilter>();
            joints=Source("Wheel joints",transform,new Vector3(0,-.4f,0),0,.09f);
            cabin=Source("Carriage wood and fittings",transform,new Vector3(1.5f,1.4f,-2),.35f,.055f);
            outdoor=Source("Wind beyond windows",transform,Vector3.zero,0,.03f,true,wind);

            door=Source("Vestibule door mechanism",transform,new Vector3(2.1f,1.1f,7.5f),1,.38f);
            feet=Source("Player footfalls",player.transform,player.transform.position,0,.32f);nextCreak=Time.time+7;
        }
        public void SubtitleTick(){if(subtitle&&subtitleSound)subtitle.PlayOneShot(subtitleSound,.8f);}
        public void EnterField(){if(FieldMusicStarted)return;FieldMusicStarted=true;fieldAge=0;arrivalCue.clip=fieldArrival;arrivalCue.Play();clock.Play();}
        public void EnterSea(){music.Stop();music.clip=seaMusic;music.volume=0;music.Play();radio.Stop();clock.Stop();}
        public void PlayCoachDoor(bool opening){coachDoor.PlayOneShot(opening?doorOpen:doorClose);}
        public void MoveHouseAudio(Vector3 shift){radio.transform.position+=shift;clock.transform.position+=shift;mechanism.transform.position+=shift;knocking.transform.position+=shift;}
        public void PowerRadio(){RadioPowered=true;radio.clip=radioStatic;radio.loop=true;radio.Play();PlaySwitch();}
        public void PlaySwitch(){if(mechanism&&latch)mechanism.PlayOneShot(latch,.55f);}
        public void Knock(){if(knocking&&houseKnock)knocking.PlayOneShot(houseKnock);}
        public void PlayRadio(){if(RadioPowered)StartCoroutine(TuneRadio());}
        System.Collections.IEnumerator TuneRadio(){radio.Stop();radio.PlayOneShot(radioTune,.65f);yield return new WaitForSeconds(2);radio.clip=radioStatic;radio.loop=true;radio.Play();}
        System.Collections.IEnumerator AnnouncementSound(){if(chime)pa.PlayOneShot(chime,.35f);yield return new WaitForSeconds(.8f);if(radioTune)pa.PlayOneShot(radioTune,.07f);}
        public void PlayArrival(){if(!saidArrival){saidArrival=true;StartCoroutine(AnnouncementSound());player.PresentSubtitle("[차내 안내] 잠시 후 밀밭 역에 도착합니다.\n내리실 문은 오른쪽입니다.",9);}}
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
                if(n.Contains("SEA / water"))return JourneySurface.Water;
                if(n.Contains("Carriage")||n.Contains("Dining")||n.Contains("Gangway")||n.Contains("oak boards")||n.Contains("carpet"))return JourneySurface.Carpet;
                if(n.Contains("Platform"))return JourneySurface.Concrete;
                if(n.Contains("House")||n.Contains("Porch")||n.Contains("Front /"))return JourneySurface.Wood;
            }
            return JourneySurface.Earth;
        }
        void Footstep()
        {
            lastSurface=Surface();var set=lastSurface switch{JourneySurface.Carpet=>carpetSteps,JourneySurface.Concrete=>concreteSteps,JourneySurface.Wood=>woodSteps,JourneySurface.Water=>waterSteps,_=>earthSteps};
            if(set==null||set.Length==0)return;
            int pick=Random.Range(0,set.Length);if(set.Length>1&&pick==lastVariant)pick=(pick+1)%set.Length;lastVariant=pick;
            feet.pitch=Random.Range(.97f,1.03f);feet.PlayOneShot(set[pick],lastSurface==JourneySurface.Carpet?.60f:.85f);footfalls++;
        }
        void Update()
        {
            if(!player||!train||player.paused)return;
            if(FieldMusicStarted){fieldAge+=Time.deltaTime;arrivalCue.volume=Mathf.SmoothStep(.005f,.34f,Mathf.Clamp01(fieldAge/4))*JourneyPreferences.Music;if(fieldAge>=2f&&!music.isPlaying)music.Play();}
            float threat=player.GetComponent<ScarecrowField>()?.Threat??0;
            float fracture=player.GetComponent<RealityTransit>()?.Blend??0;float danger=player.deadline&&player.stage<6?player.deadline.Danger:0;seaWater.volume=Mathf.Lerp(seaWater.volume,player.stage>=6?(Mathf.Abs(player.transform.position.x)<2.5f?.012f:.07f)*JourneyPreferences.Sfx:0,Time.deltaTime);music.volume=Mathf.Lerp(music.volume,FieldMusicStarted&&fieldAge>=2f&&!(player.deadline&&player.deadline.missedTrain)?Mathf.Lerp(.23f,.07f,danger)*JourneyPreferences.Music:0,Time.deltaTime);dread.volume=Mathf.Lerp(dread.volume,(danger*.12f+threat*.17f+(player.stage==5?fracture*.1f:0))*JourneyPreferences.Music,Time.deltaTime);
            if(train.departing){radio.volume=Mathf.MoveTowards(radio.volume,0,Time.deltaTime*.1f);clock.volume=Mathf.MoveTowards(clock.volume,0,Time.deltaTime*.1f);}
            subtitle.volume=.24f*JourneyPreferences.Sfx;feet.volume=.32f*JourneyPreferences.Sfx;coachDoor.volume=.3f*JourneyPreferences.Sfx;door.volume=.38f*JourneyPreferences.Sfx;pa.volume=.24f*JourneyPreferences.Sfx;mechanism.volume=.4f*JourneyPreferences.Sfx;knocking.volume=.4f*JourneyPreferences.Sfx;clock.volume=(train.departing?0:.22f)*JourneyPreferences.Sfx;radio.volume=(train.departing?0:.28f)*JourneyPreferences.Sfx;
            var p=player.transform.position;bool inside=Mathf.Abs(p.x)<2.5f&&Mathf.Abs(p.z)<28.5f;
            float speed=Mathf.Clamp01(train.speed/13),duck=DialoguePlaying?.48f:1;
            float proximity=inside?1:Mathf.Clamp01(1-new Vector2(Mathf.Max(0,Mathf.Abs(p.x)-3),Mathf.Max(0,Mathf.Abs(p.z)-30)).magnitude/22);
            wheels.volume=Mathf.Lerp(wheels.volume,speed*.15f*duck*proximity*JourneyPreferences.Sfx,Time.deltaTime*3);wheels.pitch=Mathf.Lerp(.80f,1,speed)*Mathf.Lerp(1,.68f,fracture);rollingFilter.cutoffFrequency=Mathf.Lerp(inside?1100:4500,320,fracture);
            joints.volume=(inside?.055f:.09f)*duck*proximity*JourneyPreferences.Sfx;cabin.volume=.025f*speed*duck*(inside?1:proximity*.25f)*JourneyPreferences.Sfx;
            outdoor.volume=Mathf.Lerp(outdoor.volume,(inside?.012f:.075f)*(1-threat*.55f)*JourneyPreferences.Sfx,Time.deltaTime*2);
            jointDistance+=train.speed*Time.deltaTime;
            if(jointDistance>12.5f){jointDistance-=12.5f;joints.pitch=Random.Range(.97f,1.03f);joints.PlayOneShot(railJoint);}
            if(speed>.3f&&Time.time>nextCreak){cabin.pitch=Random.Range(.9f,1.1f);cabin.PlayOneShot(creak);nextCreak=Time.time+Random.Range(18,30);}
            if(!braked&&train.elapsed>train.journeySeconds-2){braked=true;door.PlayOneShot(brake,.65f);}
            if(train.departing&&!saidDeparture){saidDeparture=true;StartCoroutine(AnnouncementSound());player.PresentSubtitle("[차내 안내] 문이 닫힙니다.\n열차가 다음 역으로 출발합니다.",8);}
            Vector3 delta=p-previousFoot;delta.y=0;previousFoot=p;
            if(!player.IsWaking&&delta.magnitude<.5f&&player.GetComponent<CharacterController>().isGrounded)distanceSinceStep+=delta.magnitude;
            if(distanceSinceStep>1.12f){distanceSinceStep-=1.12f;Footstep();}
        }
    }
}
