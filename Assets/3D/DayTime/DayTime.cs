using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTime : MonoBehaviour
{
    [Header("Day Time IDs:\n0 = not set\n1 = Day\n2 = Sun Down\n3 = Night")]
    public int dayTimeId = 0; 
    public GameObject sunLight;
    public GameObject lightMuc;
    public GameObject lightsMuc;
    public Material SkyMap_Day;
    public Material SkyMap_SunDown;
    public Material SkyMap_Night;

    private Color32 colorDay = new Color32(255, 244, 214, 255);
    private Color32 colorSunDown = new Color32(233, 100, 17, 255);
    private Color32 colorNight = new Color32(233, 231, 212, 255);

    private Color32 colorFogDay = new Color32(177, 208, 244, 255);
    private Color32 colorFogSunDown = new Color32(187, 167, 133, 255);
    private Color32 colorFogNight = new Color32(51, 54, 67, 255);


    // Start is called before the first frame update
    void Start()
    {
        lightMuc.SetActive(false);
        lightsMuc.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (dayTimeId)
        {
            case 1: //Day
                lightMuc.SetActive(false);
                lightsMuc.SetActive(false);
                sunLight.transform.localEulerAngles = new Vector3(144f, -30f, 0f);
                sunLight.GetComponent<Light>().color = colorDay;
                sunLight.GetComponent<Light>().intensity = 0.8f;
                RenderSettings.skybox = SkyMap_Day;
                RenderSettings.fogColor = colorFogDay;
                DynamicGI.UpdateEnvironment();
                dayTimeId = 0;
                break;
            case 2: //Sun Down
                lightMuc.SetActive(false);
                lightsMuc.SetActive(true);
                sunLight.transform.localEulerAngles = new Vector3(175f, -90f, 0f);
                sunLight.GetComponent<Light>().color = colorSunDown;
                sunLight.GetComponent<Light>().intensity = 1.5f;                
                RenderSettings.skybox = SkyMap_SunDown;
                RenderSettings.fogColor = colorFogSunDown;
                DynamicGI.UpdateEnvironment();
                dayTimeId = 0;
                break;
            case 3: //Night
                lightMuc.SetActive(true);
                lightsMuc.SetActive(true);
                sunLight.transform.localEulerAngles = new Vector3(145f, -78f, 0f);
                sunLight.GetComponent<Light>().color = colorNight;
                sunLight.GetComponent<Light>().intensity = 0.3f;
                RenderSettings.skybox = SkyMap_Night;
                RenderSettings.fogColor = colorFogNight;
                DynamicGI.UpdateEnvironment();
                dayTimeId = 0;
                break;
            default:
                
                break;
        }
    }
}
