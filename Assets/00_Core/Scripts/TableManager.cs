using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Base.Data;
using Base.Utils;

public class TableManager : BaseManager<TableManager>
{
    [SerializeField] private bool _useBinaryInEditor = false;
    
    private readonly Dictionary<Type, ITableContainer> _dicTableMgr = new();

    public bool UseBinaryInEditor => _useBinaryInEditor;

    public override void Init()
    {
        base.Init();
        RegisterTables();
    }

    public override void OnSceneEnter() { }
    public override void OnSceneExit() { }

    private void RegisterTables()
    {
        // 이제 타입 매칭 걱정 없이 등록 가능합니다.
        AddTable(new TableMonsterData());
    }

    private void AddTable(ITableContainer container)
    {
        var type = container.GetType();
        if (!_dicTableMgr.ContainsKey(type))
        {
            _dicTableMgr.Add(type, container);
        }
    }

    public async UniTask LoadAllTablesAsync()
    {
        // var 최우선 사용 및 안전한 비동기 처리
        var loadTasks = new List<UniTask>();
        foreach (var container in _dicTableMgr.Values)
        {
            loadTasks.Add(container.LoadAsync());
        }

        await UniTask.WhenAll(loadTasks);
    }

    public TContainer Get<TContainer>() where TContainer : class, ITableContainer
    {
        var type = typeof(TContainer);
        if (_dicTableMgr.TryGetValue(type, out var container))
        {
            return container as TContainer;
        }
        
        // 솔직한 에러 로그 출력
        DevLog.Error($"[TableManager] 등록되지 않은 테이블 요청: {type.Name}");
        return null;
    }

    public void AllUnload()
    {
        foreach (var container in _dicTableMgr.Values)
        {
            container.UnLoad();
        }
        _dicTableMgr.Clear();
    }
}