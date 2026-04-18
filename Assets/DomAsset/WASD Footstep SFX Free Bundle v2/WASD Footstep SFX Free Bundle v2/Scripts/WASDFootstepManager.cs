using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WASDSound;

namespace WASDSound
{
    [CreateAssetMenu(fileName = "New Footstep Manager", menuName = "WASD/FootstepManager")]
    public class WASDFootstepManager : ScriptableObject
    {
        public List<WASDAction> WASDFootsteps;

        public AudioClip GetAudioClip(WASDEnumAction action, WASDEnumMaterial material)
        {
            return action switch
            {
                WASDEnumAction.Drop     => WASDFootsteps[0].GetMaterialClip(material),
                WASDEnumAction.Jump     => WASDFootsteps[1].GetMaterialClip(material),
                WASDEnumAction.Run      => WASDFootsteps[2].GetMaterialClip(material),
                WASDEnumAction.Shuffle  => WASDFootsteps[3].GetMaterialClip(material),
                WASDEnumAction.Sneak    => WASDFootsteps[4].GetMaterialClip(material),
                WASDEnumAction.Walk     => WASDFootsteps[5].GetMaterialClip(material),
                _                       => WASDFootsteps[5].GetMaterialClip(material),
            };
        }





        ///---------------------------------
        private Dictionary<WASDAction, WASDMaterial> lookup;

        // Build the dictionary when the asset is loaded or changed
        private void OnEnable()
        {
            lookup = new Dictionary<WASDAction, WASDMaterial>();

            foreach (var entry in WASDFootsteps)
            {
                /*if (entry != null && entry.Material != null && !lookup.ContainsKey(entry.Material))
                {
                    lookup.Add(entry.Material, entry);
                }*/
            }
        }
    }
}

