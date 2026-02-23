using UnityEngine;
using Base.Utils;

public static class MainEntryPoint
{
    private static bool s_isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (s_isInitialized) return;

        // 시스템 매니저들을 담을 루트 객체 생성
        var root = new GameObject("@ManagerRoot");
        Object.DontDestroyOnLoad(root);

        // 필수 매니저들을 순서대로 추가
        // 여기서 추가되는 순서대로 Awake/Init이 호출됩니다.
        root.AddComponent<ResourceManager>();
        //root.AddComponent<PoolManager>();
        //root.AddComponent<SceneManager>();
        // root.AddComponent<SoundManager>(); // 추후 추가

        s_isInitialized = true;
        DevLog.Info("MainEntryPoint: 시스템 초기화 완료");
    }
}