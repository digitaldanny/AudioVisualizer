using System.Collections.Generic;
using UnityEngine;

namespace DataStruct
{
    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: EasyList
     * This class wraps the List<T> class to allow user to easily 
     * grow and shrink the size of the container for runtime 
     * user configuration.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public class EasyList<T>
    {
        private List<T> list;
        private int maxSize;
        private T defaultValue;

        // constructor
        public EasyList(int maxSize, T defaultValue)
        {
            list = new List<T>();

            // limit how large the list can get to avoid running out of memory
            this.maxSize = maxSize;

            // create a default value that new values are initialized to.
            this.defaultValue = defaultValue;
        }

        // Checks if the requested resize is too large.
        public bool IsResizeSafe(int size)
        {
            if (size > this.maxSize)
                return false;
            return true;
        }

        // User can request the size of the container during runtime.
        // ..
        // NOTE: This will return false if the requested size is larger
        // than the max size defined upon instantiation.
        public bool Resize(int size)
        {
            // Make sure the user is not requesting a size that is too large.
            if (!this.IsResizeSafe(size))
                return false;

            // Add 0s to the end if requesting a larger size.
            if (list.Count < size)
            {
                while (list.Count < size) { list.Add(this.defaultValue); }
                Debug.Log(list[0]);
            }

            // Truncate the list if requesting a smaller size.
            else
            {
                while (list.Count > size) { list.RemoveAt(this.list.Count-1); }
            }
            return true;
        }

        // User can read+write list values using brackets (eg. list[0])
        public object this[int i]
        {
            get { return this.list[i]; }
            set { this.list[i] = (T)value; }
        }

        // Gets the current size of the list
        public int GetCount() { return this.list.Count; }
    }

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: BinSamples
     * This class contains left (L) and right (R) bin samples for
     * a selected audio clip that can dynamically grow or shrink
     * based on runtime configuration.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public class BinStereo
    {
        public EasyList<float> L;
        public EasyList<float> R;
        public int size;

        // constructor - user defines a max size that the container is not
        // allowed to go above during runtime.
        public BinStereo(int max_size)
        {
            L = new EasyList<float>(max_size, 0f);
            R = new EasyList<float>(max_size, 0f);
            size = 0;
        }

        // Resize FFT list to appropriate size. Shrinking results in
        // truncation, while growth results in adding 0s to the end.
        public bool ResizeFFT(int fft_size)
        {
            // check that resize is safe before resizing
            if (!L.IsResizeSafe(fft_size))
                return false;

            L.Resize(fft_size); 
            R.Resize(fft_size);
            size = fft_size;
            return true;
        }
    }
}
