using UnityEngine;
using System.Diagnostics;

public class DungeonGenerationEvaluation : MonoBehaviour
{
    DungeonGenerator dungeonGenerator;

    private void Awake()
    {
        dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void Start()
    {
        double totalGenerationTime = 0;
        int numberOfGenerations = 500;
        for (int i = 0; i < numberOfGenerations; ++i)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-9.0
            Stopwatch sw = Stopwatch.StartNew();
            bool gameEnvironmentGenerated = dungeonGenerator.GenerateGameEnvironment();
            sw.Stop();

            totalGenerationTime += sw.Elapsed.TotalMilliseconds;
        }
        double averageGenerationTime = totalGenerationTime / numberOfGenerations;

        UnityEngine.Debug.Log($"Average Generation Time: {averageGenerationTime / 1000} seconds");
    }
}
