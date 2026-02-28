using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

//[DisallowMultipleRendererFeature] // once not internal, this needs to be here
public class GlobalVolumeFeature : ScriptableRendererFeature
{
    class GlobalVolumePass : ScriptableRenderPass
    {
        class PassData
        {
            internal VolumeProfile baseProfile;
            internal List<VolumeProfile> qualityProfiles;
        }

        public VolumeProfile _baseProfile;
        public List<VolumeProfile> _qualityProfiles;
        public LayerMask _layerMask;

        public static GameObject volumeHolder;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>("Global Volume Setup", out var passData))
            {
                passData.baseProfile = _baseProfile;
                passData.qualityProfiles = _qualityProfiles;

                builder.AllowPassCulling(false);
                builder.SetRenderFunc(static (PassData data, UnsafeGraphContext context) => ExecutePass(data));
            }
        }

        static void ExecutePass(PassData data)
        {
            if (volumeHolder == null)
            {
                volumeHolder = new GameObject("[DefaultVolume]");
                volumeHolder.hideFlags = HideFlags.HideAndDontSave;

                _staticVol = volumeHolder.AddComponent<Volume>();
                _staticVol.isGlobal = true;
                _staticQualityVol = volumeHolder.AddComponent<Volume>();
                _staticQualityVol.isGlobal = true;
            }

            if (_staticVol != null && data.baseProfile != null)
            {
                _staticVol.sharedProfile = data.baseProfile;
            }

            if (_staticQualityVol != null && data.qualityProfiles != null)
            {
                var index = QualitySettings.GetQualityLevel();
                if (data.qualityProfiles.Count > index && data.qualityProfiles[index] != null)
                    _staticQualityVol.sharedProfile = data.qualityProfiles[index];
            }
        }

        private static Volume _staticVol;
        private static Volume _staticQualityVol;
    }

    GlobalVolumePass m_ScriptablePass;

    public LayerMask _layerMask;
    public VolumeProfile _baseProfile;
    public List<VolumeProfile> _qualityProfiles = new List<VolumeProfile>();

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GlobalVolumePass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
            _baseProfile = this._baseProfile,
            _layerMask = this._layerMask,
            _qualityProfiles = this._qualityProfiles,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (GlobalVolumePass.volumeHolder == null)
        {
            var old = GameObject.Find("[DefaultVolume]");
            if (Application.isPlaying)
            {
                Destroy(old);
            }
            else
            {
                DestroyImmediate(old);
            }
        }
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
