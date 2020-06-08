using UnityEngine;
using DataStruct;
using System;

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: AudioAnalyzer
 * This class performs DSP related analysis on the input audio
 * source.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
public class AudioAnalyzer : MonoBehaviour
{
    // *****************************************************
    //                       ENUMS
    // *****************************************************

    enum Channel : int
    {
        left,
        right
    }

    // *****************************************************
    //                  CLASS ATTRIBUTES
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * CONFIGS:
     * @fftSize
     *  Number of bins to calculate magnitude of energy for.
     * @windowType
     *  Type of window to multiply audio samples by to reduce
     *  high frequency components that appear from non-integer
     *  number of sampling periods.
     * @numFreqBands
     *  Number of bands to group the frequency bins into.
     * @bufEnable
     *  Enable or disable frequency band buffering.
     * @bufDecreaseStart
     *  Initial amount that the buffer band values will decrease
     *  each frame.
     * @bufDecreaseAcceleration
     *  Percentage that the bufDecreaseStart value will increase
     *  by after every frame.
     * ------------------------------------------------------------
     * STATE:
     * 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // User Configs

    // - Frequency domain
    int fftMaxSize;
    private int fftSize;
    private FFTWindow windowType;

    // - Frequency band buffer
    private bool bufEnable;
    private float bufDecreaseStart;
    private float bufDecreaseAcceleration;

    // State
    public BinStereo bins;
    public FreqBandMono[] freqBands;
    private int numFreqBands;
    private int samplingFreq;

    // Cache
    private AudioSource audioSource;
    private UserConfigs userConfigs;

    // *****************************************************
    //               MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    // Start is called before the first frame update
    void Start()
    {
        // Cache
        audioSource = GetComponent<AudioSource>();
        userConfigs = FindObjectOfType<UserConfigs>();

        // State
        fftMaxSize = UserConfigs.MAX_FFT_SIZE;
        GetUserConfigs(); // Get configs that are set on Awake
        bins = new BinStereo(this.fftMaxSize);

        // Initialize frequency bands
        freqBands = new FreqBandMono[this.numFreqBands];
        for (int i = 0; i < numFreqBands; i++)
            freqBands[i] = new FreqBandMono();
    }

    // Update is called once per frame
    void Update()
    {
        GetUserConfigs();
        UpdateBins();
        UpdateFreqBands();
    }

    // *****************************************************
    //                   PRIVATE METHODS
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GetUserConfigs
     * Update class attributes since user configurations can change
     * during runtime.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void GetUserConfigs()
    {
        numFreqBands = userConfigs.numFreqBands;
        samplingFreq = userConfigs.samplingRate;
        fftSize = userConfigs.fftSize;
        windowType = userConfigs.fftWindowType;
        bufEnable = userConfigs.bufEnable;
        bufDecreaseStart = userConfigs.bufDecreaseStart;
        bufDecreaseAcceleration = userConfigs.bufDecreaseAcceleration;
        audioSource.clip = userConfigs.audioClip;

        // Get frequency band ranges
        /*
        for (int i = 0; i < numFreqBands; i++)
        {
            float average = 0f;
            int startBinIndex = 0; 
            int endBinIndex = 0; 

            // Determine which bin index relates the this band's frequency range.

            freqBands[i].data = average;
        }
        */
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateBins
     * This function captures bin energy for the left and right
     * channels of the selected audio clip.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateBins()
    {
        // Create temporary array to store new samples before copying them
        // over to the dynamic container.
        float[] tempLeftBins = new float[this.fftSize];
        float[] tempRightBins = new float[this.fftSize];

        // Get left and right channel bin amplitudes
        try
        {
            audioSource.GetSpectrumData(tempLeftBins, (int)Channel.left, this.windowType);
            audioSource.GetSpectrumData(tempRightBins, (int)Channel.right, this.windowType);
        }
        catch(Exception e)
        {
            Debug.Log("ERROR (AudioAnalyzer.UpdateBins) calculating spectrum values: fftSize = " + this.fftSize);
            return;
        }

        // Check if bin list needs to be resized and then copy new spectrum data into the container.
        if (this.bins.size != this.fftSize)
            this.bins.ResizeFFT(this.fftSize);

        for (int i = 0; i < this.fftSize; i++)
        {
            this.bins.L[i] = tempLeftBins[i];
            this.bins.R[i] = tempRightBins[i];
        }
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFreqBands
     * Group together a user-specified range of frequencies and
     * calculate their average values.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFreqBands()
    {
        for (int i = 0; i < this.numFreqBands; i++)
        {

        }
    }

    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GetFreqResolution
     * Returns the frequency resolution with the current user 
     * settings.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private float GetFreqResolution()
    {
        return this.samplingFreq / this.fftSize; // Units in Hz
    }
}
