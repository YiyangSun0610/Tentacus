using UnityEngine;
using System.Collections;
using Apple.Core;
using Apple.CoreHaptics;
using System;
using System.Collections.Generic;

public class HapticTest : MonoBehaviour
{
    private CHHapticEngine _engine;
    private bool isInitialized = false;
    private bool isHapticActive = false;
    private Coroutine hapticCoroutine;
    private HapticIntensity currentIntensity = HapticIntensity.Normal;
    
    // 震动强度枚举
    public enum HapticIntensity
    {
        Normal,     // 普通震动（左侧玩家）
        Strong,     // 强烈震动（右侧玩家）
        None        // 无震动（当前玩家）
    }
    
    void Awake()
    {
        // 确保不被销毁
        DontDestroyOnLoad(gameObject);
        Debug.Log($"HapticTest Awake - 设备型号: {SystemInfo.deviceModel}");
    }

    IEnumerator Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Debug.Log("应用启动，准备初始化触觉...");
        Debug.Log($"设备信息：\n型号: {SystemInfo.deviceModel}\n系统版本: {SystemInfo.operatingSystem}\n是否支持触觉: {SystemInfo.supportsVibration}");
        
        // 等待一帧以确保所有系统都已初始化
        yield return new WaitForSeconds(0.5f);

        try {
            // 创建引擎
            _engine = new CHHapticEngine();
            _engine.Start();
            
            isInitialized = true;
            Debug.Log("触觉引擎初始化成功");
        } catch (Exception e) {
            Debug.LogError($"初始化触觉引擎失败: {e.Message}\n{e.StackTrace}");
        }
#else
        Debug.Log("当前平台不支持触觉反馈");
        yield return null;
#endif
    }

    // 启动触觉反馈
    public void StartHaptic(HapticIntensity intensity = HapticIntensity.Normal)
    {
        if (isHapticActive && currentIntensity == intensity) return;
        
        isHapticActive = true;
        currentIntensity = intensity;
        Debug.Log($"开始触觉反馈 - 玩家: {gameObject.name}, 强度: {intensity}");
        
        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
        
        hapticCoroutine = StartCoroutine(PlayHapticContinuously());
    }
    
    // 停止触觉反馈
    public void StopHaptic()
    {
        isHapticActive = false;
        Debug.Log($"停止触觉反馈 - 玩家: {gameObject.name}");
        
        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
            hapticCoroutine = null;
        }
    }

    // 持续播放触觉的协程
    private IEnumerator PlayHapticContinuously()
    {
#if UNITY_IOS && !UNITY_EDITOR
        while (isInitialized && _engine != null && isHapticActive)
        {
            if (currentIntensity == HapticIntensity.None)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // 根据强度选择不同的震动模式
            if (currentIntensity == HapticIntensity.Normal)
            {
                // 普通震动模式
                for (int i = 0; i < 4 && isHapticActive; i++) {
                    PlayStrongHaptic();
                    yield return new WaitForSeconds(0.15f);
                }
                yield return new WaitForSeconds(0.3f);
            }
            else if (currentIntensity == HapticIntensity.Strong)
            {
                // 强烈震动模式
                for (int i = 0; i < 6 && isHapticActive; i++) {
                    PlayStrongHaptic();
                    PlayMultipleHaptic();
                    yield return new WaitForSeconds(0.08f); // 更快的频率
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
#else
        yield return null;
#endif
    }

    // 产生强烈震动的方法
    private void PlayStrongHaptic()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (_engine == null || !isInitialized) return;

        try
        {
            // 创建一个事件 - 使用瞬态类型获得尖锐的效果
            CHHapticEvent hapticEvent = new CHHapticEvent();
            hapticEvent.EventType = CHHapticEventType.HapticTransient;
            hapticEvent.Time = 0;
            
            // 创建参数
            var parameters = new List<CHHapticEventParameter>();
            
            // 添加强度参数 - 根据当前强度调整
            var intensityParameter = new CHHapticEventParameter();
            intensityParameter.ParameterID = CHHapticEventParameterID.HapticIntensity;
            intensityParameter.ParameterValue = currentIntensity == HapticIntensity.Strong ? 1.0f : 0.8f;
            parameters.Add(intensityParameter);
            
            // 添加锐度参数
            var sharpnessParameter = new CHHapticEventParameter();
            sharpnessParameter.ParameterID = CHHapticEventParameterID.HapticSharpness;
            sharpnessParameter.ParameterValue = currentIntensity == HapticIntensity.Strong ? 0.3f : 0.1f;
            parameters.Add(sharpnessParameter);
            
            // 设置参数
            hapticEvent.EventParameters = parameters;
            
            // 创建模式
            CHHapticPattern pattern = new CHHapticPattern();
            
            // 尝试用反射添加事件到模式中
            System.Reflection.MethodInfo method = 
                typeof(CHHapticPattern).GetMethod("set_Events") ?? 
                typeof(CHHapticPattern).GetMethod("AddEvent");
                
            if (method != null) {
                if (method.Name == "set_Events") {
                    method.Invoke(pattern, new object[] { new List<CHHapticEvent> { hapticEvent } });
                } else {
                    method.Invoke(pattern, new object[] { hapticEvent });
                }
            } else {
                pattern = new CHHapticPattern(new List<CHHapticEvent> { hapticEvent });
            }
            
            // 播放模式
            _engine.PlayPattern(pattern);
            if (currentIntensity == HapticIntensity.Strong)
            {
                _engine.PlayPattern(pattern); // 强烈模式下多播放一次
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"播放触觉模式失败: {e.Message}");
        }
#endif
    }
    
    // 产生多重叠加震动
    private void PlayMultipleHaptic()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (_engine == null || !isInitialized) return;

        try
        {
            // 创建一个持续型事件
            CHHapticEvent continuousEvent = new CHHapticEvent();
            continuousEvent.EventType = CHHapticEventType.HapticContinuous;
            continuousEvent.Time = 0;
            
            // 创建参数
            var parameters = new List<CHHapticEventParameter>();
            
            // 添加强度参数
            var intensityParameter = new CHHapticEventParameter();
            intensityParameter.ParameterID = CHHapticEventParameterID.HapticIntensity;
            intensityParameter.ParameterValue = currentIntensity == HapticIntensity.Strong ? 1.0f : 0.8f;
            parameters.Add(intensityParameter);
            
            // 添加锐度参数
            var sharpnessParameter = new CHHapticEventParameter();
            sharpnessParameter.ParameterID = CHHapticEventParameterID.HapticSharpness;
            sharpnessParameter.ParameterValue = currentIntensity == HapticIntensity.Strong ? 0.3f : 0.1f;
            parameters.Add(sharpnessParameter);
            
            // 设置持续时间属性
            try {
                var durationProperty = typeof(CHHapticEvent).GetProperty("Duration");
                if (durationProperty != null) {
                    durationProperty.SetValue(continuousEvent, currentIntensity == HapticIntensity.Strong ? 0.08f : 0.15f);
                }
            } catch {
                // 如果设置失败，忽略错误继续执行
            }
            
            // 设置参数
            continuousEvent.EventParameters = parameters;
            
            // 创建模式
            CHHapticPattern pattern = new CHHapticPattern();
            
            // 尝试用反射添加事件到模式中
            System.Reflection.MethodInfo method = 
                typeof(CHHapticPattern).GetMethod("set_Events") ?? 
                typeof(CHHapticPattern).GetMethod("AddEvent");
                
            if (method != null) {
                if (method.Name == "set_Events") {
                    method.Invoke(pattern, new object[] { new List<CHHapticEvent> { continuousEvent } });
                } else {
                    method.Invoke(pattern, new object[] { continuousEvent });
                }
            } else {
                pattern = new CHHapticPattern(new List<CHHapticEvent> { continuousEvent });
            }
            
            // 播放模式
            _engine.PlayPattern(pattern);
        }
        catch (Exception e)
        {
            Debug.LogError($"播放多重触觉模式失败: {e.Message}");
        }
#endif
    }

    void OnDisable()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (_engine != null)
        {
            _engine.Stop();
            _engine = null;
        }
        isInitialized = false;
#endif
    }

    void OnDestroy()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (_engine != null)
        {
            _engine.Destroy();
            _engine = null;
        }
        isInitialized = false;
#endif
    }
}

