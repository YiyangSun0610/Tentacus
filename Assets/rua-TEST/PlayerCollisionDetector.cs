using UnityEngine;
using System.Collections;

public class PlayerCollisionDetector : MonoBehaviour
{
    // 目标物体的标签
    [Tooltip("需要检测碰撞的目标物体的标签")]
    public string targetTag = "InteractiveObject";
    
    // 当前是否在与目标物体碰撞
    private bool isColliding = false;
    
    // 存储标准化的玩家名称
    private string standardizedName;
    
    // 碰撞检测的精度设置
    [Header("碰撞检测设置")]
    [Tooltip("碰撞检测的精度（值越小越精确，但性能消耗越大）")]
    public float collisionCheckInterval = 0.1f;
    
    [Tooltip("碰撞检测的射线长度")]
    public float raycastDistance = 0.5f;
    
    private void Awake()
    {
        // 获取CircleColliderSetup组件
        CircleColliderSetup setup = GetComponent<CircleColliderSetup>();
        if (setup != null)
        {
            // 等待一帧，让CircleColliderSetup完成名称修改
            StartCoroutine(WaitForNameStandardization());
        }
    }
    
    private IEnumerator WaitForNameStandardization()
    {
        yield return null; // 等待一帧
        standardizedName = gameObject.name;
        Debug.Log($"玩家 {standardizedName} 已标准化名称");
        
        // 启动碰撞检测协程
        StartCoroutine(CheckCollisionContinuously());
    }
    
    private void Start()
    {
        // 确保有碰撞体
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"玩家 {gameObject.name} 没有碰撞体组件，请添加碰撞体");
        }
        
        // 通知HapticsManager新玩家加入
        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.OnNewPlayerJoined(gameObject);
        }
    }
    
    private IEnumerator CheckCollisionContinuously()
    {
        while (true)
        {
            // 检查周围是否有交互物体
            Collider[] colliders = Physics.OverlapSphere(transform.position, raycastDistance);
            bool foundInteractiveObject = false;
            
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    foundInteractiveObject = true;
                    break;
                }
            }
            
            // 更新碰撞状态
            if (foundInteractiveObject && !isColliding)
            {
                isColliding = true;
                // 确保自己不震动
                if (HapticsManager.Instance != null)
                {
                    HapticsManager.Instance.StopPlayerHaptics(standardizedName);
                }
                ActivateNeighborHaptics();
            }
            else if (!foundInteractiveObject && isColliding)
            {
                isColliding = false;
                DeactivateNeighborHaptics();
            }
            
            yield return new WaitForSeconds(collisionCheckInterval);
        }
    }
    
    // 激活邻居玩家的震动
    private void ActivateNeighborHaptics()
    {
        if (HapticsManager.Instance == null)
        {
            Debug.LogError("HapticsManager实例不存在");
            return;
        }
        
        // 获取当前玩家的编号
        int currentPlayerNumber = GetPlayerNumber(standardizedName);
        
        // 如果不是Player01及以后的玩家，不处理
        if (currentPlayerNumber <= 0)
        {
            return;
        }
        
        // 获取邻居名称
        var (leftNeighbor, rightNeighbor) = HapticsManager.Instance.GetNeighborNames(standardizedName);
        
        if (leftNeighbor == null || rightNeighbor == null)
        {
            Debug.LogError($"无法获取玩家 {standardizedName} 的邻居");
            return;
        }
        
        // 获取左侧和右侧玩家的编号
        int leftPlayerNumber = GetPlayerNumber(leftNeighbor);
        int rightPlayerNumber = GetPlayerNumber(rightNeighbor);
        
        // 如果邻居不是Player01及以后的玩家，跳过该邻居
        if (leftPlayerNumber <= 0)
        {
            leftNeighbor = null;
        }
        if (rightPlayerNumber <= 0)
        {
            rightNeighbor = null;
        }
        
        // 根据玩家编号关系设置震动强度
        if (leftNeighbor != null)
        {
            if (leftPlayerNumber < currentPlayerNumber)
            {
                // 左侧玩家编号更小，使用普通震动
                HapticsManager.Instance.ActivateNeighborHaptics(leftNeighbor, standardizedName, HapticTest.HapticIntensity.Normal);
            }
            else
            {
                // 左侧玩家编号更大，使用强烈震动
                HapticsManager.Instance.ActivateNeighborHaptics(leftNeighbor, standardizedName, HapticTest.HapticIntensity.Strong);
            }
        }
        
        if (rightNeighbor != null)
        {
            if (rightPlayerNumber < currentPlayerNumber)
            {
                // 右侧玩家编号更小，使用普通震动
                HapticsManager.Instance.ActivateNeighborHaptics(rightNeighbor, standardizedName, HapticTest.HapticIntensity.Normal);
            }
            else
            {
                // 右侧玩家编号更大，使用强烈震动
                HapticsManager.Instance.ActivateNeighborHaptics(rightNeighbor, standardizedName, HapticTest.HapticIntensity.Strong);
            }
        }
        
        Debug.Log($"玩家 {standardizedName} 触发了碰撞，激活玩家 {leftNeighbor} 和 {rightNeighbor} 的震动");
    }
    
    // 停用邻居玩家的震动
    private void DeactivateNeighborHaptics()
    {
        if (HapticsManager.Instance == null)
        {
            Debug.LogError("HapticsManager实例不存在");
            return;
        }
        
        // 获取当前玩家的编号
        int currentPlayerNumber = GetPlayerNumber(standardizedName);
        
        // 如果不是Player01及以后的玩家，不处理
        if (currentPlayerNumber <= 0)
        {
            return;
        }
        
        // 获取邻居名称
        var (leftNeighbor, rightNeighbor) = HapticsManager.Instance.GetNeighborNames(standardizedName);
        
        if (leftNeighbor == null || rightNeighbor == null)
        {
            Debug.LogError($"无法获取玩家 {standardizedName} 的邻居");
            return;
        }
        
        // 获取左侧和右侧玩家的编号
        int leftPlayerNumber = GetPlayerNumber(leftNeighbor);
        int rightPlayerNumber = GetPlayerNumber(rightNeighbor);
        
        // 如果邻居不是Player01及以后的玩家，跳过该邻居
        if (leftPlayerNumber <= 0)
        {
            leftNeighbor = null;
        }
        if (rightPlayerNumber <= 0)
        {
            rightNeighbor = null;
        }
        
        // 检查邻居是否仍然被其他玩家触发震动
        if (leftNeighbor != null)
        {
            bool shouldStopLeft = true;
            // 检查其他玩家是否仍在触发该邻居的震动
            foreach (var player in HapticsManager.Instance.GetAllPlayers())
            {
                if (player != standardizedName && HapticsManager.Instance.IsPlayerTriggeringHaptics(player, leftNeighbor))
                {
                    shouldStopLeft = false;
                    break;
                }
            }
            if (shouldStopLeft)
            {
                HapticsManager.Instance.DeactivateNeighborHaptics(leftNeighbor, standardizedName);
            }
        }
        
        if (rightNeighbor != null)
        {
            bool shouldStopRight = true;
            // 检查其他玩家是否仍在触发该邻居的震动
            foreach (var player in HapticsManager.Instance.GetAllPlayers())
            {
                if (player != standardizedName && HapticsManager.Instance.IsPlayerTriggeringHaptics(player, rightNeighbor))
                {
                    shouldStopRight = false;
                    break;
                }
            }
            if (shouldStopRight)
            {
                HapticsManager.Instance.DeactivateNeighborHaptics(rightNeighbor, standardizedName);
            }
        }
        
        Debug.Log($"玩家 {standardizedName} 停止了碰撞，检查并更新玩家 {leftNeighbor} 和 {rightNeighbor} 的震动状态");
    }
    
    // 从玩家名称中提取编号
    private int GetPlayerNumber(string playerName)
    {
        if (string.IsNullOrEmpty(playerName)) return -1;
        
        // 尝试从名称中提取数字
        string numberStr = playerName.Replace("Player", "");
        if (int.TryParse(numberStr, out int number))
        {
            return number;
        }
        
        return -1;
    }
    
    // 在编辑器中可视化碰撞检测范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raycastDistance);
    }
} 