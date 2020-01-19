using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public GameObject progressBar;

    [Range(0,1)]
    public float progressTest = 0;

    // Start is called before the first frame update
    void Start()
    {
        progressBar = gameObject.transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// Moves the location of the bar, takes flaot  0 - 1
    /// </summary>
    /// <param name="t"></param>
    void UpdatePosition (float t = 0)
    {

        float updatedBarLocationX = Mathf.Lerp(10, -10, Mathf.Clamp01(t));
        Vector3 updatedBarLocation = new Vector3(updatedBarLocationX, 0,0);

        progressBar.transform.localPosition = updatedBarLocation;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition(progressTest);
    }

}
