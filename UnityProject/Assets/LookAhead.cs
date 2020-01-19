using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives user a certain amount of look ahead time so they know to press the drum on time
/// </summary>
//[ExecuteInEditMode]
public class LookAhead : MonoBehaviour
{

   [Range(0, 1)]
   public float progress = 1f;

   private float scaleAmount = 2f; // Amount of scale the lookahead is at its maximum
   private float speedMultiplier = 1f;

   private Vector3 padOriginalScale;
   private Color padOriginalColor;

   private Vector3 fromScale;
   private Color fromColor;

   // Start is called before the first frame update
   void Start()
   {
      padOriginalScale = gameObject.transform.localScale;
      padOriginalColor = new Color(1f, 1f, 1f, 1f);

      fromScale = padOriginalScale * scaleAmount;
      fromColor = new Color(1f, 1f, 1f, 0f);
   }

   /// <summary>
   /// 
   /// </summary>
   public void StartLookahead()
   {
      StopCoroutine("DriveAnimation");
      StartCoroutine("DriveAnimation");
   }

   private void UpdateScale(float v)
   {
      gameObject.transform.localScale = Vector3.Lerp(padOriginalScale, fromScale, v);
   }

   private void UpdateColor(float v)
   {
      gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(padOriginalColor, fromColor, v);
   }

   /// <summary>
   /// Set a timer
   /// </summary>
   /// <returns></returns>
   private IEnumerator DriveAnimation()
   {
      progress = 1f;

      while (true)
      {
         progress -= Time.deltaTime * speedMultiplier;

         if (progress < 0f)
         {
            progress = Mathf.Clamp01(progress);

            yield return null;
         }
         else
         {
            UpdateScale(progress);
            UpdateColor(progress);
         }

         progress = Mathf.Clamp01(progress);

         yield return new WaitForSeconds(0);
      }
   }


   private bool debugging = true;

   /// <summary>
   /// Debug
   /// </summary>
   private void Update()
   {
      if (debugging)
      {
         UpdateScale(progress);
         UpdateColor(progress);
         progress = Mathf.Clamp01(progress);
      }
   }
}
