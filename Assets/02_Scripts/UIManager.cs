using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CuteDuckGame
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private Button playButton;
        [SerializeField] private Button leaveButton;
        
        [Header("상태 표시 UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Unity Events")]
        [SerializeField] private UnityEvent OnPlayButtonPressed;
        [SerializeField] private UnityEvent OnLeaveButtonPressed;
        [SerializeField] private UnityEvent<bool> OnARPositionValidityChanged;

        // 싱글톤(Singleton) 접근을 위한 정적 필드
        public static UIManager Instance;
        
        // Action 이벤트
        public static Action OnGameStartRequested;
        public static Action OnGameLeaveRequested;

        private void Awake()
        {
            // 싱글톤 초기화
            Instance = this;
        }
        
        private void Start()
        {
            SetupButtonEvents();
        }
        
        private void Update()
        {
            UpdateARStatus();
        }
        
        /// 버튼 이벤트 설정
        private void SetupButtonEvents()
        {
            // 플레이 버튼 이벤트 연결
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
        }
        
        /// AR 상태 업데이트
        private void UpdateARStatus()
        {
            if (SceneManager.GetActiveScene().name.Contains("Game"))
            {
                // 타이틀 씬에서만 AR 상태 확인
                bool hasValidPosition = StaticData.HasValidSpawnPos();
                
                // 플레이 버튼 활성화 상태 업데이트
                if (playButton != null)
                {
                    playButton.interactable = hasValidPosition;
                }
                
                // 상태 텍스트 업데이트
                if (statusText != null)
                {
                    if (hasValidPosition)
                        statusText.text = $"위치 선택됨: {StaticData.GetCurrentSpawnPosition():F1}";
                    else
                        statusText.text = "평면을 찾는 중...";
                }
            }
        }
        
        /// AR 위치 유효성 변화 이벤트 핸들러
        private void OnARValidityChanged(bool isValid)
        {
            OnARPositionValidityChanged?.Invoke(isValid);
            
            if (playButton != null)
            {
                playButton.interactable = isValid;
            }
            
            Debug.Log($"[UIManager] AR 위치 유효성 변경: {isValid}");
        }
        
        /// 씬이 로드될 때마다 호출되는 메서드.
        /// 씬 이름에 따라 버튼 활성화 여부를 다르게 설정한다.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            // Game 씬일 경우 Leave 버튼 활성화
            if (scene.name.Contains("Game"))
            {
                TogglePlayButton(true);
                ToggleLeaveButton(true);
                return;
            }
        }
        
        /// 플레이 버튼 클릭 이벤트
        public void OnPlayButtonClicked()
        {
            Debug.Log("[UIManager] 플레이 버튼 클릭됨");
            
            // AR 위치 유효성 확인
            if (StaticData.HasValidSpawnPos())
            {
                // Unity Event 발생
                OnPlayButtonPressed?.Invoke();
                
                // Action 이벤트 발생
                OnGameStartRequested?.Invoke();
                
                // FusionSession 연결 시도
                FusionSession fusionSession = FindObjectOfType<FusionSession>();
                if (fusionSession != null)
                {
                    fusionSession.TryConnect();
                }
                else
                {
                    Debug.LogWarning("[UIManager] FusionSession을 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[UIManager] 유효한 AR 위치가 선택되지 않았습니다.");
                ShowErrorMessage("먼저 게임을 시작할 위치를 선택해주세요.");
            }
        }
        
        /// <summary>
        /// 나가기 버튼 클릭 이벤트
        /// </summary>
        public void OnLeaveButtonClicked()
        {
            Debug.Log("[UIManager] 나가기 버튼 클릭됨");
            
            // Unity Event 발생
            OnLeaveButtonPressed?.Invoke();
            
            // Action 이벤트 발생
            OnGameLeaveRequested?.Invoke();
            
            // FusionSession 연결 해제
            FusionSession fusionSession = FindObjectOfType<FusionSession>();
            if (fusionSession != null)
            {
                fusionSession.TryDisconnect();
            }
        }
        
        /// 오류 메시지 표시
        private void ShowErrorMessage(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"⚠️ {message}";
                statusText.color = Color.red;
                
                // 3초 후 원래 상태로 복원
                Invoke(nameof(ResetStatusText), 3f);
            }
        }
        
        /// 상태 텍스트 리셋
        private void ResetStatusText()
        {
            if (statusText != null)
            {
                statusText.color = Color.white;
            }
        }

        /// UI 인터랙션 활성/비활성 전환
        public void ToggleInteraction(bool isOn)
        {
            eventSystem.enabled = isOn;
        }

        /// Play 버튼 활성/비활성 전환
        public void TogglePlayButton(bool isOn)
        {
            playButton.gameObject.SetActive(isOn);
        }
        
        /// Leave 버튼 활성/비활성 전환
        public void ToggleLeaveButton(bool isOn)
        {
            leaveButton.gameObject.SetActive(isOn);
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RaycastWithTrackableTypes.OnARPositionValidityChanged -= OnARValidityChanged;
        }
    }
}