using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldStreamer : MonoBehaviour
{
    private Transform terrainParent;
    private static int sizex = 50;
    private static int sizey = 70;
    private static int tilesizex = 803;
    private static int tilesizey = 816;
    private static int viewrange = 4;
    private Transform[,] terrains = new Transform[sizex, sizey];
    private int actualx, actualy;
    private List<GameObject> activeTerrains = new List<GameObject>();
    
    // Start is called before the first frame update
    void Awake()
    {
        //terrainParent = FindObjectOfType<InfinityCode.RealWorldTerrain.RealWorldTerrainContainer>().transform;
        foreach(Transform child in terrainParent)
        {
            string[] subs = child.name.Split(' ');
            string[] subs1 = subs[1].Split("x");
            int x,y;
            int.TryParse(subs1[0], out x);
            int.TryParse(subs1[1], out y);
            Debug.Log(x + " x " + y + " Terrain added");
            terrains[x, y] = child;
            child.gameObject.SetActive(false);
        }
        actualx = (int)transform.position.x / tilesizex;
        actualy = (int)transform.position.z / tilesizey;
        PositionChanged(actualx, (sizey-1)-actualy);
    }

    // Update is called once per frame
    void Update()
    {
        int x = (int)transform.position.x / tilesizex;
        int y = (int)transform.position.z / tilesizey;

        if(x!=actualx || y != actualy)
        {
            actualx = x;
            actualy = y;
            PositionChanged(actualx, (sizey-1)-actualy);
        }

    }

    private void PositionChanged(int newx, int newy)
    {
        
        foreach(GameObject go in activeTerrains)
        {
            go.SetActive(false);
        }
        activeTerrains.Clear();

        for(int i = newx-viewrange; i < newx + viewrange; i++)
        {
           for (int j = newy - viewrange; j < newy + viewrange; j++)
           {
                if (i >= 0 && i < sizex+1 && j>=0 && j <sizey+1)
                {
                    
                    terrains[i, j].gameObject.SetActive(true);
                    activeTerrains.Add(terrains[i, j].gameObject);
                }
           }
        }
        
    }
}
