using UnityEngine;
using DannyAttributes;
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
     * STATE:
     * @bins
     *  Contains the most recent FFT bin calculations.
     * @bands
     *  Contains the most recent frequency band calculations.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // PUBLIC State
    public BinStereo bins;
    public FreqBandStereo bands;
    [SerializeField] public FreqBandStereo bandBufs;

    // PRIVATE State
    [SerializeField] private EasyList<float> _bandBufLeftDecrease;
    [SerializeField] private EasyList<float> _bandBufRightDecrease;

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
        bins = new BinStereo(UserConfigs.MAX_FFT_SIZE);
        bands = new FreqBandStereo(userConfigs.numFreqBands);
        bandBufs = new FreqBandStereo(userConfigs.numFreqBands);
        _bandBufLeftDecrease = new EasyList<float>(100, userConfigs.bufDecreaseStart); // shouldn't ever be more than 100 freq bands
        _bandBufRightDecrease = new EasyList<float>(100, userConfigs.bufDecreaseStart); // shouldn't ever be more than 100 freq bands
        _bandBufLeftDecrease.Resize(userConfigs.numFreqBands);
        _bandBufRightDecrease.Resize(userConfigs.numFreqBands);

        // Set the audio clip to play
        audioSource.clip = userConfigs.audioClip;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBins();
        UpdateFreqBands();
        UpdateFreqBandBufs();
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
        // Create temporary array to store new samples before copying them
        // over to the dynamic container.
        int fftSize = userConfigs.fftSize;
        float[] tempLeftBins = new float[fftSize];
        float[] tempRightBins = new float[fftSize];

        // Get left and right channel bin amplitudes
        try
        {
            FFTWindow windowType = userConfigs.fftWindowType;
            audioSource.GetSpectrumData(tempLeftBins, (int)Channel.left, windowType);
            audioSource.GetSpectrumData(tempRightBins, (int)Channel.right, windowType);
        }
        catch(Exception e)
        {
            Debug.Log("ERROR (AudioAnalyzer.UpdateBins) calculating spectrum values: fftSize = " + fftSize);
            return;
        }

        // Check if bin list needs to be resized and then copy new spectrum data into the container.
        if (this.bins.size != fftSize)
            this.bins.ResizeFFT(fftSize);

        for (int i = 0; i < fftSize; i++)
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
        int numFreqBands = userConfigs.numFreqBands;

        for (int i = 0; i < numFreqBands; i++)
        {
            FreqRange fband = userConfigs.freqBand[i];
            float minFreq = fband.minFreqSel;
            float maxFreq = fband.maxFreqSel;
            float freqRes = fband.freqRes;
            float L = 0;
            float R = 0;

            // determine where to start/end summation
            int startIndex = (int)Mathf.Ceil(minFreq / freqRes);
            int endIndex = (int)Mathf.Floor(maxFreq / freqRes);
            int count = 0;
            
            // summation
            for (int j = startIndex; j <= endIndex; j++)
            {
                L += (float)this.bins.L[j];
                R += (float)this.bins.R[j];
                count++;
            }

            // average together all bins within this band's freqency range.
            this.bands.L[i] = L / count;
            this.bands.R[i] = R / count;
        }
    }

    private void UpdateFreqBandBufs()
    {
        int numFreqBands = userConfigs.numFreqBands;
        float defaultValue = userConfigs.bufDecreaseStart;
        float acceleration = userConfigs.bufDecreaseAcceleration;

        // Check if user changed the starting buffer decrease amount
        if (this._bandBufLeftDecrease.defaultValue != defaultValue)
        {
            this._bandBufLeftDecrease.defaultValue = defaultValue;
            this._bandBufRightDecrease.defaultValue = defaultValue;
        }

        // ~ Only decrease the buffer by the decrease amount if the band value has
        // lowered.
        // ~ Only increase the "decrease" amount if the band value continues to 
        // decrease.
        for (int i = 0; i < numFreqBands; i++)
        {
            // Left channel
            if ((float)this.bands.L[i] >= (float)this.bandBufs.L[i])
            {
                this.bandBufs.L[i] = this.bands.L[i]; // buff = raw freqBand value
                this._bandBufLeftDecrease[i] = this._bandBufLeftDecrease.defaultValue; // reset decrease amount
            }
            else
            {
                float unclamped = (float)this.bandBufs.L[i] - (float)_bandBufLeftDecrease[i]; // lower band value by decrease amount
                this.bandBufs.L[i] = Mathf.Clamp(unclamped, 0.0001f, Mathf.Abs(unclamped)); // clamp so value doesn't go below 0
                _bandBufLeftDecrease[i] = (float)_bandBufLeftDecrease[i] * (1.0f + acceleration * Time.deltaTime); // accelerate the decrease amount
            }

            // Right channel
            if ((float)this.bands.R[i] >= (float)this.bandBufs.R[i])
            {
                this.bandBufs.R[i] = this.bands.R[i]; // buff = raw freqBand value
                this._bandBufRightDecrease[i] = this._bandBufRightDecrease.defaultValue; // reset decrease amount
            }
            else
            {
                float unclamped = (float)this.bandBufs.R[i] - (float)_bandBufRightDecrease[i]; // lower band value by decrease amount
                this.bandBufs.R[i] = Mathf.Clamp(unclamped, 0.0001f, Mathf.Abs(unclamped)); // clamp so value doesn't go below 0
                _bandBufRightDecrease[i] = (float)_bandBufRightDecrease[i] * (1.0f + acceleration * Time.deltaTime); // accelerate the decrease amount
            }
        }
    }
}
