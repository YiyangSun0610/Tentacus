using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 该脚本用于快速在Unity编辑器中设置玩家圆形碰撞器
public class CircleColliderSetup : MonoBehaviour
{
    [Header("碰撞器设置")]
    [Tooltip("圆形碰撞器的半径")]
    public float colliderRadius = 1.0f;
    
    [Tooltip("碰撞器的Y轴偏移（相对于玩家位置）")]
    public float colliderYOffset = 0.1f;
    
    [Tooltip("碰撞器的触发器模式")]
    public bool isTrigger = true;
    
    [Header("交互物体设置")]
    [Tooltip("互动物体的标签")]
    public string interactiveObjectTag = "InteractiveObject";
    
    [Header("调试")]
    [Tooltip("开启Gizmos绘制")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmos颜色")]
    public Color gizmosColor = new Color(0, 1, 0, 0.5f);
    
    private void Start()
    {
        // 检查是否是Player01及以后的玩家
        if (IsPlayerNumbered())
        {
            // 自动设置碰撞器
            SetupCollider();
        }
    }
    
    // 检查是否是Player01及以后的玩家
    private bool IsPlayerNumbered()
    {
        string name = gameObject.name;
        // 检查名称是否以"Player"开头，后面跟着数字
        if (name.StartsWith("Player"))
        {
            string numberPart = name.Substring(6); // 去掉"Player"前缀
            return int.TryParse(numberPart, out _);
        }
        return false;
    }
    
    // 设置当前物体的碰撞器
    private void SetupCollider()
    {
        // 查找或添加碰撞器
        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        
        // 设置碰撞器属性
        collider.radius = colliderRadius;
        collider.center = new Vector3(0, colliderYOffset, 0);
        collider.isTrigger = isTrigger;
        
        // 添加或更新碰撞检测器组件
        PlayerCollisionDetector detector = gameObject.GetComponent<PlayerCollisionDetector>();
        if (detector == null)
        {
            detector = gameObject.AddComponent<PlayerCollisionDetector>();
        }
        
        // 设置检测器的目标标签
        detector.targetTag = interactiveObjectTag;
        
        Debug.Log($"为玩家 {gameObject.name} 设置了碰撞器和检测器");
        
        // 确保玩家有正确的标签
        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }
    }
    
    // 在场景视图中绘制碰撞器
    private void OnDrawGizmos()
    {
        if (!showGizmos || !IsPlayerNumbered()) return;
        
        Gizmos.color = gizmosColor;
        Vector3 colliderCenter = transform.position + new Vector3(0, colliderYOffset, 0);
        Gizmos.DrawSphere(colliderCenter, colliderRadius);
    }
} 