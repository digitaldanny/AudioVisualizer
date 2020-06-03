using UnityEngine;

public class InstantiateCubes : MonoBehaviour
{
    [SerializeField] float maxHeight = 1000f;
    [SerializeField] float radius = 100f;

    public GameObject sampleCubePrefab;
    GameObject[] sampleCubes;
    AudioAnalyzer audioAnalyzer;

    // Start is called before the first frame update
    void Start()
    {
        sampleCubes = new GameObject[512];
        audioAnalyzer = FindObjectOfType<AudioAnalyzer>();

        for (int i = 0; i < 512; i++)
        {
            sampleCubes[i] = Instantiate(
                sampleCubePrefab
            ) as GameObject;

            sampleCubes[i].transform.position = this.transform.position;
            sampleCubes[i].transform.parent = this.transform; // turn cube into child
            sampleCubes[i].name = "SampleCube" + i;

            // rotate CubeSpawner around current position for next cube instantiation.
            this.transform.eulerAngles = new Vector3(0, (float)i * 360 / 512, 0);
            sampleCubes[i].transform.position = Vector3.forward * radius; // radius of the circle
        }
    }

    // Update is called once per frame
    void Update()
    {
        BinSamples bins = audioAnalyzer.GetBins();

        for (int i = 0; i < 512; i++)
        {
            if (sampleCubes[i] != null)
            {
                // position cubes in shape of circle

                // update height of the cube
                sampleCubes[i].transform.localScale = new Vector3(10, maxHeight * bins.L[i], 10);
            }
        }
    }
}
