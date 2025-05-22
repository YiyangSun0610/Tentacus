using UnityEngine;
using UnityEngine.VFX;

public class VFXMonitor : MonoBehaviour
{
    public VisualEffect vfx; // 连接到 VFX 组件
    public string ageAttribute = "Age"; // 在 VFX Graph 中获取的属性名，通常是 "Age" 或类似名称

    void Update()
    {
        // 获取 VFX Graph 中粒子的年龄属性
        if (vfx.HasFloat(ageAttribute))
        {
            float particleAge = vfx.GetFloat(ageAttribute);
            float result = particleAge - 0.01f;
            Debug.Log("Particle Age: " + particleAge + ", Result after subtracting 0.01: " + result);
        }
    }
}
