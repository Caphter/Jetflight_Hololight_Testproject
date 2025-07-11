using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScrolling : MonoBehaviour
{
    public float UVScrollSpeed = 1.0f;
    private Material mat;
    private float uvScrolling;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        uvScrolling = Time.time * UVScrollSpeed;
        mat.SetTextureOffset("_MainTex", new Vector2(0f, uvScrolling));
        mat.SetTextureOffset("_DetailAlbedoMap", new Vector2(0f, -uvScrolling));
    }
}
