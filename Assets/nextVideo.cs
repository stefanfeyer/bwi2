using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class nextVideo : MonoBehaviour
{

    string path;
    string[] videoUrls;
    //string[] blendingTextures;
    string[] regions;
    string[] descriptions;
    int videoIndex;
    bool isButtonBlocked;
    bool waitFlag;
    private AndroidJavaObject plugin;

    //adb logcat -s Unity ActivityManager PackageManager dalvikvm DEBUG

    // Use this for initialization
    void Start()
    {
        // the regions shown on the blende, are defined here
        regions = new string[] { "Bodensee", "Auf dem Weg nach Süden", "Afrika" };
        // the description text on the blende are defined here
        descriptions = new string[] { "Die Störche nutzen eine Thermik \n um höhe zu gewinnen.", "Die Störche nutzen die gewonnene \n höhe für ihre Reise.", "Die Störche nutzen eine Thermik \n um höhe zu gewinnen." };
        //default path for streaming assets on the Android device, the videos are stored in StreamingAssets
        string path = "jar:file://" + Application.dataPath + "!/assets/";
        // defines the paths to the video files
        videoUrls = new string[] { path + "bodensee.mp4", path + "zurich.mp4", path + "afrika.mp4" };
        // bool for blocking the button while video is loading
        isButtonBlocked = false;
        //the next video to play is the first in the videoUrls arry
        videoIndex = 0;
        // waitFlag is needed for the video loop
        waitFlag = false;
        //start playing the first video
        StartCoroutine(PlayVideoAndFadeAlpha());
    }

    // Update is called once per frame
    void Update()
    {
        // if the big button on the controller is clicked
        if (GvrControllerInput.ClickButtonDown)
        {
            // and the button is not blocked (is blocked while loading a video)
            if (!isButtonBlocked)
            {
                // block the button and start the call the method to play the nexxt video
                isButtonBlocked = true;
                //play the next video
                StartCoroutine(PlayVideoAndFadeAlpha());
            }
        }
        // secondary button on controller to pause the video while the button is pressed down
        if (GvrControllerInput.AppButtonDown)
        {
            if (!isButtonBlocked)
            {
                //pauses the video
                GameObject.Find("VideoPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().Pause();
            }
        }
        // secondary button on controller to resume video when the button is released
        if (GvrControllerInput.AppButtonUp)
        {
            if (!isButtonBlocked)
            {
                //plays the video again
                GameObject.Find("VideoPlayer").GetComponent<UnityEngine.Video.VideoPlayer>().Play();
            }
        }
        // debugging if running on pc, press right arrow to switch video
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //play the next video
            StartCoroutine(PlayVideoAndFadeAlpha());
        }
        //starting from here ist to automatically play the next video if the current video is up to end
        UnityEngine.Video.VideoPlayer videoPlayer = GameObject.Find("VideoPlayer").GetComponent<UnityEngine.Video.VideoPlayer>();
        long currentFrame = videoPlayer.frame;
        long totalFrames = (long)videoPlayer.frameCount;

        //detect if the video is about to end:
        if (currentFrame > 500 && totalFrames-40 == currentFrame && videoPlayer.isPlaying && !waitFlag)
        {
            waitFlag = true;
            isButtonBlocked = true;
            //play the next video
            StartCoroutine(PlayVideoAndFadeAlpha());
        }
        

    }

    IEnumerator PlayVideoAndFadeAlpha()
    {
        //for fade in and out, the material of the cube in front of the camera is needed
        Material material = GameObject.Find("blende").GetComponent<Renderer>().material;
        //how fast to fade in?
        float fadeSpeed = 0.8f;
        // Alpha start value.
        float alpha = 0.0f;

        //get the video player component
        UnityEngine.Video.VideoPlayer videoPlayer = GameObject.Find("VideoPlayer").GetComponent<UnityEngine.Video.VideoPlayer>();

        //the actual fade
        while (alpha < 1.0f)
        {
            // Reduce alpha by fadeSpeed amount.
            alpha += fadeSpeed * Time.deltaTime;
            //change alpha value
            material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
            yield return null;
        }
        //stop the video
        videoPlayer.Stop();
        // switch the url
        videoPlayer.url = videoUrls[videoIndex];
        //find the texts to change
        TextMeshPro regionOut = GameObject.Find("regionOut").GetComponent<TextMeshPro>();
        TextMeshPro descriptionOut = GameObject.Find("descriptionOut").GetComponent<TextMeshPro>();
        //set new texts
        regionOut.text = regions[videoIndex];
        //one text is longer so we reduce the font size a little
        if (videoIndex == 1)
        {
            regionOut.fontSize = 0.05f;
        } else
        {
            regionOut.fontSize = 0.1f;
        }
        //text opacity to maximum
        regionOut.faceColor = new Color32(255, 255, 255, 255);
        descriptionOut.text = descriptions[videoIndex];
        descriptionOut.faceColor = new Color32(255, 255, 255, 255);

        //go to next video index
        videoIndex++;
        //only 3 videos
        videoIndex = videoIndex % 3;
        //load the video
        videoPlayer.Prepare();
        // wait for the video to be prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        //video is loaded, so play it
        videoPlayer.Play();
        //fade out speed is lower to have more time to read the text 
        fadeSpeed = 0.2f;
        //fade...
        while (alpha > 0.0f)
        {
            // Reduce alpha by fadeSpeed amount.
            alpha -= fadeSpeed * Time.deltaTime;
            //change alpha value
            material.color = new Color(material.color.r, material.color.g, material.color.b, alpha)
            yield return null;
        }
        //after fading is done, make the text transparent
        regionOut.faceColor = new Color32(255, 255, 255, 0);
        descriptionOut.faceColor = new Color32(255, 255, 255, 0);
        isButtonBlocked = false;
        waitFlag = false;
    }
}