using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace CuteDuckGame
{
    public class RaycastWithTrackableTypes : MonoBehaviour
    {
        [Header("AR 컴포넌트")]
        [SerializeField] private GameObject indicator;
        [SerializeField] private GameObject stage;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private GameObject placedObject;
        
        [Header("Unity Events")]
        [SerializeField] private UnityEvent<Vector3> OnPositionSelected;
        [SerializeField] private UnityEvent OnValidPositionDetected;
        [SerializeField] private UnityEvent OnPositionLost;
        
        private List<ARRaycastHit> hits = new List<ARRaycastHit>();
        private Vector3 currentSelectedPosition = Vector3.zero;
        private bool hasValidPosition = false;
        private bool isInTitleScene = false;
        
        // Action 이벤트
        public static System.Action<Vector3> OnARPositionChanged;
        public static System.Action<bool> OnARPositionValidityChanged;

        private void Start()
        {
            indicator.SetActive(false);
            
            if (raycastManager == null)
                raycastManager = GetComponent<ARRaycastManager>();
                
            CheckCurrentScene();
        }

        private void Update()
        {
            DetectGround();
            HandleTouch();
        }
        
        /// <summary>
        /// 현재 씬 확인
        /// </summary>
        private void CheckCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            isInTitleScene = sceneName.Contains("Title");
            Debug.Log($"[RaycastWithTrackableTypes] 현재 씬: {sceneName}, 타이틀 씬: {isInTitleScene}");
        }
        
        private void DetectGround()
        {
            Vector2 screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
            {
                indicator.SetActive(true);
                indicator.transform.position = hits[0].pose.position;
                indicator.transform.rotation = hits[0].pose.rotation;
                indicator.transform.position += indicator.transform.up * 0.1f;
        
                Vector3 newPosition = hits[0].pose.position;
                if (Vector3.Distance(currentSelectedPosition, newPosition) > 0.1f)
                {
                    currentSelectedPosition = newPosition;
            
                    // 항상 StaticData에 저장 (씬 체크 제거)
                    StaticData.SetInitialSpawnPos(currentSelectedPosition);
                    OnPositionSelected?.Invoke(currentSelectedPosition);
                    OnARPositionChanged?.Invoke(currentSelectedPosition);
                }
        
                if (!hasValidPosition)
                {
                    hasValidPosition = true;
                    OnValidPositionDetected?.Invoke();
                    OnARPositionValidityChanged?.Invoke(true);
                }
            }
            else
            {
                indicator.SetActive(false);
        
                if (hasValidPosition)
                {
                    hasValidPosition = false;
                    OnPositionLost?.Invoke();
                    OnARPositionValidityChanged?.Invoke(false);
                }
            }
        }

        private void HandleTouch()
        {
            // 게임 씬에서만 터치로 오브젝트 배치
            if (!isInTitleScene && indicator.activeInHierarchy && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (placedObject == null)
                    {
                        placedObject = Instantiate(stage, indicator.transform.position, indicator.transform.rotation);
                    }
                    else
                    {
                        placedObject.transform.position = indicator.transform.position;
                        placedObject.transform.rotation = indicator.transform.rotation;
                    }
                }
            }
        }
        
        /// <summary>
        /// 현재 선택된 위치 반환
        /// </summary>
        public Vector3 GetCurrentSelectedPosition()
        {
            return currentSelectedPosition;
        }
        
        /// <summary>
        /// 유효한 위치가 있는지 확인
        /// </summary>
        public bool HasValidPosition()
        {
            return hasValidPosition;
        }
        
        /// <summary>
        /// 인디케이터 활성화/비활성화
        /// </summary>
        public void SetIndicatorEnabled(bool enabled)
        {
            enabled = enabled;
            if (!enabled && indicator != null)
                indicator.SetActive(false);
        }
        
        /// <summary>
        /// 씬 변경 시 호출
        /// </summary>
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckCurrentScene();
        }
    }
}