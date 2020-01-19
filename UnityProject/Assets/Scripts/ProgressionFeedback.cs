//
//  Visual feedback based on current progression state
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionFeedback : MonoBehaviour
{

   public ProgressionEntry[] StateActivations = new ProgressionEntry[0];

   public enum ActivateionMode
   {
      WhenEnteringState,
      WhenExittingState
   }

   [System.Serializable]
   public class ProgressionEntry
   {
      [Header("Trigger Conditions")]
      [Tooltip("This is here just to give pretty headers to the array entries")]
      public string ProgressionName = "[unnamed]";
      public ActivateionMode TriggerCondition = ActivateionMode.WhenEnteringState;
      public ProgressionState ActivationState = ProgressionState.Start;
      [Tooltip("turn this on to filter out this entry unless its a particular song # in the progression")]
      public bool EnableRequiredSongIdx = false;
      public int RequiredSongIdx = 0;
      [Tooltip("If this is a state that supports multiple interstitial screens (like pregameplay messages), then you can turn this on to require a particular iterstitial number")]
      public bool EnableRequiredInterstitialIdx = false;
      public int RequiredInterstitialIdx = 0;

      [Header("Actions")]
      public GameObject[] EnableGameObjects = new GameObject[0];
      public GameObject[] DisableGameObjects = new GameObject[0];
      [Space(5)]
      public AudioSource AudioSourceToPlay = null;
      public AudioSource AudioSourceToStop = null;
   }

   void Start()
   {
      if (ProgressionMgr.I)
         ProgressionMgr.I.OnStateChanged.AddListener(_OnStateChanged);
   }

   void _OnStateChanged(ProgressionState fromState, ProgressionState toState)
   {
      foreach (var p in StateActivations)
      {
         //filter out this entry if we're not on its required song
         if (p.EnableRequiredSongIdx && (ProgressionMgr.I.GetCurSongIdx() != p.RequiredSongIdx))
            continue;

         //filter out this entry if we're not on its required interstitial
         if (p.EnableRequiredInterstitialIdx && (ProgressionMgr.I.GetCurIntertitialIdx() != p.RequiredInterstitialIdx))
            continue;

         if ((p.TriggerCondition == ActivateionMode.WhenEnteringState) && (p.ActivationState == toState))
            _Activate(p);
         else if ((p.TriggerCondition == ActivateionMode.WhenExittingState) && (p.ActivationState == fromState))
            _Activate(p);
      }
   }

   void _Activate(ProgressionEntry a)
   {
      foreach (var g in a.EnableGameObjects)
         g.SetActive(true);
      foreach (var g in a.DisableGameObjects)
         g.SetActive(false);

      if (a.AudioSourceToPlay)
         a.AudioSourceToPlay.Play();
      if (a.AudioSourceToStop)
         a.AudioSourceToStop.Stop();
   }
}
