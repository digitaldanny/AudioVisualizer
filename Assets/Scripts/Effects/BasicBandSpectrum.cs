using UnityEngine;

public class BasicBandSpectrum : MonoBehaviour
{
    // *****************************************************
    //                 CLASS PARAMETERS
    // *****************************************************

    // Configs
    [SerializeField] GameObject sampleCubePrefab;
    [SerializeField] Transform markers;
    [SerializeField] float cubeWidthScale = 20f;
    [SerializeField] float maxHeight = 1000f;
    [SerializeField] bool bufferEnable = false;

    // State
    GameObject[] sampleCubes;

    // Cache
    UserConfigs userConfigs;
    AudioAnalyzer audioAnalyzer;

    // *****************************************************
    //               MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    // Start is called before the first frame update
    void Start()
    {
        // Cache
        userConfigs = FindObjectOfType<UserConfigs>();
        audioAnalyzer = FindObjectOfType<AudioAnalyzer>();

        // Instantiate all cubes based on marker locations
        sampleCubes = new GameObject[userConfigs.fftSize];
        for (int i = 0; i < userConfigs.numFreqBands; i++)
        {
            // spawn cubes as children at location of the markers
            sampleCubes[i] = Instantiate(
                sampleCubePrefab,
                markers.GetChild(i).position,
                Quaternion.identity,
                transform
            ) as GameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < audioAnalyzer.bands.numBands && i < userConfigs.numFreqBands; i++)
        {
            if (sampleCubes[i] != null)
            {
                float bandValue;

                // Get the mono value of the specified band OR band buffer.
                if (bufferEnable)
                {
                    bandValue = ((float)audioAnalyzer.bandBufs.L[i] + (float)audioAnalyzer.bandBufs.R[i]) / 2;
                }
                else
                {
                    bandValue = ((float)audioAnalyzer.bands.L[i] + (float)audioAnalyzer.bands.R[i]) / 2;
                }

                // update height of the cube
                sampleCubes[i].transform.localScale = new Vector3(cubeWidthScale,
                    maxHeight * bandValue,
                    cubeWidthScale);
            }
        }
    }
}
