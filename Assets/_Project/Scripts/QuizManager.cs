using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Sanlaq {
public class QuizManager:MonoBehaviour {
    public ClothingItem answer;
    public string[] options=new string[3];
    public int correctIndex;
    public float timeLeft;
    public bool active;
    float botDelay;
    public void Begin(PlayerController runner){
        answer=runner.outfit[Random.Range(0,4)];
        var other=GameManager.Instance.wardrobe.Where(x=>x.slot==answer.slot&&x!=answer).OrderBy(x=>Random.value).Select(x=>x.displayName).Take(2).ToList();
        // Every slot has three assets, so all distractors belong to the asked slot.
        while(other.Count<2)other.Add("None of these");
        correctIndex=Random.Range(0,3);int j=0;for(int i=0;i<3;i++)options[i]=i==correctIndex?answer.displayName:other[j++];
        timeLeft=7;botDelay=Random.Range(2.2f,4.2f);active=true;
    }
    void Update(){if(!active||GameManager.Instance.state!=MatchState.Quiz)return;timeLeft-=Time.deltaTime;
        if(timeLeft<=0){GameManager.Instance.ResolveQuiz(false);return;}
        if(!GameManager.Instance.hunter.human&&!GameManager.Instance.testing){botDelay-=Time.deltaTime;if(botDelay<=0)GameManager.Instance.ResolveQuiz(Random.value<.72f);}
    }
    public void Choose(int index){if(active&&GameManager.Instance.hunter.human){GameManager.Instance.audioFx.Play(SoundCue.Click);GameManager.Instance.ResolveQuiz(index==correctIndex);}}
    public void Cancel(){active=false;}
}
}
