using UnityEngine;
using UnityEditor;

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
    [Header("Audio Clip")]
    [SerializeField] public AudioClip audioClip;

    [Header("Frequency Domain")]
    [SerializeField] public FFTWindow fftWindowType = FFTWindow.Hanning;

    [Header("Frequency Band")]
    [SerializeField] public int numFreqBands = 8;
    [HideInInspector] public bool bufEnable = true;
    [Tooltip("If the new sample value is less than previous sample, start decreasing buffer value at this rate.")]
    [HideInInspector] public float bufDecreaseStart = 0.05f;
    [Tooltip("If the new sample value is less than previous sample, increase 'Decrease Rate' at this acceleration.")]
    [HideInInspector] public float bufDecreaseAcceleration = 0.2f;

    // State
    public int samplingRate;
    public int fftSize;

    // *****************************************************
    //              MONO BEHAVIOUR OVERRIDE
    // *****************************************************

    private void Start()
    {
        // Initialize state variables
        this.samplingRate = AudioSettings.outputSampleRate;
        this.fftSize = 1024;
    }

    private void Update()
    {
        
    }

    // *****************************************************
    //                  PRIVATE METHODS
    // *****************************************************


    // *****************************************************
    //                   PUBLIC METHODS
    // *****************************************************

}

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: UserConfigsEditor
 * This class implements a custom inspector editor for the
 * UserConfigs class.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
[CustomEditor(typeof(UserConfigs))]
public class UserConfigsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        // Use target script instantiation
        var userConfigs = target as UserConfigs;

        // Frequency band buffer details
        userConfigs.bufEnable = GUILayout.Toggle(userConfigs.bufEnable, "Buffer Enable");
        using (new EditorGUI.DisabledScope(!userConfigs.bufEnable))
        {
            userConfigs.bufDecreaseAcceleration = EditorGUILayout.FloatField(
                "Acceleration", 
                userConfigs.bufDecreaseAcceleration);

            userConfigs.bufDecreaseStart = EditorGUILayout.FloatField(
                "Decrease Init", 
                userConfigs.bufDecreaseStart);
        }
    }
}
