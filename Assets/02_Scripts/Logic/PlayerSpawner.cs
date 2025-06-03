using Fusion;
using UnityEngine;
using UnityEngine.Events;
using CuteDuckGame;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Header("플레이어 설정")]
    [Tooltip("플레이어 캐릭터 프리팹 (NetworkObject 포함)")]
    public GameObject PlayerPrefab;
    
    [Header("스폰 위치 설정")]
    [SerializeField] private Vector3 fallbackSpawnPos = Vector3.zero;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1, 0);
    [SerializeField] private float spawnRadius = 2f;
    
    [Header("Unity Events")]
    [SerializeField] private UnityEvent<PlayerRef> OnPlayerSpawned;
    [SerializeField] private UnityEvent<Vector3> OnSpawnPositionUsed;

    // Action 이벤트
    public static System.Action<PlayerRef, Vector3> OnPlayerSpawnedAtPosition;

    public void PlayerJoined(PlayerRef player)
    {
        // 로컬 플레이어만 스폰 (호스트 또는 클라이언트 자기 자신)
        if (player == Runner.LocalPlayer)
        {
            Vector3 spawnPosition = GetSpawnPosition(player);
            
            // 플레이어 스폰
            NetworkObject playerObject = Runner.Spawn(PlayerPrefab,
                         spawnPosition,
                         Quaternion.identity,
                         player); // InputAuthority 설정
            
            Debug.Log($"[PlayerSpawner] 플레이어 스폰 완료: {player} at {spawnPosition}");
            
            // 이벤트 발생
            OnPlayerSpawned?.Invoke(player);
            OnSpawnPositionUsed?.Invoke(spawnPosition);
            OnPlayerSpawnedAtPosition?.Invoke(player, spawnPosition);
        }
    }
    
    /// <summary>
    /// 스폰 위치 계산
    /// </summary>
    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        Vector3 basePosition;
        
        // StaticData에서 AR로 설정된 위치 사용
        if (StaticData.HasValidSpawnPos())
        {
            basePosition = StaticData.GetCurrentSpawnPosition();
            Debug.Log($"[PlayerSpawner] AR 위치 사용: {basePosition}");
        }
        else
        {
            basePosition = fallbackSpawnPos;
            Debug.LogWarning($"[PlayerSpawner] AR 위치가 없어 기본 위치 사용: {basePosition}");
        }
        
        // 여러 플레이어를 위한 랜덤 오프셋 (원형 배치)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // 최종 스폰 위치
        Vector3 finalPosition = basePosition + spawnOffset + randomOffset;
        
        return finalPosition;
    }
    
    /// <summary>
    /// 스폰 위치 미리보기 (Scene 뷰)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 basePos = StaticData.HasValidSpawnPos() ? 
                         StaticData.GetCurrentSpawnPosition() : 
                         fallbackSpawnPos;
        
        Vector3 spawnPos = basePos + spawnOffset;
        
        // 기본 스폰 위치
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnPos, Vector3.one * 0.5f);
        
        // 스폰 반경
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPos, spawnRadius);
        
        // 방향 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(spawnPos, Vector3.forward * 1f);
    }
}