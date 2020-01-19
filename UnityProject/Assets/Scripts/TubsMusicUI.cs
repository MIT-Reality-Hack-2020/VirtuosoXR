//
// Build a TUBS-style UI for our song pattern:
// https://en.wikipedia.org/wiki/Time_unit_box_system
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubsMusicUI : MonoBehaviour
{
   [Header("Config")]
   public float DistBetweenRows = 1.0f;
   public float DistBetweenNoteChunks = .5f;
   public float DistBetweenBetweenMeasureChunks = .1f;
   public bool AutoPopulateUIWhenSongReady = true;

   [Space(10)]

   public GameObject NoteChunkPrefab;
   public GameObject RowCursorPrefab;

   [Space(5)]
   public GameObject TimeCursor;

   //the chunks for a single row
   class RowChunks
   {
      public void DestroyChunks()
      {
         foreach(var c in Chunks)
         {
            Destroy(c);
         }
         Chunks.Clear();

         if(Cursor)
         {
            Destroy(Cursor);
            Cursor = null;
         }
      }

      public GameObject Cursor = null;
      public int lastCursorIdx = -1;
      public List<GameObject> Chunks = new List<GameObject>();
   }

   private List<RowChunks> Rows = new List<RowChunks>();

   float _maxX = 0.0f;
   float _maxY = 0.0f;

   void Start()
   {
      if (TimeCursor)
         TimeCursor.SetActive(false);

      if (SongMgr.I)
         SongMgr.I.OnSongPrepared.AddListener(_OnSongReady);
   }

   void OnDisable()
   {
      _HideCursors();
   }

   void _OnSongReady(Song song)
   {
      if(AutoPopulateUIWhenSongReady)
         RebuildUI(song);
   }

   public void RebuildUI(Song song, int maxRows = -1)
   {
      List<Song.AuthoringEvent> authoring = song.GetAuthoring();
      if ((authoring == null) || !NoteChunkPrefab)
      {
         Debug.LogWarning("Cant build ui, not configured properly");
         return;
      }

      int numRows = song.GetNumInstrumentPads();
      if(maxRows > 0) //configured to limit rows?
         numRows = Mathf.Max(maxRows, song.GetNumInstrumentPads());

      if (numRows <= 0)
      {
         Debug.LogWarning("Authoring not sufficient to build UI!");
         return;
      }

      if (TimeCursor)
      {
         Vector3 scale = TimeCursor.transform.localScale;
         scale.y = numRows + (numRows - 1)* DistBetweenRows;
         TimeCursor.transform.localScale = scale;
      }

      //clear existing UI
      foreach (var c in Rows)
      {
         c.DestroyChunks();
      }
      Rows.Clear();

      _maxX = _maxY = 0.0f;

      float chunksPerRow = Mathf.RoundToInt(song.LengthInBeats / song.BeatsPerChunk);
      Vector3 chunkOffset = Vector3.zero;
      for(int curRow = 0; curRow < numRows; curRow++)
      {
         RowChunks newRow = new RowChunks();
         Rows.Add(newRow);

         chunkOffset.x = 0.0f;

         //spawn a cursor for this row
         if (RowCursorPrefab)
         {
            GameObject cursorObj = Instantiate(RowCursorPrefab) as GameObject;
            cursorObj.SetActive(false);
            newRow.Cursor = cursorObj;
         }

         float curBeat = 0.0f;
         float prevBeat = 0.0f;
         for(int chunkIdx = 0; chunkIdx < chunksPerRow; chunkIdx++)
         {
            GameObject newObj = Instantiate(NoteChunkPrefab, this.transform) as GameObject;

            newObj.transform.localPosition = chunkOffset;

            newRow.Chunks.Add(newObj);

            TubChunk chunk = newObj.GetComponent<TubChunk>();
            if(chunk)
            {
               bool isNoteOn = song.IsNoteOnBetween(curBeat, curBeat + song.BeatsPerChunk, curRow); 
               chunk.ConfigureVisuals(isNoteOn, curBeat, curBeat + song.BeatsPerChunk);
            }

            chunkOffset.x += DistBetweenNoteChunks;

            //did we pass a measure boundary?
            Song.Mbt prevMbt = song.BeatsToMBT(prevBeat + song.BeatsPerChunk); 
            Song.Mbt curMbt = song.BeatsToMBT(curBeat + song.BeatsPerChunk);
            bool isMeasureBoundary = prevMbt.measure != curMbt.measure;
            if (isMeasureBoundary)
               chunkOffset.x += DistBetweenBetweenMeasureChunks;

            prevBeat = curBeat;
            curBeat += song.BeatsPerChunk;

            if (chunkOffset.x > _maxX)
               _maxX = chunkOffset.x;
         }

         chunkOffset.y += DistBetweenRows;

         if (chunkOffset.y > _maxY)
            _maxY = chunkOffset.y;
      }

      _maxX -= DistBetweenBetweenMeasureChunks + .5f*DistBetweenNoteChunks;
   }

   void Update()
   {
      if ((Rows.Count == 0) || !MasterTimeline.I) 
         return;

      //go thru each row and update the cursor
      if (SongMgr.I.GetCurrentSong() && SongMgr.I.GetCurrentSong().IsPlaying())
      {
         if (TimeCursor)
         {
            TimeCursor.SetActive(true);
            Vector3 curLocalPos = TimeCursor.transform.localPosition;
            curLocalPos.x = _maxX * MasterTimeline.I.GetSongProgress();
            curLocalPos.y = .5f * _maxY;
            TimeCursor.transform.localPosition = curLocalPos;
         }

         float curBeat = MasterTimeline.I.CurBeat;
         int curChunkIdx = Mathf.FloorToInt(curBeat / SongMgr.I.GetCurrentSong().BeatsPerChunk);
         foreach (var r in Rows)
         {
            TubChunk c = r.Chunks[curChunkIdx].GetComponent<TubChunk>();
            if(c && c.IsNoteOn())
            {
               r.Cursor.transform.position = c.transform.position;
               r.Cursor.SetActive(true);
               r.lastCursorIdx = curChunkIdx;
            }
            else if(r.lastCursorIdx >= 0)
            {
               r.Cursor.transform.position = r.Chunks[r.lastCursorIdx].GetComponent<TubChunk>().transform.position;
            }
         }
      }
      else //not playing, hide all cursors
      {
         _HideCursors();
      }
   }

   void _HideCursors()
   {
      if (TimeCursor)
         TimeCursor.SetActive(false);
      foreach (var r in Rows)
      {
         if (r.Cursor)
            r.Cursor.SetActive(false);
      }
   }

}
