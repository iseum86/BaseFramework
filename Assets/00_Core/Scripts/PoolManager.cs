using System.Collections.Generic;
using UnityEngine;
using Base.Utils;

public class PoolManager : BaseManager<PoolManager>
{
    private Dictionary<string, Stack<GameObject>> _poolDict = new();
    
    [SerializeField] private Transform _poolRoot;
    public Transform PoolRoot => _poolRoot;

    public override void Init()
    {
        base.Init();
        if (_poolRoot == null)
        {
            var go = new GameObject("@PoolRoot");
            _poolRoot = go.transform;
            DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// 풀에서 객체를 꺼냅니다. (Pop)
    /// </summary>
    public T Pop<T>(GameObject prefab, Transform parent = null) where T : Component
    {
        var key = prefab.name;
        GameObject go;

        if (_poolDict.TryGetValue(key, out var stack) && stack.Count > 0)
        {
            go = stack.Pop();
        }
        else
        {
            go = Instantiate(prefab);
            go.name = key;
        }

        go.transform.SetParent(parent);
        go.SetActive(true);
        
        var poolable = go.GetComponent<IPoolable>();
        poolable?.OnPop();

        return go.GetComponent<T>();
    }

    /// <summary>
    /// 사용이 끝난 객체를 풀에 다시 넣습니다. (Push)
    /// </summary>
    public void Push(GameObject go)
    {
        if (go == null) return;

        var key = go.name;
        if (!_poolDict.ContainsKey(key))
        {
            _poolDict[key] = new Stack<GameObject>();
        }

        var poolable = go.GetComponent<IPoolable>();
        poolable?.OnPush();

        go.SetActive(false);
        go.transform.SetParent(_poolRoot);
        _poolDict[key].Push(go);
    }

    public override void OnSceneExit()
    {
        // 씬 전환 시 풀에 남은 모든 객체를 파괴하여 메모리 정리
        foreach (var stack in _poolDict.Values)
        {
            while (stack.Count > 0)
            {
                var go = stack.Pop();
                if (go != null) Destroy(go);
            }
        }
        _poolDict.Clear();
        DevLog.Info("PoolManager: 모든 풀링 객체 정리 완료");
    }

    public override void OnSceneEnter() { }
}