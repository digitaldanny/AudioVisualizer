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
    FreqRange customSlider = new FreqRange(FREQ_RES_DEFAULT); // default value doesn't matter

    // State
    [HideInInspector] public int samplingRate;
    [HideInInspector] public int numFreqBands;

    // *****************************************************
    //              MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    private void OnValidate()
    {
        samplingRate = AudioSettings.outputSampleRate;
        numFreqBands = NUM_FREQ_BANDS;
        UpdateFFTSize();
        UpdateFreqResolution();
        customSlider.SetResolution(this.freqResolution);
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

    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************
}
