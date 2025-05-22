using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HapticsManager : MonoBehaviour
{
    // 单例实例
    public static HapticsManager Instance { get; private set; }
    
    // 存储所有玩家的HapticTest组件
    private Dictionary<string, HapticTest> playerHaptics = new Dictionary<string, HapticTest>();
    
    // 存储玩家之间的连接关系
    private Dictionary<string, string> playerConnections = new Dictionary<string, string>();
    
    // 存储当前正在震动的玩家
    private HashSet<string> activeHapticPlayers = new HashSet<string>();
    
    // 存储每个玩家被哪些其他玩家触发震动
    private Dictionary<string, HashSet<string>> playerTriggerSources = new Dictionary<string, HashSet<string>>();
    
    private void Awake()
    {
        // 确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 当新玩家加入时调用
    public void OnNewPlayerJoined(GameObject player)
    {
        string playerName = player.name;
        
        // 获取HapticTest组件
        HapticTest hapticTest = player.GetComponent<HapticTest>();
        if (hapticTest == null)
        {
            Debug.LogError($"玩家 {playerName} 没有HapticTest组件");
            return;
        }
        
        // 添加到字典中
        playerHaptics[playerName] = hapticTest;
        
        // 初始化触发源集合
        if (!playerTriggerSources.ContainsKey(playerName))
        {
            playerTriggerSources[playerName] = new HashSet<string>();
        }
        
        // 更新连接关系
        UpdatePlayerConnections();
        
        Debug.Log($"玩家 {playerName} 已加入，当前玩家数量: {playerHaptics.Count}");
    }
    
    // 更新玩家之间的连接关系
    private void UpdatePlayerConnections()
    {
        playerConnections.Clear();
        
        // 获取所有玩家名称并排序（排除amber）
        var playerNames = playerHaptics.Keys.Where(name => !name.ToLower().Contains("amber")).ToList();
        playerNames.Sort((a, b) => {
            // 提取数字部分进行比较
            int aNum = int.Parse(a.Replace("Player", ""));
            int bNum = int.Parse(b.Replace("Player", ""));
            return aNum.CompareTo(bNum);
        });
        
        // 建立连接关系
        for (int i = 0; i < playerNames.Count; i++)
        {
            string currentPlayer = playerNames[i];
            string nextPlayer = playerNames[(i + 1) % playerNames.Count];
            playerConnections[currentPlayer] = nextPlayer;
        }
    }
    
    // 获取指定玩家的左右邻居
    public (string left, string right) GetNeighborNames(string playerName)
    {
        if (!playerHaptics.ContainsKey(playerName))
        {
            Debug.LogError($"玩家 {playerName} 不存在");
            return (null, null);
        }
        
        // 获取所有玩家名称并排序（排除amber）
        var playerNames = playerHaptics.Keys.Where(name => !name.ToLower().Contains("amber")).ToList();
        playerNames.Sort((a, b) => {
            // 提取数字部分进行比较
            int aNum = int.Parse(a.Replace("Player", ""));
            int bNum = int.Parse(b.Replace("Player", ""));
            return aNum.CompareTo(bNum);
        });
        
        // 找到当前玩家的索引
        int currentIndex = playerNames.IndexOf(playerName);
        if (currentIndex == -1)
        {
            Debug.LogError($"无法找到玩家 {playerName} 的索引");
            return (null, null);
        }
        
        // 计算左右邻居的索引
        int leftIndex = (currentIndex - 1 + playerNames.Count) % playerNames.Count;
        int rightIndex = (currentIndex + 1) % playerNames.Count;
        
        return (playerNames[leftIndex], playerNames[rightIndex]);
    }
    
    // 激活邻居玩家的震动
    public void ActivateNeighborHaptics(string neighborName, string sourcePlayerName, HapticTest.HapticIntensity intensity = HapticTest.HapticIntensity.Normal)
    {
        if (!playerHaptics.ContainsKey(neighborName))
        {
            Debug.LogError($"玩家 {neighborName} 不存在");
            return;
        }
        
        // 获取HapticTest组件
        HapticTest hapticTest = playerHaptics[neighborName];
        
        // 启动邻居的震动
        hapticTest.StartHaptic(intensity);
        activeHapticPlayers.Add(neighborName);
        
        // 记录触发源
        if (!playerTriggerSources.ContainsKey(neighborName))
        {
            playerTriggerSources[neighborName] = new HashSet<string>();
        }
        playerTriggerSources[neighborName].Add(sourcePlayerName);
        
        Debug.Log($"激活玩家 {neighborName} 的震动，强度: {intensity}，触发源: {sourcePlayerName}");
    }
    
    // 停用邻居玩家的震动
    public void DeactivateNeighborHaptics(string neighborName, string sourcePlayerName)
    {
        if (!playerHaptics.ContainsKey(neighborName))
        {
            Debug.LogError($"玩家 {neighborName} 不存在");
            return;
        }
        
        // 移除触发源
        if (playerTriggerSources.ContainsKey(neighborName))
        {
            playerTriggerSources[neighborName].Remove(sourcePlayerName);
            
            // 如果没有其他触发源，才停止震动
            if (playerTriggerSources[neighborName].Count == 0)
            {
                // 获取HapticTest组件并停止震动
                HapticTest hapticTest = playerHaptics[neighborName];
                hapticTest.StopHaptic();
                activeHapticPlayers.Remove(neighborName);
                Debug.Log($"停用玩家 {neighborName} 的震动");
            }
            else
            {
                Debug.Log($"玩家 {neighborName} 仍有其他触发源，保持震动状态");
            }
        }
    }
    
    // 停止指定玩家的震动
    public void StopPlayerHaptics(string playerName)
    {
        if (!playerHaptics.ContainsKey(playerName))
        {
            Debug.LogError($"玩家 {playerName} 不存在");
            return;
        }
        
        // 获取HapticTest组件并停止震动
        HapticTest hapticTest = playerHaptics[playerName];
        hapticTest.StopHaptic();
        activeHapticPlayers.Remove(playerName);
        
        // 清除所有触发源
        if (playerTriggerSources.ContainsKey(playerName))
        {
            playerTriggerSources[playerName].Clear();
        }
        
        Debug.Log($"停止玩家 {playerName} 的震动");
    }
    
    // 检查玩家是否正在震动
    public bool IsPlayerHapticActive(string playerName)
    {
        return activeHapticPlayers.Contains(playerName);
    }
    
    // 获取所有玩家（排除amber）
    public List<string> GetAllPlayers()
    {
        return playerHaptics.Keys.Where(name => !name.ToLower().Contains("amber")).ToList();
    }
    
    // 检查指定玩家是否正在触发另一个玩家的震动
    public bool IsPlayerTriggeringHaptics(string sourcePlayer, string targetPlayer)
    {
        if (!playerTriggerSources.ContainsKey(targetPlayer))
        {
            return false;
        }
        return playerTriggerSources[targetPlayer].Contains(sourcePlayer);
    }
} 