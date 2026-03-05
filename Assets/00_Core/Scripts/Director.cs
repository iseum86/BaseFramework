using UnityEngine;
using Cysharp.Threading.Tasks;
using Base.Data;
using Base.Utils;

public class Director : MonoBehaviour
{
    private static Director _instance;
    public static Director Instance => _instance;
    
    private TableManager _tableMgr;
    private SoundManager _soundMgr;
    private ResourceManager _resourceMgr;
    
    public static TableManager TableMgr => _instance?._tableMgr;
    public static SoundManager SoundMgr => _instance?._soundMgr;
    public static ResourceManager ResourceMgr => _instance?._resourceMgr;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSyncManagers();
    }

    private async void Start()
    {
        await InitializeAsyncManagers();
        
        // 모든 시스템 준비 완료 전파
        EventBus.Publish(new FrameworkReadyEvent());
        DevLog.Info("Director: All Managers (Mgrs) are initialized and ready.");
    }

    private void InitializeSyncManagers()
    {
        _resourceMgr = ResourceManager.Instance;
        _resourceMgr.Init();

        _soundMgr = SoundManager.Instance;
        _soundMgr.Init();
    }

    private async UniTask InitializeAsyncManagers()
    {
        _tableMgr = TableManager.Instance;
        _tableMgr.Init();

        // 모든 기획 데이터 로드 대기
        await _tableMgr.LoadAllTablesAsync();
    }

    public void OnSceneEnter() { }
    public void OnSceneExit() { }
}