using UnityEngine;
namespace Sanlaq {
public enum SoundCue {Catch,Correct,Wrong,Victory,Defeat,Water,Sprint,Click}
public class AudioFeedback:MonoBehaviour {
    public AudioClip[] clips=new AudioClip[8];AudioSource source;
    void Awake(){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.volume=.25f;
        for(int i=0;i<8;i++)if(!clips[i]){int n=4410;var samples=new float[n];float hz=new[]{440,880,180,660,140,330,220,550}[i];for(int j=0;j<n;j++)samples[j]=Mathf.Sin(j*hz*2*Mathf.PI/44100)*.2f*(1-j/(float)n);var clip=AudioClip.Create("Placeholder "+(SoundCue)i,n,1,44100,false);clip.SetData(samples,0);clips[i]=clip;}}
    public void Play(SoundCue cue,float volume=1){if(source&&clips!=null&&(int)cue<clips.Length&&clips[(int)cue])source.PlayOneShot(clips[(int)cue],volume);}
}
}
