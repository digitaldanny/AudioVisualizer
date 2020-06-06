using UnityEngine;
using MyBox;

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
    [SerializeField]
    public AudioClip audioClip;

    [Foldout("FREQUENCY DOMAIN", true)]
    [SerializeField] 
    public FFTWindow fftWindowType = FFTWindow.Hanning;

    [Foldout("FREQUENCY BAND", true)]
    [SerializeField] public bool bufEnable = true;

    [Tooltip("If the new sample value is less than previous sample, start decreasing buffer value at this rate.")]
    [ConditionalField(nameof(bufEnable), false)] 
    public float bufDecreaseStart = 0.05f;

    [Tooltip("If the new sample value is less than previous sample, increase 'Decrease Rate' at this acceleration.")]
    [ConditionalField(nameof(bufEnable), false)] 
    public float bufDecreaseAcceleration = 0.2f;

    [MinMaxRange(0f, 44100f / 2f)]
    public RangedFloat[] freqBandRange;

    // State
    public int samplingRate;
    public int fftSize;
    public int numFreqBands;
    public float freqResolution;

    // *****************************************************
    //              MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    private void Awake()
    {
        SetGlobalDefaults();
    }

    private void Update()
    {
        UpdateFreqResolution();
    }

    // *****************************************************
    //                  PRIVATE METHODS
    // *****************************************************
    private void SetGlobalDefaults()
    {
        // Initialize state variables
        this.samplingRate = AudioSettings.outputSampleRate;
        this.fftSize = 1024;
        this.numFreqBands = 8;

        // initialize frequency band ranges
        freqBandRange = new RangedFloat[numFreqBands];
        freqBandRange[0] = new RangedFloat(0, 100);
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateFreqResolution
     * This function calculates the frequency resolution 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateFreqResolution()
    {
        this.freqResolution = samplingRate / fftSize;
    }

    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************
}
