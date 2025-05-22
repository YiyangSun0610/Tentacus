// SPDX-FileCopyrightText: Copyright 2023 Reality Design Lab <dev@reality.design>
// SPDX-FileContributor: Yuchen Zhang <yuchenz27@outlook.com>
// SPDX-License-Identifier: MIT

using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace HoloKit.ColocatedMultiplayerBoilerplate
{
    public class HoloKitMarkManager : MonoBehaviour
    {
        [SerializeField] private HoloKitMarkController m_HoloKitMarkPrefab;
        private HoloKitMarkController m_HoloKitMark;
        private string m_PlayerName;
        private bool m_IsNameCorrectionActive = true;

        private void Start()
        {
            // 获取当前连接的客户端数量（包括主机）
            int playerNumber = NetworkManager.Singleton.ConnectedClientsIds.Count;
            // 第一个设备命名为 amber，后续设备按 Player01、Player02 等格式命名
            m_PlayerName = playerNumber == 1 ? "amber" : $"Player{(playerNumber - 1):D2}";
            
            // 实例化预制体
            m_HoloKitMark = Instantiate(m_HoloKitMarkPrefab);
            m_HoloKitMark.PlayerPoseSynchronizer = transform;
            
            // 立即设置名称
            m_HoloKitMark.gameObject.name = m_PlayerName;
            
            // 添加必要的组件
            if (playerNumber > 1) // 只为Player01及以后的玩家添加组件
            {
                // 添加碰撞器设置
                var colliderSetup = m_HoloKitMark.gameObject.GetComponent<CircleColliderSetup>();
                if (colliderSetup == null)
                {
                    colliderSetup = m_HoloKitMark.gameObject.AddComponent<CircleColliderSetup>();
                }
                
                // 添加碰撞检测器
                var collisionDetector = m_HoloKitMark.gameObject.GetComponent<PlayerCollisionDetector>();
                if (collisionDetector == null)
                {
                    collisionDetector = m_HoloKitMark.gameObject.AddComponent<PlayerCollisionDetector>();
                }
                
                // 添加触觉控制器
                var hapticTest = m_HoloKitMark.gameObject.GetComponent<HapticTest>();
                if (hapticTest == null)
                {
                    hapticTest = m_HoloKitMark.gameObject.AddComponent<HapticTest>();
                }
                
                // 设置Player标签
                m_HoloKitMark.gameObject.tag = "Player";
                
                Debug.Log($"已为玩家 {m_PlayerName} 设置所有必要组件");
            }
            
            // 启动名称修正协程
            StartCoroutine(EnsureCorrectName());
        }
        
        private void Update()
        {
            // 持续监控并修正名称
            if (m_IsNameCorrectionActive && m_HoloKitMark != null && m_HoloKitMark.gameObject.name != m_PlayerName)
            {
                m_HoloKitMark.gameObject.name = m_PlayerName;
                Debug.Log($"已强制修正玩家名称为 {m_PlayerName}");
            }
        }
        
        private IEnumerator EnsureCorrectName()
        {
            // 等待几帧，确保其他系统完成初始化
            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }
            
            // 检查并修正名称
            if (m_HoloKitMark != null && m_HoloKitMark.gameObject.name != m_PlayerName)
            {
                m_HoloKitMark.gameObject.name = m_PlayerName;
                Debug.Log($"已修正玩家名称为 {m_PlayerName}");
            }
            
            // 继续监控一段时间
            float monitoringTime = 5f;
            float elapsedTime = 0f;
            
            while (elapsedTime < monitoringTime)
            {
                if (m_HoloKitMark != null && m_HoloKitMark.gameObject.name != m_PlayerName)
                {
                    m_HoloKitMark.gameObject.name = m_PlayerName;
                    Debug.Log($"持续监控中：已修正玩家名称为 {m_PlayerName}");
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 监控期结束后，如果名称仍然不正确，保持Update中的持续监控
            if (m_HoloKitMark != null && m_HoloKitMark.gameObject.name != m_PlayerName)
            {
                Debug.LogWarning($"名称监控期结束，但名称仍未正确。将保持持续监控。");
            }
            else
            {
                m_IsNameCorrectionActive = false;
                Debug.Log("名称已稳定，停止持续监控。");
            }
        }

        public void OnDestroy()
        {
            if (m_HoloKitMark)
                Destroy(m_HoloKitMark.gameObject);
        }
    }
}
