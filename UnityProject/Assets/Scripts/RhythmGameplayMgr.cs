//
//  Translates "swing attempts" from player into "hits" or "misses", based on the midi authoring for the current songs
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class RhythmGameplayMgr : MonoBehaviour
{
   [Tooltip("How close to an actual authoring event do you need to be (in ms) to 'hit' it?")]
   public int SlopWindowMs = 200;

   [Header("Debug")]
   public bool EnableDebugSwingKey = true;
   public KeyCode DebugSwingKey = KeyCode.Space;

   //events
   public UnityEvent OnSwingHit = new UnityEvent();
   public UnityEvent OnSwingMiss = new UnityEvent();

   public static RhythmGameplayMgr I { get; private set; }

   void Awake()
   {
      I = this;   
   }

   List<Song.AuthoringEvent> _GetAuthoring()
   {
      if (MasterTimeline.I && MasterTimeline.I.SongToSyncWith)
         return MasterTimeline.I.SongToSyncWith.GetAuthoring();

      return null;
   }

   //call this when the player "swings" at a note
   public bool TriggerPlayerSwing(int drumIdx)
   {
      List<Song.AuthoringEvent> authoring = _GetAuthoring();
      if (authoring == null)
      {
         Debug.LogWarning("Can't process player swing because we dont have any authoring!");
         return false;
      }

      //find an authoring event that starts near the current time
      Song.AuthoringEvent hitEvent = null;
      float curSecs = MasterTimeline.I.CurSeconds;
      float kSlopSecs = SlopWindowMs * .001f;
      foreach(Song.AuthoringEvent e in authoring)
      {
         float eventStartSecs = MasterTimeline.I.BeatsToSeconds(e.NoteOnBeat);

         if (e.NoteIdx != drumIdx)
            continue;

         float diff = Mathf.Abs(eventStartSecs - curSecs);
         if(diff <= kSlopSecs)
         {
            hitEvent = e;
            break;
         }
      }

      if(hitEvent != null)
      {
         OnSwingHit.Invoke();
         Debug.Log("HIT!");

         return true;
      }
      else
      {
         OnSwingMiss.Invoke();
         Debug.Log("MISS!");

         return false;
      }
   }

   void Update()
   {
      if(EnableDebugSwingKey && Input.GetKeyDown(DebugSwingKey))
      {
         TriggerPlayerSwing(0);
      }
   }
}
