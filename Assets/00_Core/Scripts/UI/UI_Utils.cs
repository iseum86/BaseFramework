using UnityEngine;
using Base.Utils;

namespace Base.UI
{
    public static class UI_Utils
    {
        // 자식 오브젝트 중 이름과 타입이 일치하는 컴포넌트를 찾습니다.
        public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            if (go == null) 
                return null;

            if (recursive == false)
            {
                for (var i = 0; i < go.transform.childCount; i++)
                {
                    var transform = go.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || transform.name == name)
                    {
                        var component = transform.GetComponent<T>();
                        if (component != null) return component;
                    }
                }
            }
            else
            {
                foreach (var component in go.GetComponentsInChildren<T>(true))
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        return component;
                }
            }

            return null;
        }
    }
}