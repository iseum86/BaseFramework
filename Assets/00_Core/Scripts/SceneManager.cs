using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Base.Utils;

public class SceneManager : BaseManager<SceneManager>
{
    [SerializeField] private LoadingUI _loadingUIPrefab;
    [SerializeField] private float _minLoadingTime = 0.5f; // 너무 빨리 로딩될 때의 번쩍임 방지

    private LoadingUI _loadingUI;
    private string _currentSceneName;
    public string CurrentSceneName => _currentSceneName;

    public override void Init()
    {
        base.Init();
        _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 로딩 UI 프리팹이 있다면 미리 생성하여 관리
        if (_loadingUIPrefab != null)
        {
            _loadingUI = Instantiate(_loadingUIPrefab, transform);
            _loadingUI.Hide();
            DontDestroyOnLoad(_loadingUI.gameObject);
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        StartCoroutine(LoadSceneProcess(sceneName));
    }

    private IEnumerator LoadSceneProcess(string sceneName)
    {
        // 1. 현재 씬 정리 (ISceneContext 검색 및 실행)
        var oldContext = FindObjectOfType<MonoBehaviour>() as ISceneContext;
        oldContext?.OnCleanup();
        
        // 매니저 단위 정리
        ResourceManager.Instance.OnSceneExit();
        PoolManager.Instance.OnSceneExit();

        // 2. 로딩 UI 표시
        if (_loadingUI != null)
        {
            _loadingUI.SetProgress(0f);
            _loadingUI.Show();
        }

        var startTime = Time.time;
        var handle = Addressables.LoadSceneAsync(sceneName);

        // 3. 로딩 진행률 업데이트
        while (!handle.IsDone)
        {
            var progress = handle.PercentComplete;
            _loadingUI?.SetProgress(progress);
            yield return null;
        }

        // 최소 로딩 시간 대기 (너무 빠를 경우 UX를 위해)
        var loadTime = Time.time - startTime;
        if (loadTime < _minLoadingTime)
        {
            yield return new WaitForSeconds(_minLoadingTime - loadTime);
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _currentSceneName = sceneName;

            // 4. 새 씬 준비 (ISceneContext 검색 및 실행)
            // 씬 로드 직후에는 찾기 어려울 수 있으므로 한 프레임 대기
            yield return null;
            
            var newContext = FindObjectOfType<MonoBehaviour>() as ISceneContext;
            newContext?.OnSetup();

            ResourceManager.Instance.OnSceneEnter();
            PoolManager.Instance.OnSceneEnter();

            DevLog.Info($"{sceneName} 씬으로 전환 완료");
        }
        else
        {
            DevLog.Error($"{sceneName} 씬 로드 실패");
        }

        // 5. 로딩 UI 숨기기
        _loadingUI?.Hide();
    }

    public override void OnSceneExit() { }
    public override void OnSceneEnter() { }
}