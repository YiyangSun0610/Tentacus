using UnityEngine;
using UnityEngine.VFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VFXAlphaController : MonoBehaviour
{
    public VisualEffect vfx;
    public string alphaParameter = "AlphaChange";
    public float transitionDuration = 1f;
    [Tooltip("VFX开始播放后等待多少秒才开始Alpha渐变")]
    public float startDelay = 0f;

    [SerializeField]
    private bool previewInEditor = false;

    private bool isPlaying = false;
    private bool isWaitingForDelay = false;
    private float currentAlpha = 0f;
    private float timer = 0f;
    private float delayTimer = 0f;

    void Start()
    {
        if (vfx == null)
        {
            vfx = GetComponent<VisualEffect>();
        }
        // 确保初始Alpha值为0
        ResetAlpha();
    }

    void Update()
    {
        #if UNITY_EDITOR
        // 在编辑器模式下，使用previewInEditor来控制效果
        if (!Application.isPlaying)
        {
            if (previewInEditor && !isPlaying && !isWaitingForDelay)
            {
                StartTransition();
            }
            else if (!previewInEditor && (isPlaying || isWaitingForDelay))
            {
                ResetAlpha();
                isPlaying = false;
                isWaitingForDelay = false;
                timer = 0f;
                delayTimer = 0f;
            }
        }
        #endif

        // 正常运行时的逻辑
        if (Application.isPlaying)
        {
            bool currentlyPlaying = vfx.aliveParticleCount > 0;
            if (currentlyPlaying && !isPlaying && !isWaitingForDelay)
            {
                if (startDelay > 0)
                {
                    isWaitingForDelay = true;
                    delayTimer = 0f;
                }
                else
                {
                    StartTransition();
                }
            }
        }

        // 处理延迟
        if (isWaitingForDelay)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= startDelay)
            {
                isWaitingForDelay = false;
                StartTransition();
            }
        }

        // 执行渐变
        if (isPlaying && timer < transitionDuration)
        {
            timer += Time.deltaTime;
            currentAlpha = Mathf.Lerp(0f, 1f, timer / transitionDuration);
            
            if (vfx.HasFloat(alphaParameter))
            {
                vfx.SetFloat(alphaParameter, currentAlpha);
            }
        }
    }

    public void StartTransition()
    {
        isPlaying = true;
        isWaitingForDelay = false;
        timer = 0f;
        delayTimer = 0f;
    }

    private void ResetAlpha()
    {
        if (vfx != null && vfx.HasFloat(alphaParameter))
        {
            vfx.SetFloat(alphaParameter, 0f);
            currentAlpha = 0f;
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && vfx != null)
        {
            if (!previewInEditor)
            {
                ResetAlpha();
                isPlaying = false;
                isWaitingForDelay = false;
                timer = 0f;
                delayTimer = 0f;
            }
            // 确保延迟时间不为负数
            startDelay = Mathf.Max(0f, startDelay);
        }
    }
    #endif
} 