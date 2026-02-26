using System;
using System.Collections.Generic;
using Base.Utils;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public abstract class UIBase : MonoBehaviour
{
    // 타입별로 바인딩된 컴포넌트들을 저장
    private Dictionary<Type, UnityEngine.Object[]> _objects = new();
    
    [SerializeField] private UI_Layer _layer;
    public UI_Layer Layer => _layer;

    protected virtual void Awake()
    {
        // UI 바인딩 로직 등을 여기에 넣을 수 있습니다.
    }

    // UI가 파괴될 때 리소스 해제 확인
    protected virtual void OnDestroy()
    {
        // 필요한 경우 ResourceManager를 통해 에셋 해제 트리거
    }
    
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        var names = Enum.GetNames(type);
        var objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (var i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Base.UI.UI_Utils.FindChild<GameObject>(gameObject, names[i], true);
            else
                objects[i] = Base.UI.UI_Utils.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                DevLog.Warning($"Failed to bind: {names[i]}");
        }
    }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        if (_objects.TryGetValue(typeof(T), out var objects) == false)
            return null;

        return objects[idx] as T;
    }

    // 자주 쓰는 컴포넌트 전용 Get
    protected Button GetButton(int idx) => Get<Button>(idx);
    protected Image GetImage(int idx) => Get<Image>(idx);
    protected TMPro.TextMeshProUGUI GetText(int idx) => Get<TMPro.TextMeshProUGUI>(idx);

    public abstract UniTask OnOpen();
    public abstract UniTask OnClose();
}