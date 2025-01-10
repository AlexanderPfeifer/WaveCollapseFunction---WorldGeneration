using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RandomTester : MonoBehaviour
{
    [SerializeField] private int numberOfRandoms;
    [SerializeField] private Vector2Int randomRange;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start(); // Start the stopwatch
        
        RandomTest(numberOfRandoms);
        
        stopwatch.Stop(); // Stop the stopwatch
        UnityEngine.Debug.Log($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
    }


    void RandomTest(int number)
    {
        Dictionary<int,int> verteilungNumber = new Dictionary<int, int>();
        
        for (int i = 0; i < number; i++)
        {
            int ranInt = Random.Range(randomRange.x, randomRange.y);
            int counter = 0;
            verteilungNumber.TryGetValue(ranInt, out counter);
            if (counter != 0)
            {
                counter++;
                verteilungNumber[ranInt] = counter;
            }
            else
            {
                verteilungNumber.Add(ranInt, 1);
            }
        }

        /*var verteilungNumberSorted = new SortedDictionary<int, int>(verteilungNumber);
        
        foreach (KeyValuePair<int,int> keyValuePair in verteilungNumberSorted)
        {
            Debug.Log("number: " + keyValuePair.Key + " - count: " + keyValuePair.Value);
        }*/
        
    }
}
