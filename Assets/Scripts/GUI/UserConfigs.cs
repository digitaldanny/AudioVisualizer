using UnityEngine;
using MyBox;
using DannyAttributes;
using System;

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: UserConfigs
 * This class contains global properties for the audio visualizer
 * tool configured by the user during runtime.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioAnalyzer))]
public class UserConfigs : MonoBehaviour
{

    // *****************************************************
    //                       CONSTS
    // *****************************************************

    public const int FFT_DEFAULT_SIZE = 1024;
    public const int NUM_FREQ_BANDS = 8;
    public const int MAX_FFT_SIZE = 8192;
    public const float FREQ_BAND_MIN = 0.0f;
    public const float FREQ_BAND_MAX = 20000.0f;
    public const float FREQ_RES_DEFAULT = 100.0f;

    // *****************************************************
    //                  CLASS ATTRIBUTES
    // *****************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * AUDIO CLIP:
     * @audioClip
     * The track that all effects will be applied to.
     * -------------------------------------------------------------
     * FFT:
     * @fftSize
     *  Number of bins to calculate magnitude of energy for.
     * @fftWindowType
     *  Type of window to multiply audio samples by to reduce
     *  high frequency components that appear from non-integer
     *  number of sampling periods.
     * -------------------------------------------------------------
     * FREQUENCY BANDS:
     * @numFreqBands
     *  Number of bands to group the frequency bins into.
     * -------------------------------------------------------------
     * BAND BUFFER:
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

    // User Configs
    [Foldout("AUDIO CLIP", true)]
    [MustBeAssigned]
    public AudioClip audioClip;

    [Foldout("FREQUENCY DOMAIN", true)]
    [SerializeField] public FFTWindow fftWindowType = FFTWindow.Hanning;
    [SerializeField] [Delayed] public int fftSize;
    [SerializeField] [ReadOnly] public float freqResolution;

    [Foldout("FREQUENCY BAND", true)]
    [SerializeField] public bool bufEnable = true;

    [Tooltip("If the new sample value is less than previous sample, start decreasing buffer value at this rate.")]
    [ConditionalField(nameof(bufEnable), false)] 
    public float bufDecreaseStart = 0.05f;

    [Tooltip("If the new sample value is less than previous sample, increase 'Decrease Rate' at this acceleration.")]
    [ConditionalField(nameof(bufEnable), false)] 
    public float bufDecreaseAcceleration = 0.2f;

    [SerializeField]
    [Tooltip("Select the frequency range for this band. Slider steps increase by value of frequency resolution (Hz).")]
    [FreqBandSlider(FREQ_BAND_MIN, FREQ_BAND_MAX)]
    public FreqRange[] freqBand = new FreqRange[NUM_FREQ_BANDS];

    // State
    [HideInInspector] public int samplingRate;
    [HideInInspector] public int numFreqBands;

    // *****************************************************
    //              MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    private void OnValidate()
    {
        CheckConstantSettings();
        samplingRate = AudioSettings.outputSampleRate;
        numFreqBands = freqBand.Length;
        UpdateFFTSize();
        UpdateFreqResolution();
        UpdateFreqBandSelector();
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: CheckConstantSettings
     * Reset values that were accidentally changed to the default.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void CheckConstantSettings()
    {
        // Number of frequency bands can't go past 8 currently.
        if (freqBand.Length != NUM_FREQ_BANDS)
        {
            Debug.LogWarning("WARNING (UserConfigs.CheckConstantSettings): Do not change freqBand size!");
            Array.Resize(ref freqBand, NUM_FREQ_BANDS);
        }
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFreqResolution
     * This function calculates the frequency resolution 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFreqResolution()
    {
        if (samplingRate > 0 && fftSize > 0)
            this.freqResolution = samplingRate / fftSize;
        else
            this.freqResolution = FREQ_RES_DEFAULT;
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFFTSize
     * Rounds the FFT size to be the nearest power of 2.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFFTSize()
    {
        // Handle division by zero error
        if (this.fftSize > 0)
            this.fftSize = (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(this.fftSize) / Mathf.Log(2)));
        else
            this.fftSize = FFT_DEFAULT_SIZE;
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFreqBandSelector
     * Update the frequency resolution for each of the frequency
     * band sliders.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFreqBandSelector()
    {
        for (int i = 0; i < numFreqBands; i++)
        {
            freqBand[i].SetResolution(this.freqResolution);
        }
    }

    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************
}
