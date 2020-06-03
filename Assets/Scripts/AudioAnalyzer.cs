using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: BinSamples
 * This class contains left (L) and right (R) bin samples for
 * a selected audio clip.
 * Upon instantiation, user can define fft size.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
public class BinSamples
{
    public float[] L;
    public float[] R;

    public BinSamples(int fftSize)
    {
        L = new float[fftSize];
        R = new float[fftSize];
    }
}

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: AudioAnalyzer
 * This class performs DSP related analysis on the input audio
 * source.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
[RequireComponent(typeof(AudioSource))]
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
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // Configs
    [Header("Frequency Domain")]
    [SerializeField] int fftSize = 512;
    [SerializeField] FFTWindow windowType = FFTWindow.Hanning;
    [SerializeField] int numFreqBands = 8;

    [Header("Buffer")]
    [SerializeField] bool bufEnable = true;
    [SerializeField] float bufDecreaseStart = 0.05f;
    [SerializeField] float bufDecreaseAcceleration = 0.2f;

    // State
    int samplingFreq;
    BinSamples bins;
    float[] freqBand;

    // Cache
    AudioSource audioSource;

    // *****************************************************
    //               MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    // Start is called before the first frame update
    void Start()
    {
        // State
        bins = new BinSamples(this.fftSize);
        samplingFreq = AudioSettings.outputSampleRate;

        // Cache
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBins();
        UpdateFreqBands();
    }

    // *****************************************************
    //                   PRIVATE METHODS
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateBins
     * This function captures bin energy for the left and right
     * channels of the selected audio clip.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateBins()
    {
        // Get left and right channel bin amplitudes
        audioSource.GetSpectrumData(this.bins.L, (int)Channel.left, this.windowType);
        audioSource.GetSpectrumData(this.bins.R, (int)Channel.right, this.windowType);
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFreqBands
     * Group together a user-specified range of frequencies.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFreqBands()
    {

    }

    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GetBins
     * These functions return the current left and right channel
     * bin amplitudes.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public BinSamples GetBins() { return this.bins; }

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
