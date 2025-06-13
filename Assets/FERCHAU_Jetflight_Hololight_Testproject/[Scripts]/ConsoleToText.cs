using TMPro;
using UnityEngine;


public class ConsoleToText : MonoBehaviour
{
    public TextMeshProUGUI consoleText;
    public int maxEntries = 10;

    void Start()
    {
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        consoleText.text += logString + "\n";

        TrimExcessEntries();
    }

    void TrimExcessEntries()
    {
        string[] lines = consoleText.text.Split('\n');
        if (lines.Length > maxEntries)
        {
            int linesToRemove = lines.Length - maxEntries;
            int totalLength = 0;

            for (int i = 0; i < linesToRemove; i++)
            {
                totalLength += lines[i].Length + 1;
            }

            consoleText.text = consoleText.text.Substring(totalLength);
        }
    }
}
