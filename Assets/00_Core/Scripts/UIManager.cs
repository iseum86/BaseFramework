using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Base.Utils;

public class UIManager : BaseManager<UIManager>
{
    private Dictionary<UI_Layer, Transform> _layers = new();
    private Stack<UIBase> _popupStack = new();

    [SerializeField] private Canvas _rootCanvas;

    public override void Init()
    {
        base.Init();
        InitLayers();
    }

    private void InitLayers()
    {
        if (_rootCanvas == null)
        {
            // 런타임에 Canvas 생성 로직 또는 프리팹 로드
        }

        // 레이어별 부모 객체 생성
        var enumValues = System.Enum.GetValues(typeof(UI_Layer));
        foreach (UI_Layer layer in enumValues)
        {
            var go = new GameObject(layer.ToString());
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(_rootCanvas.transform);
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
            
            // Anchor 설정 등 추가...
            _layers[layer] = go.transform;
        }
    }

    // 팝업 열기
    public async UniTask<T> ShowPopupAsync<T>(string prefabKey) where T : UIBase
    {
        // 리소스 매니저에서 에셋을 비동기로 가져옴 (인자 1개)
        var prefab = await ResourceManager.Instance.LoadAssetAsync<GameObject>(prefabKey);
        if (prefab == null) 
            return null;

        var go = Instantiate(prefab, _layers[UI_Layer.Popup]);
        var ui = go.GetComponent<T>();

        if (ui != null)
        {
            _popupStack.Push(ui);
            await ui.OnOpen();
        }

        return ui;
    }

    // 가장 최근 팝업 닫기 (뒤로 가기 등에 사용)
    public async UniTask ClosePopupAsync()
    {
        if (_popupStack.Count == 0) 
            return;

        var ui = _popupStack.Pop();
        await ui.OnClose();
        
        // 실제 파괴 또는 풀링 처리
        Destroy(ui.gameObject);
        // ResourceManager.Instance.UnloadAsset(ui.name);
    }

    public override void OnSceneExit()
    {
        // 씬 전환 시 모든 팝업 스택 비우기
        while (_popupStack.Count > 0)
        {
            var ui = _popupStack.Pop();
            Destroy(ui.gameObject);
        }
        _popupStack.Clear();
    }

    public override void OnSceneEnter() { }
}