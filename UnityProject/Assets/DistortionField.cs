using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives user a certain amount of look ahead time so they know to press the drum on time
/// </summary>
[ExecuteInEditMode]
public class DistortionField : MonoBehaviour
{

   [Range(0, 1)]
   public float progress = 1f;
   public float motionNum = 0f;

   private float scaleAmount = 2f; // Amount of scale the lookahead is at its maximum
   private float speedMultiplier = 1f;

   private Vector3 padOriginalScale;
   private float shaderAmount = 0f;

   private Vector3 fromScale;
   private float fromShader = 10f;

   private Renderer rend;

   // Start is called before the first frame update
   void Start()
   {
      padOriginalScale = gameObject.transform.localScale;
      fromScale = padOriginalScale * scaleAmount;
      rend = this.gameObject.GetComponent<Renderer>();
   }

   /// <summary>
   /// The distortion field to pulse
   /// </summary>
   public void Pulse()
   {
      StopCoroutine("DriveAnimation");
      StartCoroutine("DriveAnimation");
   }

   /// <summary>
   /// Updates the
   /// </summary>
   /// <param name="v"></param>
   private void UpdateScale(float v)
   {
      gameObject.transform.localScale = Vector3.Lerp(fromScale, padOriginalScale, v);
   }

   /// <summary>
   /// Update the shader parameters
   /// </summary>
   /// <param name="v"></param>
   private void UpdateShader(float v)
   {

      motionNum = Mathf.Lerp(fromShader, shaderAmount, v);

      if (motionNum < 0.5 * motionNum)
      {
         rend.sharedMaterial.SetFloat("_Refraction", (motionNum * 1f));
      }
      else
      {
         rend.sharedMaterial.SetFloat("_Refraction", Mathf.Clamp((motionNum * 20f), 0, 10f));
      }
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
            UpdateShader(progress);
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
         UpdateShader(progress);
         progress = Mathf.Clamp01(progress);
      }
   }
}
