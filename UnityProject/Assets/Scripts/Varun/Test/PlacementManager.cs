using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeapTools;
using UnityEngine.XR.MagicLeap;

public class PlacementManager : MonoBehaviour
{
    [SerializeField]
    GameObject Drums;
    public static GameObject DrumsPlacedInAR;

//    public KeepInFront inFront;


   public Vector3 raycastRight, raycastLeft, raycastForward, raycastBehind, raycastDown;
   enum RaycastDirectionsForSnapping
   {
      Forward, 
      Behind,
      Right,
      Left,
      Down
   }



   // public Transform handController;
    public static Vector3 placementPosition
    {
        get;
        private set;
    }
    //public ControlInput controllerInput;

    bool contentPlaced = false, placementIsValid = false;
   Ray ray;
   RaycastHit hitinfo;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Drums == null)
            Debug.LogError("Please assign Drums to be placed in AR");
  
    }

   // Update is called once per frame
   void Update()
   {
      Debug.DrawRay(Drums.transform.position, raycastForward);
      Debug.DrawRay(Drums.transform.position, raycastLeft);
      Debug.DrawRay(Drums.transform.position, raycastRight);
      Debug.DrawRay(Drums.transform.position, raycastBehind);
      Debug.DrawRay(Drums.transform.position, raycastDown);
      //UpdatePlacmentPosition();
      //if (placementIsValid)
      //{
      //   //instructions.SetActive(false);
      //   inFront.enabled = false;
      //   Drums.transform.position = placementPosition;
      //   return;
      //}

      ////instructions.SetActive(true);
      //inFront.enabled = true;
   }


   Ray GetRay(RaycastDirectionsForSnapping direction)
    {
      Vector3 raycastDirection = Vector3.zero;

      switch(direction)
      {
         case RaycastDirectionsForSnapping.Forward:
            raycastDirection = Quaternion.Euler(raycastForward) * Vector3.up * 0.1f;
            break;
         case RaycastDirectionsForSnapping.Behind:
            raycastDirection = Quaternion.Euler(raycastBehind) * Vector3.down * 0.1f;
            break;
         case RaycastDirectionsForSnapping.Left:
            raycastDirection = Quaternion.Euler(raycastLeft) * Vector3.left * 0.1f;
            break;
         case RaycastDirectionsForSnapping.Right:
            raycastDirection = Quaternion.Euler(raycastRight) * Vector3.right * 0.1f;
            break;
         case RaycastDirectionsForSnapping.Down:
            raycastDirection = Vector3.down;
            break;

      }
      Debug.DrawRay(Drums.transform.position, raycastDirection);
      return new Ray(Drums.transform.position, raycastDirection);
    }



    public void UpdatePlacmentPosition()
    {
        bool newPositionFound = false;
        Debug.Log("Drums Released");
        //inFront.enabled = false;
        float minimumDistance = Mathf.Infinity;

      foreach(RaycastDirectionsForSnapping direction in System.Enum.GetValues(typeof(RaycastDirectionsForSnapping)))
      {
         ray = GetRay(direction);
         minimumDistance = Mathf.Min(minimumDistance, GetRaycastDistance(ray));
         if (placementIsValid)
         {
            placementPosition = hitinfo.transform.position;
            newPositionFound = true;
            Debug.Log("Drums placed at new position " + placementPosition);
         }

      }
        if (newPositionFound)
            Drums.transform.position = placementPosition;



        //RaycastHit hitinfo;
        //if (Physics.Raycast(ray, out hitinfo))
        //{
        //   SurfaceType surface = SurfaceDetails.Analyze(hitinfo);
        //   //If we hit a surface directly
        //   if (surface == SurfaceType.Table)
        //   {
        //      placementPosition = hitinfo.transform.position;
        //      placementIsValid = true;
        //      return;
        //   }
        //   //If we have already placed our drums in AR and our raycast hits the drums.
        //   if (hitinfo.transform.gameObject.tag == "Drums")
        //   {
        //      RaycastHit[] raycastHit = Physics.RaycastAll(handController.transform.position, handController.transform.forward);
        //      foreach (var hit in raycastHit)
        //      {
        //         SurfaceType surfaceType = SurfaceDetails.Analyze(hit);
        //         if (surface == SurfaceType.Table)
        //         {
        //            placementPosition = hitinfo.transform.position;
        //            placementIsValid = true;
        //            return;
        //         }
        //      }
        //   }
        //   else
        //      placementIsValid = false;
        //}
    }



    float GetRaycastDistance(Ray ray)
   {
      if (Physics.Raycast(ray, out hitinfo))
      {
         UnityEngine.XR.MagicLeap.SurfaceType surface = (UnityEngine.XR.MagicLeap.SurfaceType)SurfaceDetails.Analyze(hitinfo);
         //If we hit a surface directly
         if (surface == UnityEngine.XR.MagicLeap.SurfaceType.Horizontal)
         {
            Debug.Log("Raycast Hit " + surface);
            placementIsValid = true;
            return hitinfo.distance;
         }
         Debug.Log("Raycast does not hit");

         //If we have already placed our drums in AR and our raycast hits the drums.
         //if (hitinfo.transform.gameObject.tag == "Drums")
         //{
         //   RaycastHit[] raycastHit = Physics.RaycastAll(handController.transform.position, handController.transform.forward);
         //   foreach (var hit in raycastHit)
         //   {
         //      SurfaceType surfaceType = SurfaceDetails.Analyze(hit);
         //      if (surface == SurfaceType.Table)
         //      {
         //         placementPosition = hitinfo.transform.position;
         //         placementIsValid = true;
         //      }
         //   }
         //}

      }
      placementIsValid = false;
      return Mathf.Infinity;
   }




   ////Maybe use it later to instantiate the New Drums?
   //public static void ReplaceDrums(GameObject newDrums)
   //{
   //    var currentDrums = DrumsPlacedInAR;
   //    DestroyImmediate(currentDrums, true);
   //    DrumsPlacedInAR = Instantiate(newDrums);
   //}

}



