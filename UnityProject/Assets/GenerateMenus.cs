using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMenus : MonoBehaviour
{
    public GameObject menu;
    public GameObject menuitem;
    public SongMgr songMgr;
    // public Song[] SongList = new Song[0];

    // Start is called before the first frame update
    void Start()
    {
        songMgr = menu.GetComponent<SongMgr>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
