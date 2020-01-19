using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumFeedback : MonoBehaviour
{
   [Header("Lookahead Feedback")]
   public float LookaheadBeats = 1.0f;
   public Animator LookaheadAnimator;
   public string LookaheadAnimatorState = "";
   public int LookaheadAnimatorLayer = 0;

   int _myDrumIdx = 0;
   Song.AuthoringEvent _myNextAuthEvent = null;
   Song.AuthoringEvent _prevAuthEvent = null;
   float _prevBeat = -1.0f;


   void Start()
   {

   }

   Song.AuthoringEvent _FindFirstAuthEventAfter(float beat, int drumIdx)
   {
      Song curSong = SongMgr.I.GetCurrentSong();
      if (!curSong)
         return null;

      var auth = curSong.GetAuthoring();
      if (auth.Count == 0)
         return null;

      Song.AuthoringEvent firstWrapAround = null;
      foreach (var ev in auth)
      {
         //wrap around first event
         if ((firstWrapAround == null) && (ev.NoteIdx == drumIdx))
         {
            firstWrapAround = new Song.AuthoringEvent();
            firstWrapAround.NoteIdx = ev.NoteIdx;
            firstWrapAround.NoteOnBeat = ev.NoteOnBeat + curSong.LengthInBeats;
         }

         if ((ev.NoteOnBeat > beat) && (ev.NoteIdx == drumIdx))
            return ev;
      }


      return firstWrapAround;
   }

   void Update()
   {
      _myDrumIdx = GetComponentInParent<CheckLocation>() ? GetComponentInParent<CheckLocation>().drumIdx : -1;
      if (_myDrumIdx < 0)
         return;

      float curBeat = SongMgr.I.GetCurrentSong() ? SongMgr.I.GetCurrentSong().SecsToBeats(SongMgr.I.GetCurrentSong().GetCurContentTime()) : 0.0f;

      if(curBeat < _prevBeat) //looped around
      {
         _prevAuthEvent = _myNextAuthEvent = null;
      }

      Song.AuthoringEvent newEvent = _FindFirstAuthEventAfter(curBeat, _myDrumIdx);
      float prevNoteOn = (_myNextAuthEvent != null) ? _myNextAuthEvent.NoteOnBeat : -1.0f;
      if((newEvent != null) && (newEvent.NoteOnBeat > prevNoteOn))
      {
         _prevAuthEvent = _myNextAuthEvent;
         _myNextAuthEvent = newEvent;
      }

      if(_myNextAuthEvent != null)
      {
         if (LookaheadAnimator && LookaheadAnimatorState.Length > 0)
         {
            float lookaheadStart = Mathf.Max((_prevAuthEvent != null) ? _prevAuthEvent.NoteOnBeat : 0.0f, _myNextAuthEvent.NoteOnBeat - LookaheadBeats);
            float lookaheadEnd = _myNextAuthEvent.NoteOnBeat;

            float u = Mathf.InverseLerp(lookaheadStart, lookaheadEnd, curBeat);

            //Debug.Log("curBeat: " + curBeat + " prevBeat: " + _prevBeat + " lookatStart " + lookaheadStart + " lookaheadEnd " + lookaheadEnd + " u: " + u);

            LookaheadAnimator.speed = 0.0f;
            LookaheadAnimator.Play(LookaheadAnimatorState, LookaheadAnimatorLayer, u);
         }
      }

      _prevBeat = curBeat;
   }
}
