using UnityEngine;

public abstract class BaseManager<T> : Singleton<T> where T : MonoBehaviour
{
    [SerializeField] private bool _isInitialized;
    public bool IsInitialized => _isInitialized;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    // 매니저 초기 로직 (최초 1회)
    public virtual void Init()
    {
        if (_isInitialized) return;
        
        // 초기화 로직 작성
        _isInitialized = true;
    }

    // 씬 전환 시 정리 로직
    public abstract void OnSceneExit();

    // 새 씬 진입 시 준비 로직
    public abstract void OnSceneEnter();
}
