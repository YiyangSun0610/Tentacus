using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CircleColliderSetup))]
public class CircleColliderSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 显示默认的Inspector
        DrawDefaultInspector();
        
        CircleColliderSetup setup = (CircleColliderSetup)target;
        
        EditorGUILayout.Space();
        
        // 添加手动设置按钮
        if (GUILayout.Button("手动设置碰撞器"))
        {
            // 直接调用SetupCollider方法
            var method = setup.GetType().GetMethod("SetupCollider", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (method != null)
            {
                method.Invoke(setup, null);
            }
            else
            {
                Debug.LogError("无法找到SetupCollider方法");
            }
        }
    }
} 