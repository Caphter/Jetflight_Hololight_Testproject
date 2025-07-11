using UnityEngine;

public class startFireFight : MonoBehaviour
{
    public ParticleSystem particleWater;
    public bool canRunWater = false;
    private float emissionRate;



    // Update is called once per frame
    void Update()
    {
        if(canRunWater)
        {
            emissionRate = 20f;
            var emission = particleWater.emission;
            emission.rateOverTime = emissionRate;
        }

        if(!canRunWater)
        {
            emissionRate = 0f;
            var emission = particleWater.emission;
            emission.rateOverTime = emissionRate;
        }
    }
}
