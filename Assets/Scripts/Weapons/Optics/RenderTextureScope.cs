using Starlight.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight.Weapons.Optics
{
    public class RenderTextureScope : MonoBehaviour, IManagedBehaviour
    {
        BaseWeapon bw;
        [SerializeField] Camera cam;
        [SerializeField] Renderer rend;
        [SerializeField] Material mat;
        [SerializeField] CustomRenderTexture crt;
        [SerializeField, Range(1, 5)] int scopeResolution;
        [SerializeField] int trueResolution;
        [SerializeField] bool manualUpdate;
        [SerializeField] bool useFixedUpdate;

        [SerializeField, Header("Zoom"), Range(1, 20)] float zoomLevel;
        [SerializeField] float defaultFocalLength;
        [SerializeField] int baseZoomAmount;
        public void ManagedFixedUpdate()
        {
            if(useFixedUpdate && manualUpdate)
            {
                crt.Update();
            }
        }

        public void ManagedLateUpdate()
        {
            bool viewable = bw.CM_Focus > 0.4f;
            cam.enabled = viewable;
            rend.enabled = viewable;
            if (!useFixedUpdate && manualUpdate)
            {
                crt.Update();
            }
            cam.focalLength = defaultFocalLength * (baseZoomAmount * zoomLevel);
        }

        public void ManagedUpdate()
        {
        }

        private void Awake()
        {
            BehaviourManager.Subscribe(ManagedUpdate, ManagedLateUpdate, ManagedFixedUpdate);
            bw = GetComponentInParent<BaseWeapon>();
            CreateCRT();
            rend.material = new(mat)
            {
                mainTexture = crt
            };
        }
        private void Start()
        {
            bw.CM.SetZoomLevel(zoomLevel * baseZoomAmount);
        }
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                trueResolution = (int)System.Math.Pow(2, scopeResolution) * 128;
            }
            else
            {
                if (bw.CM)
                {
                    bw.CM.SetZoomLevel(zoomLevel * baseZoomAmount);
                }
            }
        }
        void CreateCRT()
        {
            
            int size = trueResolution;
            crt = new(size,size, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Default);
            cam.targetTexture = crt;
            crt.updateMode = CustomRenderTextureUpdateMode.OnDemand;
            crt.Create();
            crt.Initialize();
        }
    }
}