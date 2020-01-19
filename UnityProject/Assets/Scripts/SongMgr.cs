using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SongPlayedEvent : UnityEvent<Song>
{
}

public class SongMgr : MonoBehaviour
{
   public AudioSource source;
   public Song[] SongList = new Song[0];

   [Header("Debug")]
   public bool EnablePlayTestSongAtStart = false;
   public int TestSongIdxToPlayAtStart = 0;

   //events
   public SongPlayedEvent OnSongPrepared = new SongPlayedEvent();
   public SongPlayedEvent OnSongPlayed = new SongPlayedEvent();
   public SongPlayedEvent OnSongStopped = new SongPlayedEvent();

   Song _curSong = null;

   public static SongMgr I { get; private set; }

   public Song GetCurrentSong()
   {
      return _curSong;
   }

   void Awake()
   {
      I = this;
   }

   void Start()
   {
      if (EnablePlayTestSongAtStart)
      {
         PrepareSong(GetSong(TestSongIdxToPlayAtStart));
         PlaySong();
      }
   }

   public Song GetSong(int idx)
   {
      return ((idx >= 0) && (idx < SongList.Length)) ? SongList[idx] : null;
   }

   //load the song, but dont play it yet
   public void PrepareSong(Song s)
   {
      if (!s)
         return;

      _curSong = s;
      _curSong.Prepare();

      OnSongPrepared.Invoke(s);
   }

   public void PlaySong()
   {
      if (!_curSong)
      {
         Debug.LogWarning("Can't play song because PrepareSong was not called yet");
         return;
      }

      _curSong.Play(source);

      OnSongPlayed.Invoke(_curSong);
   }

   public void StopSong()
   {
      if (!_curSong || !_curSong.IsPlaying())
      {
         Debug.LogWarning("Can't play song because PrepareSong was not called yet");
         return;
      }

      _curSong.Stop();

      OnSongStopped.Invoke(_curSong);
   }
}
