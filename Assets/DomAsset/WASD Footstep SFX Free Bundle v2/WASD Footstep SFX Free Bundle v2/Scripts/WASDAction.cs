using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WASDSound;

namespace WASDSound
{
    [CreateAssetMenu(fileName = "New Action", menuName = "WASD/Action")]
    public class WASDAction : ScriptableObject
    {
        public List<WASDMaterial> materials;

        public AudioClip GetMaterialClip(WASDEnumMaterial material)
        {
            return material switch
            {
                WASDEnumMaterial.Dirt   => materials[0].GetRandomClip(),
                WASDEnumMaterial.Grass  => materials[1].GetRandomClip(),
                WASDEnumMaterial.Stone  => materials[2].GetRandomClip(),
                WASDEnumMaterial.Wood   => materials[3].GetRandomClip(),
                _                       => materials[2].GetRandomClip(),
            };
        }
    }
}
