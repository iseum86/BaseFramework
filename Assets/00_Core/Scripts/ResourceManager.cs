using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Base.Utils;

public class ResourceManager : BaseManager<ResourceManager>
{
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new();

    // 콜백 방식 대신 UniTask를 반환하여 await 가능하게 변경
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
    {
        // 1. 이미 로드된 에셋인지 확인
        if (_assetHandles.TryGetValue(key, out var handle))
        {
            return handle.Result as T;
        }

        // 2. 어드레서블 비동기 로드 시작
        var loadHandle = Addressables.LoadAssetAsync<T>(key);
        
        try
        {
            // ToUniTask를 사용하여 await (이전에 설정한 asmdef 참조 덕분에 가능합니다)
            await loadHandle.ToUniTask();

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _assetHandles[key] = loadHandle;
                return loadHandle.Result;
            }
        }
        catch (System.Exception e)
        {
            DevLog.Error($"{key} 에셋 로드 중 예외 발생: {e.Message}");
        }

        // 로드 실패 시 핸들 해제 및 null 반 Manuel
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
        }
        return null;
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
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        _assetHandles.Clear();
        DevLog.Info("모든 리소스 해제 및 정리 완료");
    }

    public override void OnSceneEnter()
    {
        
    }
}