//
// Manage main progression states of the app
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ProgressionStateChangedEvent : UnityEvent<ProgressionState, ProgressionState> //from state, to state
{
}

public enum ProgressionState
{
   Start,
   FindASurfaceMessage,
   PlaceDrums, //show msg and wait a tick for spawning drums
   PreGameplayIntertitial, //one or more screens of content
   Gameplay,
   Results, //interstitial text
   None
}


public class ProgressionMgr : MonoBehaviour
{
   [Header("Config")]
   public GameObject DrumPrefab;
   public TubsMusicUI MusicUI;
   public Vector3 MusicUIOffset = Vector3.zero;
   public Song[] SongSequence = new Song[0];
   public MagicLeapTools.PushButton NextStateButton;
   [Tooltip("Wait this long before you can hit button again")]
   public float NextStateCooldownSecs = 2.0f;
   //public Vector3 NextStateButtonOffsetDuringGameplay = Vector3.zero; //fix the next button in space
   [Tooltip("When we spawn the drums, we want to offset them appear. ")]
   public Vector3[] DrumWorldOffsets = new Vector3[0];
   [Tooltip("When we spawn the drums, we want to offset them appear.  these are the offsets sent to the KeepInFront script")]
   public Vector3[] DrumKeepInFrontOffsets = new Vector3[0];
   public bool MoveDrumsWithHeadDuringPlacement = true;
   public Vector3 ButtonOffsetWhenDockedToDrums = new Vector3(0.0f, .1f, 0.0f);

   [Header("Debug")]
   public bool UseDebugDrumLocations;
   public Transform[] DebugDrumLocations = new Transform[0];
   public KeyCode DebugNextProgressionTrigger = KeyCode.Space;

   //events
   public ProgressionStateChangedEvent OnStateChanged = new ProgressionStateChangedEvent();


   public static ProgressionMgr I { get; private set; }

   List<GameObject> _spawnedDrums = new List<GameObject>();
   ProgressionState _curState = ProgressionState.None;
   int _curSongIdx = 0;
   int _interstitialIdx = 0;

   Vector3 _nextButtonLocalPos = Vector3.zero;
   Quaternion _nextButtonLocalRot = Quaternion.identity;
   Transform _nextButtonParent = null;
   float _lastNextButtonPressTime = -1.0f;

   void Awake()
   {
      I = this;
   }

   void Start()
   {
      //just force our sequence into the song list
      SongMgr.I.SongList = SongSequence;

      if (NextStateButton)
      {
         NextStateButton.events.OnPressed.AddListener(_OnNextStatePressed);

         _nextButtonLocalPos = NextStateButton.transform.localPosition;
         _nextButtonLocalRot = NextStateButton.transform.localRotation;
         _nextButtonParent = NextStateButton.transform.parent;
      }
      _curSongIdx = 0;
      _SetState(ProgressionState.Start, true);
   }

   void _DetachNextStateButton(Vector3 newPos)
   {
      if (!NextStateButton)
         return;

      NextStateButton.transform.SetParent(null, true);
      NextStateButton.transform.position = newPos;
   }

   void _ReAttachNextStateButton()
   {
      if (!NextStateButton)
         return;

      NextStateButton.transform.SetParent(_nextButtonParent, true);
      NextStateButton.transform.localPosition = _nextButtonLocalPos;
      NextStateButton.transform.localRotation = _nextButtonLocalRot;
   }

   void _OnNextStatePressed()
   {
      //ignore if button press comes in too fast
      if ((Time.time - _lastNextButtonPressTime) < NextStateCooldownSecs)
         return;

      _GotoNextState();

      _lastNextButtonPressTime = Time.time;
   }

   void _GotoNextState()
   {
      switch (_curState)
      {
         case ProgressionState.Start:
            _SetState(ProgressionState.FindASurfaceMessage);
            break;
         case ProgressionState.FindASurfaceMessage:
            _SetState(ProgressionState.PlaceDrums);
            break;
         case ProgressionState.PlaceDrums:
            _interstitialIdx = 0;
            _SetState(ProgressionState.PreGameplayIntertitial);
            break;
         case ProgressionState.PreGameplayIntertitial:
            _interstitialIdx++;
            if (_interstitialIdx >= GetCurSong().NumPregameplayIntertitials)
               _SetState(ProgressionState.Gameplay);
            else
               _SetState(ProgressionState.PreGameplayIntertitial, true); //force retrigger transition into this state for new initierial idx
            break;
         case ProgressionState.Gameplay:
            _SetState(ProgressionState.Results);
            break;
         case ProgressionState.Results:
            _curSongIdx++;
            _interstitialIdx = 0;

            if (_curSongIdx >= SongSequence.Length) //done with song sequence? then just restart to initial state
            {
               _curSongIdx = 0;
               _SetState(ProgressionState.Start);
            }
            else
            {
               SongMgr.I.PrepareSong(SongMgr.I.GetSong(_curSongIdx));
               _SetState(ProgressionState.PreGameplayIntertitial);
            }
            break;
      }
   }

   void Update()
   {
      //debug key to advance progression
      if (Input.GetKeyDown(DebugNextProgressionTrigger))
      {
         _GotoNextState();
      }

      if(GetCurState() == ProgressionState.PlaceDrums)
      {
         if(MoveDrumsWithHeadDuringPlacement)
         {
            for(int i = 0; i < _spawnedDrums.Count; i++)
            {
               Vector3 offset = DrumWorldOffsets[i];
               _PlaceObjectInFrontOfPlayer(_spawnedDrums[i].transform, offset);
            }
         }
      }
   }

   public ProgressionState GetCurState()
   {
      return _curState;
   }

   public int GetCurSongIdx()
   {
      return _curSongIdx;
   }

   public int GetCurIntertitialIdx()
   {
      return _interstitialIdx;
   }

   public Song GetCurSong()
   {
      return SongSequence[GetCurSongIdx()];
   }

   void _DestroyDrums()
   {
      foreach (GameObject g in _spawnedDrums)
         Destroy(g);

      _spawnedDrums.Clear();
   }

   void _ShowMusicUI(bool b)
   {
      if (MusicUI)
      {
         MusicUI.gameObject.SetActive(b);

         //place ui in front of player when we show it
         if(b)
            _PlaceObjectInFrontOfPlayer(MusicUI.transform, MusicUIOffset);
      }
   }

   int _GetMaxDrumPads()
   {
      int maxDrumPads = 0;
      foreach(Song s in SongSequence)
      {
         s.Prepare();
         int numPads = s.GetNumInstrumentPads();
         if (numPads > maxDrumPads)
            maxDrumPads = numPads;
      }

      return maxDrumPads;
   }

   void _EnableDrumHeadCount(int cnt)
   {
      for(int i = 0; i < _spawnedDrums.Count; i++)
      {
         _spawnedDrums[i].SetActive(i < cnt);
      }
   }

   void _PlaceObjectInFrontOfPlayer(Transform obj, Vector3 offset)
   {
      /*Camera mainCam = Camera.main;
      Vector3 offsetPoint = mainCam.transform.TransformPoint(offset);
      Vector3 castVector = Vector3.Normalize(offsetPoint - mainCam.transform.position);
      const bool kFlatLocation = true;
      if (kFlatLocation)
      {
         castVector = Vector3.ProjectOnPlane(castVector, Vector3.up).normalized;
      }

      obj.position = castVector * offset.magnitude;*/

      Camera mainCam = Camera.main;
      obj.position = mainCam.transform.TransformPoint(offset);
      Vector3 to = Vector3.Normalize(transform.position - mainCam.transform.position);
      //if (flipForward) to *= -1;
      if (mainCam)
      {
         to = Vector3.ProjectOnPlane(to, Vector3.up).normalized;
         obj.rotation = Quaternion.LookRotation(to);
      }
      else
      {
         obj.rotation = Quaternion.LookRotation(to);
      }
   }

   void _SetState(ProgressionState newState, bool force = false)
   {
      if ((_curState == newState) && !force)
         return;

      ProgressionState prevState = _curState;

      _curState = newState;


      //after gameplay, next state button is back moving with UI
      if(prevState == ProgressionState.Gameplay)
      {
         _ReAttachNextStateButton();
      }


      if (newState == ProgressionState.Start)
      {
         _ShowMusicUI(false);
         _DestroyDrums();
      }
      else if (newState == ProgressionState.PlaceDrums)
      {

         //spawn in drums
         //prepare song
         SongMgr.I.PrepareSong(SongMgr.I.GetSong(_curSongIdx));
         _DestroyDrums();
         int numDrums = _GetMaxDrumPads();
         for (int i = 0; i < numDrums; i++)
         {
            GameObject spawnedDrum = Instantiate(DrumPrefab);
            _spawnedDrums.Add(spawnedDrum);

            CheckLocation loc = spawnedDrum.GetComponent<CheckLocation>();
            if (loc)
            {
               loc.drumIdx = i;
               loc.EnableDebugPlayerSwipeKey = true;
               loc.DebugPlayerSwipeKey = KeyCode.Alpha1 + i;
            }

            if (UseDebugDrumLocations && (i < DebugDrumLocations.Length))
            {
               spawnedDrum.transform.position = DebugDrumLocations[i].position;
            }
            else if((DrumWorldOffsets.Length > 0) && (i < DrumWorldOffsets.Length)) //just plop in front of camera
            {
               Vector3 offset = DrumWorldOffsets[i];
               _PlaceObjectInFrontOfPlayer(spawnedDrum.transform, offset);
            }

            MagicLeapTools.KeepInFront keepInFront = spawnedDrum.GetComponent<MagicLeapTools.KeepInFront>();
            if(keepInFront && (i < DrumKeepInFrontOffsets.Length))
            {
               keepInFront.offset = DrumKeepInFrontOffsets[i];
            }
         }
      }
      else if(newState == ProgressionState.PreGameplayIntertitial)
      {
         if(_interstitialIdx == 0) //reduce to number of drum pads for THIS song
         {
            _EnableDrumHeadCount(GetCurSong().GetNumInstrumentPads());
         }
      }
      else if (newState == ProgressionState.Gameplay)
      {
         _ShowMusicUI(true);

         //move next state button between the drums current gameplay
         _DetachNextStateButton(.5f*(_spawnedDrums[0].transform.position + _spawnedDrums[1].transform.position) + ButtonOffsetWhenDockedToDrums);

         //trigger song
         SongMgr.I.PlaySong();
      }
      else if (newState == ProgressionState.Results)
      {
         _ShowMusicUI(false);

         //stop song
         SongMgr.I.StopSong();
      }

      OnStateChanged.Invoke(prevState, newState);
   }
}
