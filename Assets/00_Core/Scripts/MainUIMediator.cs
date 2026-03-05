using Base.Utils;
using UnityEngine;
using UnityEngine.UI;

// 준비 완료 이벤트를 받기 위한 구조체
public struct FrameworkReadyEvent : IEvent { }

public class MainUIMediator : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _mainMenuScreen;

    private void OnEnable()
    {
        // 이벤트 버스 구독
        EventBus.Subscribe<FrameworkReadyEvent>(OnFrameworkReady);
    }

    private void OnDisable()
    {
        // 메모리 안전 처리 (구독 해제)
        EventBus.Unsubscribe<FrameworkReadyEvent>(OnFrameworkReady);
    }

    private void OnFrameworkReady(FrameworkReadyEvent ev)
    {
        // 시스템이 준비되면 로딩 화면을 끄고 메인 화면을 켭니다.
        if (_loadingScreen != null) _loadingScreen.SetActive(false);
        if (_mainMenuScreen != null) _mainMenuScreen.SetActive(true);
        
        DevLog.Info("Main UI Switched to Ready State.");
    }
}