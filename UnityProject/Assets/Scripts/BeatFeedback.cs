using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatFeedback : MonoBehaviour
{

   [Header("Beat Anim Trigger")]
   public Animator OnBeatAnimator;
   public string   OnBeatTrigger = "";

   void Start()
   {
      if (MasterTimeline.I)
      {
         MasterTimeline.I.OnBeat.AddListener(_OnBeat);
      }
   }

   void _OnBeat()
   {
      //trigger animation every beat
      if (OnBeatAnimator && (OnBeatTrigger.Length > 0))
         OnBeatAnimator.SetTrigger(OnBeatTrigger);
   }

}
