using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(HapticsManager))]
public class HapticsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 显示默认的Inspector
        DrawDefaultInspector();
        
        HapticsManager manager = (HapticsManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("玩家状态", EditorStyles.boldLabel);
        
        // 获取所有带有Player标签的对象
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            EditorGUILayout.HelpBox("场景中没有找到带有'Player'标签的玩家对象", MessageType.Warning);
        }
        else
        {
            foreach (GameObject player in players)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(player.name);
                
                // 检查玩家是否已初始化
                bool isInitialized = false;
                var method = manager.GetType().GetMethod("IsPlayerInitialized", 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (method != null)
                {
                    isInitialized = (bool)method.Invoke(manager, new object[] { player.name });
                }
                
                if (isInitialized)
                {
                    EditorGUILayout.LabelField("已初始化", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("未初始化", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        // 添加手动初始化按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("手动初始化所有玩家"))
        {
            var method = manager.GetType().GetMethod("InitializeHaptics", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (method != null)
            {
                method.Invoke(manager, null);
            }
            else
            {
                Debug.LogError("无法找到InitializeHaptics方法");
            }
        }
        
        // 添加说明
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "此管理器会自动初始化所有带有'Player'标签的玩家对象。\n" +
            "确保所有玩家预制体都添加了以下组件：\n" +
            "1. CircleColliderSetup\n" +
            "2. PlayerCollisionDetector\n" +
            "3. HapticTest", 
            MessageType.Info);
    }
} 