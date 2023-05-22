using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class DCL_Intro : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public float fadeOutDuration = 3f;
    public float fadeInDuration = 3f;

    private float currentAlpha;
    private float fadeSpeed;


    public VideoPlayer videoPlayer;

    public IEnumerator Start()
    {
        
        videoPlayer.playOnAwake = false;  
        videoPlayer.loopPointReached += EndReached;

        currentAlpha = 1f;
        fadeSpeed = 1f / fadeOutDuration;       

        yield return StartCoroutine(FadeCanvasOut());

        videoPlayer.Play();
    }

    void EndReached(VideoPlayer vp)
    {
        
        videoPlayer.Stop();
        StartCoroutine(CompleteVideo());

    }

    public IEnumerator CompleteVideo()
    {

        yield return StartCoroutine(FadeCanvasIn());
        //carag escena
        SceneManager.LoadScene(1);
    }



    private IEnumerator FadeCanvasIn()
    {
        fadeSpeed = 1f / fadeInDuration;
        while (currentAlpha < 1f)
        {
            currentAlpha += fadeSpeed * Time.deltaTime;
            canvasGroup.alpha = currentAlpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }


    private IEnumerator FadeCanvasOut()
    {
        while (currentAlpha > 0f)
        {
           
            currentAlpha -= fadeSpeed * Time.deltaTime;
           
            canvasGroup.alpha = currentAlpha;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }


}
