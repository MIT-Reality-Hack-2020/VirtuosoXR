//
// Each chunk in the TubsMusicUI.  this configures the different visual states
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubChunk : MonoBehaviour
{
   [Header("Color Customization")]
   public bool EnableColorCustomization  = true;
   public Renderer RendererToChangeColor;
   public Color NoteOnColor = Color.cyan;
   public Color NoteOffColor = Color.gray;

   bool _isNoteOn = false;
   float _fromBeat = 0.0f;
   float _toBeat = 0.0f;

   public bool IsNoteOn() { return _isNoteOn; }
   public float GetFromBeat() { return _fromBeat; }
   public float GetToBeat() { return _toBeat; }

   //called when the UI is built
   public void ConfigureVisuals(bool isNoteOn, float fromBeat, float toBeat)
   {
      _isNoteOn = isNoteOn;
      _fromBeat = fromBeat;
      _toBeat = toBeat;

      if (EnableColorCustomization && RendererToChangeColor)
      {
         RendererToChangeColor.material.color = isNoteOn ? NoteOnColor : NoteOffColor;
      }
   }
}
