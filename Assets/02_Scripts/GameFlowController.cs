using UnityEngine;
using System;

namespace CuteDuckGame
{
    public enum GamePhase
    {
        ARInitializing,    // AR 시스템 초기화
        ARScanning,        // 평면 스캔 중
        Ready,            // 플레이 준비 완료
        Connecting,       // 네트워크 연결 중
        Playing,          // 게임 진행 중
        GameOver          // 게임 종료
    }

    public class GameFlowController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private FusionSession fusionSession;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private RaycastWithTrackableTypes arController;
        [SerializeField] private GameSessionManager gameSession;
        
        [Header("Current State")]
        [SerializeField] private GamePhase currentPhase = GamePhase.ARInitializing;
        
        // 이벤트 시스템
        public static Action<GamePhase> OnPhaseChanged;
        public static Action<string> OnPhaseMessage;
        
        private void Start()
        {
            Debug.Log("[GameFlowController] 게임 플로우 시작");
            
            // 이벤트 구독
            SubscribeToEvents();
            
            // AR 초기화 단계부터 시작
            TransitionToPhase(GamePhase.ARInitializing);
        }
        
        private void SubscribeToEvents()
        {
            // AR 이벤트 구독
            RaycastWithTrackableTypes.OnARPositionValidityChanged += OnARValidityChanged;
            
            // UI 이벤트 구독
            UIManager.OnGameStartRequested += OnGameStartRequested;
            UIManager.OnGameLeaveRequested += OnGameLeaveRequested;
            
            // 네트워크 이벤트는 FusionSession에서 직접 호출
        }
        
        public void TransitionToPhase(GamePhase newPhase)
        {
            if (currentPhase == newPhase) return;
            
            Debug.Log($"[GameFlowController] {currentPhase} → {newPhase}");
            
            // 이전 단계 종료
            ExitPhase(currentPhase);
            
            // 새 단계 진입
            currentPhase = newPhase;
            EnterPhase(newPhase);
            
            // 이벤트 발생
            OnPhaseChanged?.Invoke(newPhase);
        }
        
        private void EnterPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.ARInitializing:
                    OnPhaseMessage?.Invoke("AR 시스템을 초기화하는 중...");
                    uiManager.ShowPanel("ARScanPanel");
                    break;
                    
                case GamePhase.ARScanning:
                    OnPhaseMessage?.Invoke("주변 환경을 스캔해주세요");
                    arController.SetIndicatorEnabled(true);
                    break;
                    
                case GamePhase.Ready:
                    OnPhaseMessage?.Invoke("게임을 시작할 준비가 완료되었습니다!");
                    uiManager.EnablePlayButton(true);
                    break;
                    
                case GamePhase.Connecting:
                    OnPhaseMessage?.Invoke("다른 플레이어를 찾는 중...");
                    uiManager.ShowPanel("ConnectionPanel");
                    break;
                    
                case GamePhase.Playing:
                    OnPhaseMessage?.Invoke("게임이 시작되었습니다!");
                    uiManager.ShowPanel("GamePanel");
                    break;
                    
                case GamePhase.GameOver:
                    OnPhaseMessage?.Invoke("게임이 종료되었습니다");
                    break;
            }
        }
        
        private void ExitPhase(GamePhase phase)
        {
            // 각 단계별 정리 작업
        }
        
        // 이벤트 핸들러들
        // ReSharper disable Unity.PerformanceAnalysis
        private void OnARValidityChanged(bool isValid)
        {
            if (currentPhase == GamePhase.ARScanning && isValid)
            {
                TransitionToPhase(GamePhase.Ready);
            }
        }
        
        private void OnGameStartRequested()
        {
            if (currentPhase == GamePhase.Ready)
            {
                TransitionToPhase(GamePhase.Connecting);
                fusionSession.TryConnect();
            }
        }
        
        private void OnGameLeaveRequested()
        {
            fusionSession.TryDisconnect();
            TransitionToPhase(GamePhase.Ready);
        }
        
        // FusionSession에서 호출할 메서드들
        public void OnNetworkConnected()
        {
            if (currentPhase == GamePhase.Connecting)
            {
                TransitionToPhase(GamePhase.Playing);
            }
        }
        
        public void OnNetworkDisconnected()
        {
            if (currentPhase == GamePhase.Playing)
            {
                TransitionToPhase(GamePhase.Ready);
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            RaycastWithTrackableTypes.OnARPositionValidityChanged -= OnARValidityChanged;
            UIManager.OnGameStartRequested -= OnGameStartRequested;
            UIManager.OnGameLeaveRequested -= OnGameLeaveRequested;
        }
    }
}