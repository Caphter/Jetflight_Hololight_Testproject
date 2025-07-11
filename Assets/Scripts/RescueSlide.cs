using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueSlide : MonoBehaviour
{
    [Header("Animation IDs:\n0 = Nothing\n1 = Ground Full\n2 = Ground Flat\n3 = Ground Stuck\n4 = Ground Roll Out\n5 = Ground Lose Air\n6 = Ground Fire\n7 = Water Full\n8 = Water Flat\n9 = Water Stuck\n10 = Water Roll Out\n11 = Water Lose Air\n12 = Water Fire")]
    public int animID = 0;
    public GameObject laneModel;
    public GameObject laneGroup;
    public AudioSource audioSlide;
    public AudioClip audioClipSlideFull;
    public AudioClip audioClipSlideShort;

    private int animIDOld;
    private float timer;
    private float normalScale = 0f;
    private Animation slideAnim;
    private Material mat;

    private int normalScaleID;

    // Start is called before the first frame update
    void Start()
    {
        slideAnim = laneGroup.GetComponent<Animation>();
        mat = laneModel.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (animIDOld != animID)
            PlayAnimation();

        SetFoldsAnimation();

        animIDOld = animID;
        
    }

    private void PlayAnimation()
    {
        switch (animID)
        {
            case 1:
                slideAnim.Play("groundFull", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 1;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 2:
                slideAnim.Play("groundFlat", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideShort, 1f);
                normalScaleID = 2;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 3:
                slideAnim.Play("groundStuck", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 3;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 4:
                slideAnim.Play("groundRollout", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideShort, 1f);
                normalScaleID = 4;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 5:
                slideAnim.Play("groundLoseair", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 5;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 6:
                slideAnim.Play("groundFire", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 5;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 7:
                slideAnim.Play("waterFull", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 1;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 8:
                slideAnim.Play("waterFlat", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideShort, 1f);
                normalScaleID = 2;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 9:
                slideAnim.Play("waterStuck", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 3;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 10:
                slideAnim.Play("waterRollout", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideShort, 1f);
                normalScaleID = 4;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 11:
                slideAnim.Play("waterLoseair", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 5;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
            case 12:
                slideAnim.Play("waterFire", PlayMode.StopAll);
                audioSlide.PlayOneShot(audioClipSlideFull, 1f);
                normalScaleID = 5;
                animID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;

            default:
                animID = 0;
                normalScaleID = 0;
                timer = 0f;
                normalScale = 0.8f;
                break;
        }
    }

    private void SetFoldsAnimation()
    {
        if (normalScaleID == 1)
        {
            timer += Time.deltaTime;
            float timeStamp1 = 5.24f;
            float timeStamp2 = 6.84f;
            float timeDiff = timeStamp2 - timeStamp1;
            float timeScaler = 1f / timeDiff;

            if (timer >= timeStamp1)
            {
                normalScale -= (Time.deltaTime * timeScaler);
                if (normalScale <= 0f)
                    normalScale = 0;
            }
            if(timer >= timeStamp2)
            {
                normalScale = 0f;
                normalScaleID = 0;
            }

            mat.SetFloat("_DetailNormalMapScale", normalScale);
        }


        if (normalScaleID == 2)
        {
            mat.SetFloat("_DetailNormalMapScale", normalScale);
            normalScaleID = 0;
        }

        if (normalScaleID == 3)
        {
            timer += Time.deltaTime;
            float timeStamp1 = 1.48f;
            float timeStamp2 = 4.60f;
            float timeDiff = timeStamp2 - timeStamp1;
            float timeScaler = 1f / timeDiff;

            if (timer >= timeStamp1)
            {
                normalScale -= (Time.deltaTime * timeScaler);
                if (normalScale <= 0.4f)
                    normalScale = 0.4f;
            }
            if (timer >= timeStamp2)
            {
                normalScale = 0.4f;
                normalScaleID = 0;
            }

            mat.SetFloat("_DetailNormalMapScale", normalScale);
        }

        if (normalScaleID == 4)
        {
            mat.SetFloat("_DetailNormalMapScale", normalScale);
            normalScaleID = 0;
        }

        if (normalScaleID == 5)
        {
            timer += Time.deltaTime;
            float timeStamp1 = 5.28f;
            float timeStamp2 = 6.88f;
            float timeDiff = timeStamp2 - timeStamp1;
            float timeScaler = 1f / timeDiff;

            float timeStamp3 = 10.4f;
            float timeStamp4 = 13.6f;
            float timeDiff1 = timeStamp4 - timeStamp3;
            float timeScaler1 = 1f / timeDiff1;

            if (timer >= timeStamp1 && timer < timeStamp2)
            {
                normalScale -= (Time.deltaTime * timeScaler);
                if (normalScale <= 0f)
                    normalScale = 0;
            }
            if (timer >= timeStamp2 && timer < timeStamp3)
            {
                normalScale = 0f;
            }
            if (timer >= timeStamp3 && timer < timeStamp4)
            {
                normalScale += (Time.deltaTime * timeScaler1);
                if (normalScale >= 0.7f)
                    normalScale = 0.7f;
            }
            if (timer >= timeStamp4)
            {
                normalScale = 0.7f;
                normalScaleID = 0;
            }

                mat.SetFloat("_DetailNormalMapScale", normalScale);
            print("Yes");
        }
    }
}
