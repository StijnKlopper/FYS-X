using System;
using UnityEngine;

namespace UnityStandardAssets.Water
{
    [RequireComponent(typeof(WaterBase))]
    [ExecuteInEditMode]
    public class SpecularLighting : MonoBehaviour
    {
        public Transform SpecularLight;
        private WaterBase m_WaterBase;

        public void Start()
        {
            m_WaterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
        }

        public void Update()
        {
            if (!m_WaterBase)
            {
                m_WaterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
            }

            if (SpecularLight && m_WaterBase.SharedMaterial)
            {
                m_WaterBase.SharedMaterial.SetVector("_WorldLightDir", SpecularLight.transform.forward);
            }
        }
    }
}