using UnityEngine;
using System;

namespace CuteDuckGame
{
    public static class StaticData
    {
        public static string LocalNickname;
        public static string CurrentRoomName = "123";
        
        // AR 위치 관련 추가
        public static Vector3 _initialSpawnPos = Vector3.zero;
        public static bool HasValidSpawnPosition = false;
        
        // 이벤트 - Action 사용
        public static Action<Vector3> OnSpawnPositionChanged;
        public static Action<string> OnRoomNameChanged;
        
        /// 스폰 위치 설정
        public static void SetInitialSpawnPos(Vector3 pos)
        {
            _initialSpawnPos = pos;
            HasValidSpawnPosition = true;
            
            Debug.Log($"[StaticData] 스폰 위치 설정: {pos}");
            
            // 이벤트 발생
            OnSpawnPositionChanged?.Invoke(pos);
        }
        
        /// 룸 이름 설정
        public static void SetRoomName(string roomName)
        {
            if (!string.IsNullOrEmpty(roomName))
            {
                CurrentRoomName = roomName;
                Debug.Log($"[StaticData] 룸 이름 설정: {roomName}");
                
                // 이벤트 발생
                OnRoomNameChanged?.Invoke(roomName);
            }
        }
        
        /// 현재 스폰 위치 반환
        public static Vector3 GetCurrentSpawnPosition()
        {
            return _initialSpawnPos;
        }
        
        /// 유효한 스폰 위치가 있는지 확인
        public static bool HasValidSpawnPos()
        {
            return HasValidSpawnPosition;
        }
    }
}