// SPDX-FileCopyrightText: Copyright 2023 Reality Design Lab <dev@reality.design>
// SPDX-FileContributor: Yuchen Zhang <yuchenz27@outlook.com>
// SPDX-License-Identifier: MIT

using UnityEngine;
using HoloKit;
using Unity.Netcode;

namespace HoloKit.ColocatedMultiplayerBoilerplate
{
    public class HoloKitMarkController : MonoBehaviour
    {
        public Transform PlayerPoseSynchronizer { get; set; }

        [SerializeField] private Vector3 m_Offset = new(0f, 0.15f, 0f);

        private Transform m_CenterEyePose;
        private string m_PlayerName;
        private bool m_IsNameSet = false;

        private void Awake()
        {
            // 在Awake中设置名称，确保在其他系统之前执行
            SetPlayerName();
        }

        private void Start()
        {
            m_CenterEyePose = FindObjectOfType<HoloKitCameraManager>().CenterEyePose;
            
            // 再次确保名称正确
            if (!m_IsNameSet)
            {
                SetPlayerName();
            }
        }

        private void SetPlayerName()
        {
            if (m_IsNameSet) return;

            // 获取当前连接的客户端数量（包括主机）
            int playerNumber = NetworkManager.Singleton.ConnectedClientsIds.Count;
            // 第一个设备命名为 amber，后续设备按 Player01、Player02 等格式命名
            m_PlayerName = playerNumber == 1 ? "amber" : $"Player{(playerNumber - 1):D2}";
            
            // 设置名称
            gameObject.name = m_PlayerName;
            m_IsNameSet = true;
            
            // 为所有玩家添加碰撞器和碰撞检测器
            SetupColliderAndDetector();
            
            // 只为Player01及以后的玩家添加触觉功能
            if (playerNumber > 1)
            {
                // 添加触觉控制器
                var hapticTest = gameObject.GetComponent<HapticTest>();
                if (hapticTest == null)
                {
                    hapticTest = gameObject.AddComponent<HapticTest>();
                }
                
                Debug.Log($"已为玩家 {m_PlayerName} 添加触觉功能");
            }
        }

        private void SetupColliderAndDetector()
        {
            // 添加碰撞器设置
            var colliderSetup = gameObject.GetComponent<CircleColliderSetup>();
            if (colliderSetup == null)
            {
                colliderSetup = gameObject.AddComponent<CircleColliderSetup>();
            }
            
            // 添加碰撞检测器
            var collisionDetector = gameObject.GetComponent<PlayerCollisionDetector>();
            if (collisionDetector == null)
            {
                collisionDetector = gameObject.AddComponent<PlayerCollisionDetector>();
            }
            
            // 设置Player标签
            gameObject.tag = "Player";
            
            Debug.Log($"已为玩家 {m_PlayerName} 设置碰撞器和检测器");
        }

        private void LateUpdate()
        {
            transform.position = PlayerPoseSynchronizer.position + m_Offset;
            transform.rotation = Quaternion.Euler(0f, m_CenterEyePose.rotation.eulerAngles.y, 0f);
            
            // 持续确保名称正确
            if (gameObject.name != m_PlayerName)
            {
                gameObject.name = m_PlayerName;
            }
        }
    }
}
