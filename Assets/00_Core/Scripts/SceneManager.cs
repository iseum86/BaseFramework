using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Base.Utils;

public class SceneManager : BaseManager<SceneManager>
{
    [SerializeField] private float _minLoadingTime = 0.5f;
    private string _currentSceneName;

    public async UniTaskVoid ChangeSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // 1. 페이드 아웃 시작
        await FadeManager.Instance.FadeOutAsync();

        // 2. 현재 씬 정리
        var oldContext = FindObjectOfType<MonoBehaviour>() as ISceneContext;
        oldContext?.OnCleanup();
        
        ResourceManager.Instance.OnSceneExit();
        PoolManager.Instance.OnSceneExit();

        // 3. 비동기 씬 로딩 시작
        var startTime = Time.time;
        var handle = Addressables.LoadSceneAsync(sceneName);

        // 로딩 화면 UI 진척도 업데이트 (필요 시 로딩 UI도 UniTask로 제어)
        await handle.ToUniTask(Progress.Create<float>(p => {
            // LoadingUI.Instance.SetProgress(p);
        }));

        // 최소 로딩 시간 보장
        var loadTime = Time.time - startTime;
        if (loadTime < _minLoadingTime)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(_minLoadingTime - loadTime));
        }

        // 4. 새 씬 준비
        _currentSceneName = sceneName;
        await UniTask.Yield(); // 씬 오브젝트들이 Awake를 마칠 때까지 한 프레임 대기

        var newContext = FindObjectOfType<MonoBehaviour>() as ISceneContext;
        newContext?.OnSetup();

        ResourceManager.Instance.OnSceneEnter();
        PoolManager.Instance.OnSceneEnter();

        // 5. 페이드 인으로 마무리
        await FadeManager.Instance.FadeInAsync();
        
        DevLog.Info($"{sceneName} 씬 전환 프로세스 완료");
    }

    public override void OnSceneExit() { }
    public override void OnSceneEnter() { }
}