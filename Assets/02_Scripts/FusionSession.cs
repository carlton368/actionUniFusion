using System;
using System.Collections;
using System.Collections.Generic;
using CuteDuckGame;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace CuteDuckGame
{
    public class FusionSession : MonoBehaviour, INetworkRunnerCallbacks
    {
        // NetworkRunner 프리팹 참조
        public NetworkRunner runnerPrefab;

        // 현재 사용 중인 NetworkRunner 인스턴스 접근 프로퍼티
        public NetworkRunner Runner { get; private set; }

        // 게임 시작 시 자동 씬 로드 제거
        private void Start()
        {
            Debug.Log("[FusionSession- Start()] 초기화 완료 - 단일 씬 모드");
            
            // TODO: 2단계에서 GameFlowController 연동 예정
            // GameFlowController.Instance?.TransitionToPhase(GamePhase.ARScan);
        }

        // 세션에 접속 시도하는 메서드
        public void TryConnect()
        {
            ConnectSharedSessionRoutine($"{StaticData.CurrentRoomName}").Forget();
        }

        // 실질적으로 공유 세션에 접속을 시도하는 코루틴
        public async UniTask ConnectSharedSessionRoutine(string sessionCode)
        {
            Debug.Log($"[FusionSession] 연결 시도: {sessionCode}");
            
            // UI의 상호작용을 잠시 비활성화
            UIManager.Instance.ToggleInteraction(false);

            // 기존의 Runner가 존재한다면 종료 후 갱신
            if (Runner)
                Runner.Shutdown();
            
            Runner = Instantiate(runnerPrefab);
            Runner.AddCallbacks(this);

            // 세션을 시작하는 비동기 작업
            var startGameTask = Runner.StartGame(
                new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = sessionCode,
                    SceneManager = Runner.GetComponent<INetworkSceneManager>(),
                    ObjectProvider = Runner.GetComponent<INetworkObjectProvider>()
                });
            
            // 비동기 작업이 완료될 때까지 대기
            await UniTask.WaitUntil(() => startGameTask.IsCompleted);
            
            // 작업 완료 후 UI 다시 활성화
            UIManager.Instance.ToggleInteraction(true);

            // 결과 확인 및 로그 출력
            var result = startGameTask.Result;
            Debug.Log($"[FusionSession] 연결 결과: {result.ShutdownReason}");
            
            if (result.Ok)
            {
                Debug.Log("[FusionSession] 네트워크 연결 성공!");
                // TODO: 2단계에서 GameFlowController 연동
                // GameFlowController.Instance?.OnConnectionSuccess();
            }
            else
            {
                Debug.LogWarning($"[FusionSession] 연결 실패: {result.ShutdownReason}");
                // TODO: 2단계에서 GameFlowController 연동
                // GameFlowController.Instance?.OnConnectionFailed();
            }
        }

        // 세션 연결 해제 시도
        public void TryDisconnect()
        {
            Debug.Log("[FusionSession] 연결 해제 시도");
            if (Runner != null)
            {
                Runner.Shutdown();
            }
        }

        // NetworkRunner가 종료될 때 호출되는 콜백
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Runner = null;
            Debug.Log($"[FusionSession] 세션 종료: {shutdownReason}");
            
            // 씬 전환 대신 상태 변경만
            if (shutdownReason == ShutdownReason.Ok)
            {
                Debug.Log("[FusionSession] 정상 종료 - 타이틀로 복귀");
                // TODO: 2단계에서 GameFlowController 연동
                // GameFlowController.Instance?.TransitionToPhase(GamePhase.Title);
            }
            else
            {
                Debug.LogWarning($"[FusionSession] 비정상 종료: {shutdownReason}");
                // TODO: 2단계에서 에러 처리
                // GameFlowController.Instance?.OnConnectionError(shutdownReason);
            }
        }

        #region INetworkRunnerCallbacks

        // 서버에 접속 완료
        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("[FusionSession] 서버 연결 완료");
        }

        // 서버에 접속 실패
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError($"[FusionSession] 서버 연결 실패: {reason}");
        }

        // 플레이어가 접속했을 때 콜백
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[FusionSession] 플레이어 접속: {player}");
        }

        // 플레이어가 접속을 종료했을 때 콜백
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[FusionSession] 플레이어 퇴장: {player}");
        }

        // 접속 요청 처리
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        // 사용자 인증 반환값 처리
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        // 호스트가 변경될 때 (호스트 마이그레이션)
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        // 입력을 받는 콜백
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        // 입력이 누락되었을 때 콜백
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        // 씬 로드가 시작될 때 콜백
        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        // 씬 로드가 완료되었을 때 콜백
        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        // 세션 목록이 갱신될 때 콜백
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        // 시뮬레이션 메시지가 도착했을 때 처리
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        // Observer Authority(시야)에서 오브젝트가 빠져나갔을 때 콜백
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        // Observer Authority(시야)로 오브젝트가 진입했을 때 콜백
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        // 서버와의 연결이 끊어졌을 때 콜백
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.LogWarning($"[FusionSession] 서버 연결 끊어짐: {reason}");
        }

        // 신뢰성 있는(NetworkRunner에서 보장되는) 데이터가 수신되었을 때
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        // 신뢰성 있는 데이터 전송 진행 상황 콜백
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        #endregion
    }
}