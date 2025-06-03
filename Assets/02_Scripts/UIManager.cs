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
        [SerializeField] private GameObject playButton;
        [SerializeField] private GameObject leaveButton;
        
        [Header("상태 표시 UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button playButtonComponent;
        
        [Header("Unity Events")]
        [SerializeField] private UnityEvent OnPlayButtonPressed;
        [SerializeField] private UnityEvent OnLeaveButtonPressed;
        [SerializeField] private UnityEvent<bool> OnARPositionValidityChanged;

        // 싱글톤(Singleton) 접근을 위한 정적 필드
        public static UIManager Instance;
        
        // Action 이벤트
        public static System.Action OnGameStartRequested;
        public static System.Action OnGameLeaveRequested;

        private void Awake()
        {
            // 싱글톤 초기화
            Instance = this;

            // 씬이 로드될 때마다 이벤트를 받아서 OnSceneLoaded를 호출하도록 연결
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // AR 위치 유효성 변화 이벤트 구독
            RaycastWithTrackableTypes.OnARPositionValidityChanged += OnARValidityChanged;
            
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupButtonEvents();
        }
        
        private void Update()
        {
            UpdateARStatus();
        }
        
        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // 플레이 버튼 컴포넌트 자동 찾기
            if (playButtonComponent == null && playButton != null)
                playButtonComponent = playButton.GetComponent<Button>();
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtonEvents()
        {
            // 플레이 버튼 이벤트 연결
            if (playButtonComponent != null)
            {
                playButtonComponent.onClick.RemoveAllListeners();
                playButtonComponent.onClick.AddListener(OnPlayButtonClicked);
            }
        }
        
        /// <summary>
        /// AR 상태 업데이트
        /// </summary>
        private void UpdateARStatus()
        {
            if (SceneManager.GetActiveScene().name.Contains("Title"))
            {
                // 타이틀 씬에서만 AR 상태 확인
                bool hasValidPosition = StaticData.HasValidSpawnPos();
                
                // 플레이 버튼 활성화 상태 업데이트
                if (playButtonComponent != null)
                {
                    playButtonComponent.interactable = hasValidPosition;
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
        
        /// <summary>
        /// AR 위치 유효성 변화 이벤트 핸들러
        /// </summary>
        private void OnARValidityChanged(bool isValid)
        {
            OnARPositionValidityChanged?.Invoke(isValid);
            
            if (playButtonComponent != null)
            {
                playButtonComponent.interactable = isValid;
            }
            
            Debug.Log($"[UIManager] AR 위치 유효성 변경: {isValid}");
        }

        /// <summary>
        /// 씬이 로드될 때마다 호출되는 메서드.
        /// 씬 이름에 따라 버튼 활성화 여부를 다르게 설정한다.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Title 씬일 경우
            if (scene.name.Contains("Title"))
            {
                TogglePlayButton(true);
                ToggleLeaveButton(false);
                return;
            }

            // Game 씬일 경우 Leave 버튼 활성화
            if (scene.name.Contains("Game"))
            {
                TogglePlayButton(false);
                ToggleLeaveButton(true);
                return;
            }
        }
        
        /// <summary>
        /// 플레이 버튼 클릭 이벤트
        /// </summary>
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
        
        /// <summary>
        /// 오류 메시지 표시
        /// </summary>
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
        
        /// <summary>
        /// 상태 텍스트 리셋
        /// </summary>
        private void ResetStatusText()
        {
            if (statusText != null)
            {
                statusText.color = Color.white;
            }
        }

        /// <summary>
        /// UI 인터랙션 활성/비활성 전환
        /// </summary>
        public void ToggleInteraction(bool isOn)
        {
            eventSystem.enabled = isOn;
        }

        /// <summary>
        /// Play 버튼 활성/비활성 전환
        /// </summary>
        public void TogglePlayButton(bool isOn)
        {
            playButton.SetActive(isOn);
        }

        /// <summary>
        /// Leave 버튼 활성/비활성 전환
        /// </summary>
        public void ToggleLeaveButton(bool isOn)
        {
            leaveButton.SetActive(isOn);
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RaycastWithTrackableTypes.OnARPositionValidityChanged -= OnARValidityChanged;
        }
    }
}