using UnityEngine;
using DataStruct;
using System;
using DannyAttributes;

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

    // State
    public BinStereo bins;
    public FreqBandStereo bands; 

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

        // Set the audio clip to play
        audioSource.clip = userConfigs.audioClip;
        audioSource.Play();
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
}
