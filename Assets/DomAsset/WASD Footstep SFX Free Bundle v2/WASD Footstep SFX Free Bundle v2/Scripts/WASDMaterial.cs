using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WASDSound;

namespace WASDSound
{
    [CreateAssetMenu(fileName = "New Material", menuName = "WASD/Material")]
    public class WASDMaterial : ScriptableObject
    {
        public List<AudioClip> clips;
        private List<int> ShuffleSeed = new List<int>();
        private int shuffle = 0;

        public AudioClip GetRandomClip()
        {
            int randomClip = ShuffleSeed[shuffle];
            shuffle++;
            if (shuffle > 5)
            {
                ShuffleList();
                shuffle = 0;
            }

            return clips[randomClip];
        }

        void ShuffleList()
        {
            int lastNumInArray = ShuffleSeed[^1];

            for (int i = 0; i < 6; i++)
            {
                int temp = ShuffleSeed[i];
                int r = Random.Range(i, ShuffleSeed.Count);
                ShuffleSeed[i] = ShuffleSeed[r];
                ShuffleSeed[r] = temp;
            }

            //Avoid doubles on last number in array and first number after new order
            if (ShuffleSeed[0] == lastNumInArray)
            {
                int temp = ShuffleSeed[^1];
                ShuffleSeed[0] = ShuffleSeed[1];
                ShuffleSeed[0] = temp;
            }
        }

        private void OnEnable()
        {
            for (int i=0;i<6;i++)
            {
                ShuffleSeed.Add(i);
            }
        }

    }
}
