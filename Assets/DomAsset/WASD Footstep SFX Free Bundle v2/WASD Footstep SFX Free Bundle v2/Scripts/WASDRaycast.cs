using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WASDSound;

namespace WASDSound
{
    public class WASDRaycast : MonoBehaviour
    {
        public List<Material> Dirt = new List<Material>();
        public List<Material> Grass = new List<Material>();
        public List<Material> Stone = new List<Material>();
        public List<Material> Wood = new List<Material>();
        public List<Material> Silent = new List<Material>();

        private Material lastMaterial = null;

        public UnityEvent onNewMaterial;
        
        Dictionary<Material, int> materialDic;

        private void FixedUpdate()
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position + (Vector3.forward * 0.3f) + (Vector3.up * 0.1f), transform.TransformDirection(-Vector3.up) * 2, Color.yellow);

            if (Physics.Raycast(transform.position + (Vector3.forward * 0.3f) + (Vector3.up * 0.1f), transform.TransformDirection(-Vector3.up), out hit, 2))
            {
                var hitMaterial = hit.transform.GetComponent<Renderer>();

                if (hitMaterial.sharedMaterial == lastMaterial)
                {
                    return;
                }

                foreach(Material mat in Dirt)
                {
                    if (mat == hitMaterial.sharedMaterial)
                    {
                        if (Dirt == null) return;
                        lastMaterial = mat;
                        gameObject.GetComponent<WASDFootstepSource>().SetMaterial(WASDEnumMaterial.Dirt);
                        onNewMaterial.Invoke();
                        return;
                    }
                }

                foreach (Material mat in Grass)
                {
                    if (mat == hitMaterial.sharedMaterial)
                    {
                        if (Grass == null) return;
                        lastMaterial = mat;
                        gameObject.GetComponent<WASDFootstepSource>().SetMaterial(WASDEnumMaterial.Grass);
                        onNewMaterial.Invoke();
                        return;
                    }
                }

                foreach (Material mat in Stone)
                {
                    if (mat == hitMaterial.sharedMaterial)
                    {
                        if (Stone == null) return;
                        lastMaterial = mat;
                        gameObject.GetComponent<WASDFootstepSource>().SetMaterial(WASDEnumMaterial.Stone);
                        onNewMaterial.Invoke();
                        return;
                    }
                }

                foreach (Material mat in Wood)
                {
                    if (mat == hitMaterial.sharedMaterial)
                    {
                        if (Wood == null) return;
                        lastMaterial = mat;
                        gameObject.GetComponent<WASDFootstepSource>().SetMaterial(WASDEnumMaterial.Wood);
                        onNewMaterial.Invoke();
                        return;
                    }
                }

                switch (materialDic[hitMaterial.sharedMaterial])
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }
            }
        }


        void SetupMaterialDic()
        {
            materialDic.Clear();

            foreach (Material mat in Dirt)
            {
                materialDic.Add(mat, 0);
            }

            foreach (Material mat in Grass)
            {
                materialDic.Add(mat, 1);
            }

            foreach (Material mat in Stone)
            {
                materialDic.Add(mat, 2);
            }

            foreach (Material mat in Wood)
            {
                materialDic.Add(mat, 3);
            }
        }
    }
}
