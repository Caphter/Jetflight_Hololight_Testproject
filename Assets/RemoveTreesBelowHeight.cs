using UnityEngine;

public class RemoveTreesBelowHeight : MonoBehaviour
{
    public Terrain terrain; // Ziehe dein Terrain-Objekt hier rein
    public float heightThreshold = 10f; // Die Höhengrenze (Y-Wert), unter der Bäume gelöscht werden

    void Start()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("Kein Terrain zugewiesen!");
                return;
            }
        }

        RemoveTrees();
    }

    void RemoveTrees()
    {
        // Hole die Terrain-Daten
        TerrainData terrainData = terrain.terrainData;
        TreeInstance[] trees = terrainData.treeInstances;
        int[] treeIndicesToKeep = new int[trees.Length];
        int keepCount = 0;

        // Prüfe jede Baum-Instanz
        for (int i = 0; i < trees.Length; i++)
        {
            Vector3 treePos = trees[i].position; // Position im Terrain-Koordinatensystem (0-1)
            // Konvertiere in Weltkoordinaten
            treePos = new Vector3(
                treePos.x * terrainData.size.x,
                treePos.y * terrainData.size.y,
                treePos.z * terrainData.size.z
            ) + terrain.transform.position;

            // Prüfe, ob die Höhe des Baums unter der Schwelle liegt
            if (treePos.y >= heightThreshold)
            {
                treeIndicesToKeep[keepCount] = i;
                keepCount++;
            }
        }

        // Erstelle ein neues Array für die zu behaltenden Bäume
        TreeInstance[] newTrees = new TreeInstance[keepCount];
        for (int i = 0; i < keepCount; i++)
        {
            newTrees[i] = trees[treeIndicesToKeep[i]];
        }

        // Aktualisiere die Baum-Instanzen im Terrain
        terrainData.treeInstances = newTrees;
        Debug.Log($"Es wurden {trees.Length - keepCount} Bäume unter der Höhe {heightThreshold} entfernt.");
    }
}