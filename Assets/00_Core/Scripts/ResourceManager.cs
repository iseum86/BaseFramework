using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Base.Utils;

public class ResourceManager : BaseManager<ResourceManager>
{
    // 로드된 에셋들의 핸들을 관리하여 누수 방지
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new();

    public override void Init()
    {
        base.Init();
        // 필요한 경우 초기 에셋 카탈로그 업데이트 로직 추가
    }

    public void LoadAssetAsync<T>(string key, System.Action<T> callback) where T : Object
    {
        // 이미 로드된 에셋인지 확인
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            callback?.Invoke(handle.Result as T);
            return;
        }

        // 비동기 로드 시작
        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        loadHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _assetHandles[key] = op;
                callback?.Invoke(op.Result);
            }
            else
            {
                DevLog.Error($"에셋 로드 실패: {key}");
                Addressables.Release(op);
            }
        };
    }

    public void UnloadAsset(string key)
    {
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _assetHandles.Remove(key);
            DevLog.Info($"{key} 에셋 해제 완료");
        }
    }

    public override void OnSceneExit()
    {
        foreach (var handle in _assetHandles.Values)
        {
            Addressables.Release(handle);
        }
        _assetHandles.Clear();
        DevLog.Info("모든 리소스 해제 및 정리 완료");
    }

    public override void OnSceneEnter()
    {
        
    }
}