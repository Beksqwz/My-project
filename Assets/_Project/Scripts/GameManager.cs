using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Sanlaq {
public enum MatchState { Reveal, Countdown, Playing, Quiz, Feedback, Result }
public class GameManager:MonoBehaviour {
    public static GameManager Instance {get;private set;}
    public PlayerController playerPrefab;
    public ClothingItem[] wardrobe;
    public List<PlayerController> players=new List<PlayerController>();
    public PlayerController human,hunter,captured;
    public MatchState state;
    public float remaining=90,phaseTime,visionRadius=4.6f;
    public bool hunterWon,testing;
    public int catchesThisRound;
    public string feedback="";
    public bool feedbackCorrect;
    public AudioFeedback audioFx;
    public QuizManager quiz;
    public bool IsLive=>state==MatchState.Playing||state==MatchState.Quiz||state==MatchState.Feedback;
    public int RunnersRemaining=>players.Count(p=>p.role==PlayerRole.Runner&&!p.eliminated);
    void Awake(){if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;Application.targetFrameRate=60;audioFx=gameObject.AddComponent<AudioFeedback>();quiz=gameObject.AddComponent<QuizManager>();gameObject.AddComponent<GameHud>();}
    void Start(){
        if(!FindFirstObjectByType<Arena>()){var a=new GameObject("Steppe arena").AddComponent<Arena>();a.Build();}
        if(!Camera.main){var c=new GameObject("Main Camera").AddComponent<Camera>();c.tag="MainCamera";c.orthographic=true;c.transform.position=new Vector3(0,0,-10);c.gameObject.AddComponent<AudioListener>();}
        Camera.main.orthographic=true;Camera.main.orthographicSize=6.4f;Camera.main.backgroundColor=Art.Navy;
        if(!Camera.main.GetComponent<FollowCamera>())Camera.main.gameObject.AddComponent<FollowCamera>();
        Restart(-1);
        bool smokeRequested=System.IO.File.Exists("Temp/sanlaq-smoke");
        if(System.Environment.GetCommandLineArgs().Contains("-sanlaqSmoke")||smokeRequested){if(smokeRequested)System.IO.File.Delete("Temp/sanlaq-smoke");gameObject.AddComponent<SmokeValidation>();}
    }
    public void Restart(int humanRole=-1){
        StopAllCoroutines();quiz.Cancel();captured=null;state=MatchState.Reveal;
        foreach(var p in players)if(p){p.gameObject.SetActive(false);Destroy(p.gameObject);}players.Clear();
        remaining=90;phaseTime=3;feedback="";hunterWon=false;catchesThisRound=0;
        int hunterIndex=humanRole==0?0:humanRole==1?Random.Range(1,4):Random.Range(0,4);
        Vector2[] spawns={new Vector2(0,-3),new Vector2(-7,1),new Vector2(6,-4),new Vector2(2,3)};
        for(int i=0;i<4;i++){
            var p=Instantiate(playerPrefab,spawns[i],Quaternion.identity);p.name=i==0?"You":"Runner "+i;p.human=i==0;p.displayName=i==0?"YOU":new[]{"","AIYA","ARLAN","DANA"}[i];p.role=i==hunterIndex?PlayerRole.Sokyrteke:PlayerRole.Runner;
            for(int s=0;s<4;s++){var candidates=wardrobe.Where(x=>x.slot==(ClothingSlot)s).ToArray();p.outfit[s]=candidates[Random.Range(0,candidates.Length)];}
            var visual=new GameObject("Visual");visual.transform.SetParent(p.transform,false);p.visual=visual.AddComponent<PlayerVisual>();p.visual.Build(p);
            players.Add(p);if(p.human)human=p;if(p.role==PlayerRole.Sokyrteke)hunter=p;
        }
        foreach(var a in players)foreach(var b in players)if(a!=b)Physics2D.IgnoreCollision(a.GetComponent<Collider2D>(),b.GetComponent<Collider2D>());
        Camera.main.GetComponent<FollowCamera>().Snap();
    }
    void Update(){
        if(state==MatchState.Result)return;
        if(state==MatchState.Reveal||state==MatchState.Countdown){phaseTime-=Time.deltaTime;if(phaseTime<=0){if(state==MatchState.Reveal){state=MatchState.Countdown;phaseTime=3;}else{state=MatchState.Playing;feedback="GO!";phaseTime=.8f;audioFx.Play(SoundCue.Click);}}return;}
        remaining=Mathf.Max(0,remaining-Time.deltaTime);
        if(remaining<=0){Finish(RunnersRemaining==0);return;}
        if(state==MatchState.Feedback){phaseTime-=Time.deltaTime;if(phaseTime<=0){Release();if(RunnersRemaining==0)Finish(true);else state=MatchState.Playing;}}
        if(state==MatchState.Playing&&!testing)TryCatch();
        if(state==MatchState.Playing)phaseTime=Mathf.Max(0,phaseTime-Time.deltaTime);
    }
    public bool TryCatch(){
        if(state!=MatchState.Playing||hunter.Slowed||Time.time<hunter.immuneUntil)return false;
        foreach(var r in players){if(r.role!=PlayerRole.Runner||r.eliminated||Time.time<r.immuneUntil)continue;
            Vector2 delta=r.transform.position-hunter.transform.position;
            if(delta.magnitude>.9f||Physics2D.Linecast(hunter.transform.position,r.transform.position,1<<8))continue;
            captured=r;catchesThisRound++;hunter.frozen=true;r.frozen=true;hunter.Body.linearVelocity=Vector2.zero;r.Body.linearVelocity=Vector2.zero;state=MatchState.Quiz;quiz.Begin(r);hunter.visual.Punch();r.visual.Punch();Art.Burst(r.transform.position,Art.Gold);audioFx.Play(SoundCue.Catch);return true;
        }return false;
    }
    public void ResolveQuiz(bool correct){
        if(state!=MatchState.Quiz||!captured)return;quiz.Cancel();feedbackCorrect=correct;
        if(correct){captured.Eliminate();feedback=captured.displayName+" ELIMINATED";audioFx.Play(SoundCue.Correct);}
        else{hunter.slowUntil=Time.time+3;captured.immuneUntil=Time.time+4;feedback="ESCAPED!  Hunter slowed · 3s";audioFx.Play(SoundCue.Wrong);Art.Burst(hunter.transform.position,new Color(.9f,.35f,.3f));Release();}
        state=MatchState.Feedback;phaseTime=1.1f;
    }
    void Release(){if(hunter)hunter.frozen=false;if(captured){captured.frozen=false;captured=null;}}
    public void Finish(bool won){hunterWon=won;state=MatchState.Result;quiz.Cancel();Release();foreach(var p in players){p.input=Vector2.zero;p.Body.linearVelocity=Vector2.zero;}audioFx.Play((human.role==PlayerRole.Sokyrteke?won:!won&&!human.eliminated)?SoundCue.Victory:SoundCue.Defeat);}
    void OnDestroy(){if(Instance==this)Instance=null;}
}
public class FollowCamera:MonoBehaviour {
    public void Snap(){if(GameManager.Instance&&GameManager.Instance.human)transform.position=Target();}
    Vector3 Target(){var g=GameManager.Instance;var actor=g.human.eliminated?g.hunter:g.human;float height=GetComponent<Camera>().orthographicSize;float width=height*GetComponent<Camera>().aspect;var p=actor.transform.position+Vector3.up*.7f;
        // Clamp to the authored border, including headroom for the elevated sprites and HUD.
        float horizontal=Mathf.Max(0,Arena.HalfWidth+.6f-width);
        float bottom=-Arena.HalfHeight-1.2f+height,top=Arena.HalfHeight+2.8f-height;
        return new Vector3(Mathf.Clamp(p.x,-horizontal,horizontal),Mathf.Clamp(p.y,Mathf.Min(bottom,top),Mathf.Max(bottom,top)),-10);}
    void LateUpdate(){if(GameManager.Instance&&GameManager.Instance.human)transform.position=Vector3.Lerp(transform.position,Target(),1-Mathf.Exp(-12*Time.deltaTime));}
}
}
