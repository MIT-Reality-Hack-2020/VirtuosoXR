//
//  Access time of the currently playing song
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MasterTimeline : MonoBehaviour
{
   public Song SongToSyncWith;

   [Header("Outputs")]
   public float CurSeconds = 0.0f;
   public float CurBeat = 0.0f;
   public float CurSongProgress = 0.0f;


   //events
   public UnityEvent OnBeat = new UnityEvent();

   public static MasterTimeline I { get; private set; }

   float _lastContentSecs;

   public float GetCurrentBeat()
   {
      return CurBeat;
   }

   public float GetSongProgress()
   {
      if (!SongToSyncWith)
         return 0.0f;

      float songLen = SongToSyncWith.GetLengthSeconds();
      return Mathf.Clamp01(_lastContentSecs / songLen);
   }

   public float GetCurrentSeconds()
   {
      return _lastContentSecs;
   }

   public float BeatsToSeconds(float beat)
   {
      if (SongToSyncWith)
         return SongToSyncWith.BeatsToSecs(beat);
      return 0.0f;
   }

   public float SecondsToBeats(float beat)
   {
      if (SongToSyncWith)
         return SongToSyncWith.SecsToBeats(beat);
      return 0.0f;
   }

   void Awake()
   {
      I = this;
   }


   void Start()
   {
        
   }

   void Update()
   {
      if (!SongToSyncWith)
         return;

      _lastContentSecs = SongToSyncWith.GetCurContentTime();

      float prevBeat = CurBeat;

      CurSeconds = _lastContentSecs;
      CurBeat = SecondsToBeats(CurSeconds);
      CurSongProgress = GetSongProgress();

      if((int)prevBeat != (int)CurBeat)
      {
         OnBeat.Invoke();
         //Debug.Log("BEAT!");
      }
   }
}
