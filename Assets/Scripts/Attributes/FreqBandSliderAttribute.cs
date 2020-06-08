/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: FreqBandSliderAttribute.cs
 * This file implements a frequency range slider attribute
 * to be used inside a Unity inspector. The min/max values
 * consider the FFT size and frequency resolution to mark
 * what frequencies are being selected. The step size of the
 * slider will be equal to the frequency resolution.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * Example usage:
 * [FreqBandSlider(minFreq, maxFreq)]
 * FreqRange freqBand = new FreqRange();
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/

namespace DannyAttributes
{
    using UnityEngine;
    using UnityEditor;

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FreqBandSliderAttribute
     * This class contains the frequencies that will appear in
     * the inspector.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public class FreqBandSliderAttribute : PropertyAttribute
    {
        public readonly float minFreq; // this is the slider's permanent min value
        public readonly float maxFreq; // this is the slider's permanent max value

        public FreqBandSliderAttribute(float min, float max)
        {
            this.minFreq = min;
            this.maxFreq = max;
        }
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FreqRange
     * This class contains the frequencies that will appear in
     * the inspector.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    [System.Serializable]
    public class FreqRange
    {
        public float minFreqSel; // this is the min slider value
        public float maxFreqSel; // this is the max slider value
        public float freqRes; // this value can optionally be updated during runtime

        public void SetResolution(float freqRes)
        {
            if (float.IsNaN(minFreqSel))
                minFreqSel = 100f;
            if (float.IsNaN(maxFreqSel))
                maxFreqSel = 1000f;

            this.freqRes = freqRes;
        }
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FreqBandSliderDrawer
     * This class tells Unity how to update the inspector slider
     * before/during runtime.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FreqBandSliderAttribute))]
    public class FreqBandSliderAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // --------------------------------------------------------------------------------
            // Get the current raw frequency and frequency resolution from the FreqRange variable
            // that the user has selected with the slider range.
            // --------------------------------------------------------------------------------

            SerializedProperty minSelectedProp = property.FindPropertyRelative("minFreqSel");
            SerializedProperty maxSelectedProp = property.FindPropertyRelative("maxFreqSel");
            SerializedProperty freqResProperty = property.FindPropertyRelative("freqRes");

            // --------------------------------------------------------------------------------
            // Check that the properties were found
            // --------------------------------------------------------------------------------

            if (minSelectedProp == null || maxSelectedProp == null || freqResProperty == null)
            {
                Debug.Log("ERROR (FreqBandSliderAttribute.OnGUI): Could not find minFreq, maxFreq properties.");
                return;
            }

            // --------------------------------------------------------------------------------
            // Round the selected frequency to the nearest multiple of the frequency resolution.
            // --------------------------------------------------------------------------------

            float selectedFreqRes = freqResProperty.floatValue;
            float selectedMinFreq = RoundToNearestMultiple(selectedFreqRes, minSelectedProp.floatValue);
            float selectedMaxFreq = RoundToNearestMultiple(selectedFreqRes, maxSelectedProp.floatValue);

            // --------------------------------------------------------------------------------
            // Determine the min and max frequency value.
            // --------------------------------------------------------------------------------

            FreqBandSliderAttribute rangeAttribute = (FreqBandSliderAttribute)attribute;
            float rangeMin = rangeAttribute.minFreq;
            float rangeMax = rangeAttribute.maxFreq;

            // --------------------------------------------------------------------------------
            // Draw text boxes to show user which frequencies they selected based on the 
            // FreqRange property.
            // --------------------------------------------------------------------------------

            const float rangeBoundsLabelWidth = 50f;
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            var rangeBoundsLabel1Rect = new Rect(position);
            rangeBoundsLabel1Rect.width = rangeBoundsLabelWidth;
            GUI.Label(rangeBoundsLabel1Rect, new GUIContent(selectedMinFreq.ToString()));
            position.xMin += rangeBoundsLabelWidth;

            var rangeBoundsLabel2Rect = new Rect(position);
            rangeBoundsLabel2Rect.xMin = rangeBoundsLabel2Rect.xMax - rangeBoundsLabelWidth;
            GUI.Label(rangeBoundsLabel2Rect, new GUIContent(selectedMaxFreq.ToString()));
            position.xMax -= rangeBoundsLabelWidth;

            // --------------------------------------------------------------------------------
            // Adjust the property values in the inspector (write)
            // --------------------------------------------------------------------------------

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, ref selectedMinFreq, ref selectedMaxFreq, rangeMin, rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                minSelectedProp.floatValue = selectedMinFreq;
                maxSelectedProp.floatValue = selectedMaxFreq;
            }
            EditorGUI.EndProperty();
        }

        public float RoundToNearestMultiple(float multipleOf, float rawValue)
        {
            if (multipleOf > Mathf.Epsilon)
                return Mathf.Round(rawValue / multipleOf) * multipleOf;
            else
                return rawValue;
        }
    }
#endif
}
