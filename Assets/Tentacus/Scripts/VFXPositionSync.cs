using UnityEngine;
using UnityEngine.VFX;

public class VFXPositionSync : MonoBehaviour
{
    [Tooltip("场景中的VFX物体")]
    public VisualEffect sceneVFX;
    private int playerNumber;
    private string playerBoolName;
    private string playerPositionName;

    void Start()
    {
        // 在运行时查找场景中的VFX物体
        if (sceneVFX == null)
        {
            sceneVFX = FindObjectOfType<VisualEffect>();
            if (sceneVFX == null)
            {
                Debug.LogError("场景中未找到VFX物体！请确保场景中有一个带有VisualEffect组件的物体。");
                return;
            }
        }

        // 从物体名称中提取玩家编号
        string objectName = gameObject.name;
        if (objectName.StartsWith("Player"))
        {
            string numberStr = objectName.Substring(6); // 去掉"Player"前缀
            if (int.TryParse(numberStr, out playerNumber))
            {
                // 设置对应的参数名称
                playerBoolName = $"isPlayer{playerNumber:D2}";
                playerPositionName = $"Player{playerNumber:D2}";
                
                // 设置对应玩家的bool参数为true
                if (sceneVFX.HasBool(playerBoolName))
                {
                    sceneVFX.SetBool(playerBoolName, true);
                }
                else
                {
                    Debug.LogWarning($"在场景VFX中未找到bool参数: {playerBoolName}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"物体名称 {objectName} 不符合PlayerXX格式！");
        }
    }

    void Update()
    {
        if (sceneVFX != null)
        {
            // 更新对应玩家的位置参数
            if (sceneVFX.HasVector3(playerPositionName))
            {
                sceneVFX.SetVector3(playerPositionName, transform.position);
            }
            else
            {
                Debug.LogWarning($"在场景VFX中未找到Vector3参数: {playerPositionName}");
            }
        }
    }

    void OnDestroy()
    {
        // 当物体被销毁时，将对应的bool参数设置为false
        if (sceneVFX != null && sceneVFX.HasBool(playerBoolName))
        {
            sceneVFX.SetBool(playerBoolName, false);
        }
    }
}
