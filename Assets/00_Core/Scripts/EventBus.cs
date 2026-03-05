using System;
using System.Collections.Generic;

public interface IEvent { }

public static class EventBus
{
    // 각 이벤트 타입별 리스너 리스트
    private static readonly Dictionary<Type, List<Action<IEvent>>> _events = new();
    
    // 원래의 Action<T> 리스너를 감싸고 있는 Action<IEvent> 래퍼를 찾기 위한 딕셔너리
    private static readonly Dictionary<Delegate, Action<IEvent>> _wrapperMap = new();

    /// <summary>
    /// 특정 이벤트에 대한 구독을 시작합니다.
    /// </summary>
    public static void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        var type = typeof(T);
        if (!_events.ContainsKey(type))
        {
            _events[type] = new List<Action<IEvent>>();
        }

        // 이미 등록된 리스너인지 확인 (중복 구독 방지)
        if (_wrapperMap.ContainsKey(listener)) return;

        // Action<T>를 Action<IEvent>로 래핑하여 리스트에 추가
        Action<IEvent> wrapper = ev => listener((T)ev);
        _wrapperMap[listener] = wrapper;
        _events[type].Add(wrapper);
    }

    /// <summary>
    /// 특정 이벤트에 대한 구독을 해제합니다.
    /// </summary>
    public static void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        var type = typeof(T);
        
        // 래핑된 액션을 찾아 리스트에서 제거
        if (_wrapperMap.TryGetValue(listener, out var wrapper))
        {
            if (_events.TryGetValue(type, out var listeners))
            {
                listeners.Remove(wrapper);
            }
            _wrapperMap.Remove(listener);
        }
    }

    /// <summary>
    /// 이벤트를 발생시켜 구독자들에게 알립니다.
    /// </summary>
    public static void Publish<T>(T eventMessage) where T : IEvent
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var listeners))
        {
            // 리스트 순회 중 구독 해제가 발생할 수 있으므로 복사본을 만들어 순회합니다.
            var listenersCopy = new List<Action<IEvent>>(listeners);
            foreach (var listener in listenersCopy)
            {
                try
                {
                    listener?.Invoke(eventMessage);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"이벤트 처리 중 오류 발생: {e}");
                }
            }
        }
    }

    /// <summary>
    /// 모든 구독 정보를 초기화합니다. (씬 전환 시 유용)
    /// </summary>
    public static void ClearAll()
    {
        _events.Clear();
        _wrapperMap.Clear();
    }
}