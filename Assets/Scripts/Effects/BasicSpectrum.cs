using System.Collections;
using UnityEngine;
using DataStruct;

public class BasicSpectrum : MonoBehaviour
{
    // *****************************************************
    //                 CLASS PARAMETERS
    // *****************************************************

    // Configs
    [SerializeField] GameObject sampleCubePrefab;
    [SerializeField] float cubeWidthScale = 5f;
    [SerializeField] float maxHeight = 1000f;
    [SerializeField] float radius = 100f;

    // State
    GameObject[] sampleCubes;
    int count = 0;

    // Cache
    UserConfigs userConfigs;
    AudioAnalyzer audioAnalyzer;

    // *****************************************************
    //               MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    // Start is called before the first frame update
    void Start()
    {
        // State

        // Cache
        userConfigs = FindObjectOfType<UserConfigs>();
        audioAnalyzer = FindObjectOfType<AudioAnalyzer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (count == 0)
        {
            sampleCubes = new GameObject[userConfigs.fftSize];
            for (int i = 0; i < userConfigs.fftSize; i++)
            {
                sampleCubes[i] = Instantiate(
                    sampleCubePrefab
                ) as GameObject;

                sampleCubes[i].transform.position = this.transform.position;
                sampleCubes[i].transform.parent = this.transform; // turn cube into child
                sampleCubes[i].name = "SampleCube" + i;

                // rotate CubeSpawner around current position for next cube instantiation.
                this.transform.eulerAngles = new Vector3(0, (float)i * 360 / userConfigs.fftSize, 0);
                sampleCubes[i].transform.position = Vector3.forward * radius; // radius of the circle
            }
            count++;
        }

        for (int i = 0; i < audioAnalyzer.bins.size; i++)
        {
            if (sampleCubes[i] != null)
            {
                // update height of the cube
                sampleCubes[i].transform.localScale = new Vector3(cubeWidthScale, 
                    maxHeight * (float)audioAnalyzer.bins.L[i],
                    cubeWidthScale);
            }
        }
    }
}
