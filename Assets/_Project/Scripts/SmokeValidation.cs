using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Sanlaq {
// Only activated by an explicit editor command or -sanlaqSmoke standalone argument.
public class SmokeValidation:MonoBehaviour {
    readonly List<string> report=new List<string>();int failures;
    InputSettings.BackgroundBehavior previousBackground;
#if UNITY_EDITOR
    InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
#endif
    void Awake(){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Logs"));previousBackground=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
        previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
    }
    void OnDestroy(){Time.timeScale=1;InputSystem.settings.backgroundBehavior=previousBackground;
#if UNITY_EDITOR
        InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;
#endif
    }
    void Check(bool ok,string name){report.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;Debug.Log(report.Last());}
    IEnumerator Start(){
        var g=GameManager.Instance;g.testing=true;g.Restart(0);yield return null;
        Check(g.players.Count==4&&g.players.Count(p=>p.role==PlayerRole.Sokyrteke)==1,"four players / exactly one hunter");
        Check(g.wardrobe.Length>=10&&g.players.All(p=>p.outfit.All(x=>x)),"clothing assets assigned");
        Check(g.state==MatchState.Reveal,"role reveal");g.phaseTime=.01f;yield return new WaitForSeconds(.05f);Check(g.state==MatchState.Countdown,"countdown");g.phaseTime=.01f;yield return new WaitForSeconds(.05f);Check(g.state==MatchState.Playing,"countdown enters match");
        g.human.Body.position=new Vector2(0,-4);g.human.input=Vector2.right;var start=g.human.Body.position;yield return new WaitForSeconds(.3f);float normal=Vector2.Distance(start,g.human.Body.position);Check(normal>1&&normal<1.8f,"Rigidbody movement at normal speed");
        g.human.sprint=true;start=g.human.Body.position;yield return new WaitForSeconds(.3f);Check(Vector2.Distance(start,g.human.Body.position)>normal*1.15f,"sprint increases movement");g.human.input=Vector2.zero;g.human.sprint=false;
        g.human.Body.position=Arena.Water.center;yield return new WaitForFixedUpdate();Check(g.human.InWater&&Mathf.Abs(g.human.Speed-2.925f)<.01f,"water multiplier");
        g.human.Body.position=new Vector2(12,-5);g.human.input=Vector2.right;yield return new WaitForSeconds(.5f);Check(g.human.Body.position.x<13,"boundary collider blocks movement");g.human.input=Vector2.zero;
        g.human.Body.position=new Vector2(-5,2.4f);g.human.input=Vector2.right;yield return new WaitForSeconds(.5f);Check(g.human.Body.position.x<-4.1f,"rock collider matches visible blocker");g.human.input=Vector2.zero;
        g.human.Body.position=new Vector2(0,7.5f);yield return new WaitForFixedUpdate();yield return new WaitForEndOfFrame();Camera.main.GetComponent<FollowCamera>().Snap();Check(Camera.main.WorldToScreenPoint(g.human.transform.position+Vector3.up*2.2f).y<Screen.height,"north boundary preserves character headroom");
        var runner=g.players[1];runner.Body.position=Arena.Hide.center;yield return new WaitForFixedUpdate();Check(runner.Hiding,"yurt hiding detection");
        g.hunter.Body.position=new Vector2(0,-4);runner.Body.position=new Vector2(.6f,-4);yield return new WaitForFixedUpdate();
        Check(g.TryCatch()&&g.state==MatchState.Quiz,"catch opens quiz");Check(!g.TryCatch(),"duplicate catch rejected");Check(g.hunter.frozen&&runner.frozen&&!g.players[2].frozen,"only caught pair freezes");Check(g.quiz.options.Distinct().Count()==3,"three distinct quiz answers");
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../Logs/sanlaq-quiz.png"));yield return new WaitForEndOfFrame();
        g.quiz.Choose((g.quiz.correctIndex+1)%3);Check(g.hunter.Slowed&&!runner.eliminated,"wrong answer grants escape and slow");Check(!g.TryCatch(),"catch blocked during penalty");yield return new WaitForSeconds(1.2f);Check(!g.hunter.frozen&&!runner.frozen,"wrong answer resumes actors");
        g.hunter.slowUntil=0;runner.immuneUntil=0;Check(g.TryCatch(),"catch available after penalty");g.quiz.timeLeft=.001f;yield return new WaitForSeconds(.05f);Check(g.hunter.Slowed&&!runner.eliminated,"quiz timeout counts as wrong");yield return new WaitForSeconds(1.2f);
        g.hunter.slowUntil=0;runner.immuneUntil=0;g.TryCatch();g.quiz.Choose(g.quiz.correctIndex);Check(runner.eliminated&&!runner.GetComponent<Collider2D>().enabled,"correct answer eliminates and disables collision");yield return new WaitForSeconds(1.2f);
        Check(g.RunnersRemaining==2,"active runner count");
        foreach(var r in g.players.Where(p=>p.role==PlayerRole.Runner&&!p.eliminated).ToArray()){r.Body.position=new Vector2(.6f,-4);yield return new WaitForFixedUpdate();g.hunter.slowUntil=0;r.immuneUntil=0;g.TryCatch();g.quiz.Choose(g.quiz.correctIndex);yield return new WaitForSeconds(1.2f);}
        Check(g.state==MatchState.Result&&g.hunterWon,"hunter victory");float time=g.remaining;yield return new WaitForSeconds(.1f);Check(Mathf.Approximately(time,g.remaining)&&g.human.Body.linearVelocity==Vector2.zero,"result stops timer and movement");
        g.Restart(1);yield return null;Check(g.RunnersRemaining==3&&!g.human.eliminated&&g.remaining==90&&g.human.role==PlayerRole.Runner,"restart resets every state and runner role");g.state=MatchState.Playing;g.remaining=.02f;yield return new WaitForSeconds(.1f);Check(g.state==MatchState.Result&&!g.hunterWon,"runner victory / hunter defeat on timeout");
        g.Restart(1);yield return null;g.state=MatchState.Playing;g.human.Eliminate();Check(g.human.eliminated&&g.IsLive,"human elimination keeps match running");
        g.Restart(1);yield return null;g.state=MatchState.Playing;g.testing=false;var botStart=g.hunter.Body.position;yield return new WaitForSeconds(1);Check(Vector2.Distance(botStart,g.hunter.Body.position)>.3f,"hunter bot moves toward runner");Check(g.players.Where(p=>!p.human&&p.role==PlayerRole.Runner).Any(p=>p.Body.linearVelocity.sqrMagnitude>.1f),"runner bots move");
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../Logs/sanlaq-gameplay.png"));yield return new WaitForEndOfFrame();
        g.testing=true;g.Restart(0);yield return null;g.state=MatchState.Playing;yield return new WaitForSeconds(.15f);ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../Logs/sanlaq-vision.png"));yield return new WaitForEndOfFrame();
        Check(Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length==1,"single manager");Check(g.players.All(p=>Mathf.Abs(p.transform.position.x)<13&&Mathf.Abs(p.transform.position.y)<8),"all spawns inside arena");
        var hidden=g.players[1];hidden.Body.position=new Vector2(-10,6);yield return new WaitForEndOfFrame();Check(hidden.visual.GetComponentsInChildren<SpriteRenderer>().All(r=>!r.enabled),"runner outside hunter vision is invisible");
        hidden.Body.position=new Vector2(.5f,-3);yield return new WaitForFixedUpdate();yield return new WaitForEndOfFrame();Check(hidden.visual.GetComponentsInChildren<SpriteRenderer>().All(r=>r.enabled),"nearby runner visible");
        hidden.Body.position=Arena.Hide.center;g.human.Body.position=Arena.Hide.center+Vector2.left*.6f;yield return new WaitForFixedUpdate();yield return new WaitForEndOfFrame();Check(hidden.visual.GetComponentsInChildren<SpriteRenderer>().All(r=>!r.enabled),"yurt conceals nearby runner");Check(g.TryCatch(),"hidden runner can still be caught on contact");
        g.Restart(1);yield return null;g.state=MatchState.Playing;g.human.Body.position=new Vector2(0,-6);g.testing=false;
        var testKeyboard=InputSystem.AddDevice<Keyboard>();testKeyboard.MakeCurrent();start=g.human.Body.position;InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.D));yield return new WaitForSeconds(.25f);InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());Check(g.human.Body.position.x-start.x>.5f,"Input System keyboard reaches movement (distance="+(g.human.Body.position.x-start.x).ToString("F2")+")");InputSystem.RemoveDevice(testKeyboard);
        // A natural complete match: no forced catch or quiz outcomes, human stands still.
        g.Restart(1);yield return null;g.state=MatchState.Playing;Time.timeScale=4;float deadline=Time.realtimeSinceStartup+30;
        while(g.state!=MatchState.Result&&Time.realtimeSinceStartup<deadline)yield return null;
        Time.timeScale=1;Check(g.state==MatchState.Result,"complete natural bot-driven match reaches result");Check(g.catchesThisRound>0,"bot naturally catches and resolves quiz");
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../Logs/sanlaq-result.png"));yield return new WaitForEndOfFrame();
        g.testing=true;g.Restart(0);yield return null;g.state=MatchState.Playing;foreach(var r in g.players.Where(p=>p.role==PlayerRole.Runner))r.Eliminate();g.remaining=.001f;yield return new WaitForSeconds(.05f);Check(g.hunterWon,"last elimination takes precedence over timer expiry");
        report.Add("FAILURES="+failures);Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Logs"));File.WriteAllLines(Path.Combine(Application.dataPath,"../Logs/sanlaq-validation.txt"),report);g.testing=false;g.Restart();Destroy(this);
    }
}
}
