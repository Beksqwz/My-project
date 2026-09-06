using UnityEngine;
namespace Sanlaq {
public class GameHud:MonoBehaviour {
    GUIStyle label,button;Texture2D vision;Font font;
    void Init(){if(label!=null)return;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label=new GUIStyle{font=font,alignment=TextAnchor.MiddleCenter,wordWrap=true};button=new GUIStyle(GUI.skin.button){font=font,fontSize=23,alignment=TextAnchor.MiddleCenter};button.normal.background=Texture2D.whiteTexture;button.hover.background=Texture2D.whiteTexture;button.active.background=Texture2D.whiteTexture;button.normal.textColor=Art.Navy;button.hover.textColor=Art.Navy;button.active.textColor=Art.Navy;
        vision=new Texture2D(256,256,TextureFormat.RGBA32,false);vision.wrapMode=TextureWrapMode.Clamp;
        for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(127.5f,127.5f))/127.5f;vision.SetPixel(x,y,new Color(Art.Navy.r,Art.Navy.g,Art.Navy.b,.94f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.73f,1,d))));}vision.Apply();
    }
    void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Text(Rect r,string text,int size=22,Color? color=null,TextAnchor align=TextAnchor.MiddleCenter){label.fontSize=size;label.normal.textColor=color??Art.Cream;label.alignment=align;GUI.Label(r,text,label);}
    bool Button(Rect r,string text){GUI.backgroundColor=Art.Gold;bool clicked=GUI.Button(r,text,button);GUI.backgroundColor=Color.white;return clicked;}
    void Panel(Rect r){Fill(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),Art.Gold);Fill(r,Art.Navy);}
    void Vision(GameManager g){if(g.human.role!=PlayerRole.Sokyrteke||g.state==MatchState.Result)return;
        var p=Camera.main.WorldToScreenPoint(g.human.transform.position+Vector3.up*.6f);float radius=g.visionRadius*Screen.height/(Camera.main.orthographicSize*2);float x=p.x-radius,y=Screen.height-p.y-radius,d=radius*2;Color dark=new Color(Art.Navy.r,Art.Navy.g,Art.Navy.b,.94f);
        Fill(new Rect(0,0,Screen.width,Mathf.Max(0,y)),dark);Fill(new Rect(0,y+d,Screen.width,Mathf.Max(0,Screen.height-y-d)),dark);
        Fill(new Rect(0,Mathf.Max(0,y),Mathf.Max(0,x),Mathf.Min(Screen.height,y+d)-Mathf.Max(0,y)),dark);
        Fill(new Rect(x+d,Mathf.Max(0,y),Mathf.Max(0,Screen.width-x-d),Mathf.Min(Screen.height,y+d)-Mathf.Max(0,y)),dark);
        GUI.DrawTexture(new Rect(x,y,d,d),vision);
    }
    void OnGUI(){var g=GameManager.Instance;if(!g||!g.human)return;Init();Vision(g);
        var safe=Screen.safeArea;float s=Mathf.Min(safe.width/1280f,safe.height/720f);float offsetX=safe.x+(safe.width-1280*s)/2,offsetY=Screen.height-safe.yMax+(safe.height-720*s)/2;var old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,Vector3.one*s);
        Panel(new Rect(24,22,310,76));Text(new Rect(40,28,276,24),"SAÑLAQ  /  "+(g.human.role==PlayerRole.Sokyrteke?"HUNTER":"RUNNER"),17,Art.Gold,TextAnchor.MiddleLeft);
        Text(new Rect(40,53,276,34),g.human.role==PlayerRole.Sokyrteke?"СОҚЫРТЕКЕ":"Stay free. Stay sharp.",23,null,TextAnchor.MiddleLeft);
        int seconds=Mathf.CeilToInt(g.remaining);Panel(new Rect(563,22,154,76));Text(new Rect(568,25,144,19),"ROUND TIME",12,Art.Gold);Text(new Rect(570,44,140,46),$"{seconds/60:00}:{seconds%60:00}",34);
        Panel(new Rect(1010,22,246,76));Text(new Rect(1022,30,222,22),"RUNNERS REMAINING",13,Art.Gold);Text(new Rect(1022,52,222,36),g.RunnersRemaining+" / 3",27);
        string status=g.human.eliminated?"ELIMINATED · Watching the hunter":g.human.Slowed?"SLOWED · Catch disabled":g.human.Hiding?"HIDDEN · Yurt shelter":g.human.InWater?"WATER · Movement reduced":"";
        if(status!=""){Panel(new Rect(420,647,440,44));Text(new Rect(425,650,430,38),status,19,Art.Gold);}
        foreach(var p in g.players){if(p.eliminated)continue;if(g.human.role==PlayerRole.Sokyrteke&&p.role==PlayerRole.Runner&&(p.Hiding||Vector2.Distance(p.transform.position,g.human.transform.position)>g.visionRadius))continue;
            var v=Camera.main.WorldToScreenPoint(p.transform.position+Vector3.up*2.22f);float xx=(v.x-offsetX)/s,yy=(Screen.height-v.y-offsetY)/s;
            if(yy>108) {Fill(new Rect(xx-43,yy-10,86,22),Art.Navy);Text(new Rect(xx-43,yy-10,86,22),p.displayName,12,p.human?Art.Gold:Art.Cream);}}
        if(g.state==MatchState.Reveal){Panel(new Rect(335,224,610,298));Text(new Rect(355,240,570,35),"YOUR ROLE",16,Art.Gold);Text(new Rect(350,278,580,60),g.human.role==PlayerRole.Sokyrteke?"СОҚЫРТЕКЕ":"RUNNER",43);Text(new Rect(365,350,550,55),g.human.role==PlayerRole.Sokyrteke?"Catch all runners. Identify their clothing.":"Survive until time runs out. Use the yurt to hide.",23);Text(new Rect(365,437,550,48),"WASD / ARROWS to move     SHIFT to sprint",18,Art.Gold);}
        if(g.state==MatchState.Countdown){Text(new Rect(490,240,300,160),Mathf.CeilToInt(g.phaseTime).ToString(),110,Art.Navy);Text(new Rect(430,416,420,40),"GET READY",24,Art.Navy);}
        if(g.state==MatchState.Playing&&g.phaseTime>0)Text(new Rect(480,260,320,100),"GO!",80,Art.Gold);
        if(g.state==MatchState.Quiz)DrawQuiz(g);
        if(g.state==MatchState.Feedback){Panel(new Rect(335,274,610,154));Text(new Rect(355,285,570,52),g.feedbackCorrect?"CORRECT":"WRONG ANSWER",31,g.feedbackCorrect?Art.Gold:new Color(1,.55f,.45f));Text(new Rect(355,347,570,54),g.feedback,23);}
        if(g.state==MatchState.Result)DrawResult(g);
        GUI.matrix=old;
    }
    void DrawQuiz(GameManager g){var q=g.quiz;Fill(new Rect(0,0,1280,720),new Color(.04f,.07f,.12f,.66f));Panel(new Rect(330,120,620,510));
        Text(new Rect(350,139,580,30),"CAUGHT · "+g.captured.displayName,17,Art.Gold);
        Text(new Rect(365,183,550,65),"Identify the "+q.answer.slot.ToString().ToLower()+" item",30);
        Fill(new Rect(574,264,132,105),Art.Cream);if(q.answer.icon)GUI.DrawTexture(new Rect(590,270,100,90),q.answer.icon.texture,ScaleMode.ScaleToFit);
        Text(new Rect(710,280,170,60),Mathf.CeilToInt(q.timeLeft)+"s",32,Art.Gold);
        Fill(new Rect(365,383,550,5),new Color(.25f,.3f,.36f));Fill(new Rect(365,383,550*Mathf.Clamp01(q.timeLeft/7),5),Art.Gold);
        if(g.hunter.human){for(int i=0;i<3;i++)if(Button(new Rect(365,406+i*64,550,53),q.options[i]))q.Choose(i);}
        else{Text(new Rect(365,413,550,66),"The hunter is identifying the clothing…",25);Text(new Rect(365,505,550,70),g.captured.human?"You are caught. A wrong answer sets you free.":"Other runners can keep moving.",20,Art.Gold);}
    }
    void DrawResult(GameManager g){Fill(new Rect(0,0,1280,720),new Color(.04f,.07f,.12f,.75f));Panel(new Rect(310,142,660,460));
        bool win=g.human.role==PlayerRole.Sokyrteke?g.hunterWon:!g.hunterWon&&!g.human.eliminated;
        Text(new Rect(340,168,600,32),"MATCH COMPLETE",16,Art.Gold);Text(new Rect(340,208,600,68),win?"YOU WIN":"YOU LOSE",48);
        Text(new Rect(340,295,600,65),g.hunterWon?"Sokyrteke caught every runner.":"Time is up. The runners survive.",25,Art.Gold);
        if(Button(new Rect(365,389,550,59),"RESTART · RANDOM ROLE"))g.Restart();
        if(Button(new Rect(365,471,262,57),"PLAY HUNTER"))g.Restart(0);
        if(Button(new Rect(653,471,262,57),"PLAY RUNNER"))g.Restart(1);
    }
}
}
