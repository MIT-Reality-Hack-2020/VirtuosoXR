using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckLocation : MonoBehaviour
{
   private SphereCollider sphereCollider;
   private EffectDrumPad effectDrumPad;
   public GameObject touching;
   public bool inButtonRange;
   [Tooltip("Which drum am I in the authoring?")]
   public int drumIdx = 0;

   [Header("Debug")]
   public bool EnableDebugPlayerSwipeKey = true;
   public KeyCode DebugPlayerSwipeKey = KeyCode.Space;



   //Handler Color Change for drum pad
   [Header("Drum Pad Color Change")]
   [SerializeField]
   SpriteRenderer drumPad = null;
   public Color baseColor, hitColor;

   private void Start()
   {
      effectDrumPad = this.gameObject.GetComponent<EffectDrumPad>();

      ////Find drum pad outer ring
      //for (int i = 0; i < transform.childCount; i++)
      //{
      //    Debug.Log(transform.GetChild(i).name);
      //    if (transform.GetChild(i).name == "pad")
      //    {
      //        this.drumPad = transform.GetChild(0).GetComponent<SpriteRenderer>();
      //        break;
      //    }

      //}
      /*Transform temp = transform.GetChild(0);
      this.drumPad = temp.GetChild(0).GetComponent<SpriteRenderer>();
      if (this.drumPad == null)
         Debug.Log("NULL");
      else
         Debug.Log("GOT iT");*/
   }

   /// <summary>
   /// Check if anything enters
   /// </summary>
   /// <param name="other"></param>
   void OnTriggerEnter(Collider other)
   {
      if (GetComponent<Collider>().GetType() == typeof(BoxCollider)) ;
      {
         if (other.gameObject.tag == "Hand")
         {
            inButtonRange = true;
            this.drumPad.color = hitColor;

            touching = other.gameObject;

            HandSwiped();
         }
      }
   }

   /// <summary>
   /// Called when person is touching a button
   /// </summary>
   /// <param name="touching">The game object we’re near</param>
   public void HandSwiped()
   {
      //ignore when in the place drums state
      if (ProgressionMgr.I && (ProgressionMgr.I.GetCurState() == ProgressionState.PlaceDrums))
         return;

      // On hit
      if (RhythmGameplayMgr.I && RhythmGameplayMgr.I.TriggerPlayerSwing(drumIdx))
      {
         effectDrumPad.EmitHit();
      }

      // On miss!
      else
      {
         effectDrumPad.EmitMiss();
      }
   }

   /// <summary>
   /// Check if anything exits
   /// </summary>
   /// <param name="other"></param>
   void OnTriggerExit(Collider other)
   {
      if (other.gameObject.tag == "Hand")
      {
         inButtonRange = false;
         this.drumPad.color = baseColor;

         touching = null;
      }
   }


   public float timeNearButton = 0f;
   bool timing = false;

   /// <summary>
   /// See how long the user has been triggering the button
   /// </summary>
   /// <returns></returns>
   IEnumerable NearButtonTimer()
   {
      timeNearButton = 0f;

      while (timing)
      {
         timeNearButton += Time.deltaTime;
      }

      yield return null;
   }


   private void Update()
   {
      if (EnableDebugPlayerSwipeKey && Input.GetKeyDown(DebugPlayerSwipeKey))
      {
         HandSwiped();
      }
   }
}

