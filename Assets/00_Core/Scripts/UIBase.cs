using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class UIBase : MonoBehaviour
{
    [SerializeField] private UI_Layer _layer;
    public UI_Layer Layer => _layer;

    protected virtual void Awake()
    {
        // UI 바인딩 로직 등을 여기에 넣을 수 있습니다.
    }

    public abstract UniTask OnOpen();
    public abstract UniTask OnClose();

    // UI가 파괴될 때 리소스 해제 확인
    protected virtual void OnDestroy()
    {
        // 필요한 경우 ResourceManager를 통해 에셋 해제 트리거
    }
}