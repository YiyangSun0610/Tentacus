using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;

public class VFXPlatformManager : MonoBehaviour
{
    [SerializeField] private VisualEffect[] vfxEffects;
    [SerializeField] private Volume[] globalVolumes;
    
    private void Awake()
    {
        #if UNITY_IOS
        DisableVFXOnMobile();
        DisableGlobalVolumeOnMobile();
        #endif
    }

    private void DisableVFXOnMobile()
    {
        if (vfxEffects == null || vfxEffects.Length == 0)
        {
            Debug.LogWarning("No VFX effects assigned to VFXPlatformManager");
            return;
        }

        foreach (var vfx in vfxEffects)
        {
            if (vfx != null)
            {
                vfx.enabled = false;
            }
        }
    }

    private void DisableGlobalVolumeOnMobile()
    {
        if (globalVolumes == null || globalVolumes.Length == 0)
        {
            Debug.LogWarning("No Global Volumes assigned to VFXPlatformManager");
            return;
        }

        foreach (var volume in globalVolumes)
        {
            if (volume != null)
            {
                volume.enabled = false;
            }
        }
    }
} 