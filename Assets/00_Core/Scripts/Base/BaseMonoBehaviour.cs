using UnityEngine;

#if UNITY_EDITOR
using Base.Editor;
#endif

namespace Base.Core
{
    /// <summary>
    /// 프로젝트의 모든 MonoBehaviour 스크립트가 상속받는 베이스 클래스입니다.
    /// </summary>
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {
        [ContextMenu("Framework/Link Components Only This")]
        protected void LinkComponents()
        {
#if UNITY_EDITOR
            FrameworkLinker.Link(this);
#endif
        }

        // 추후 모든 객체에 공통으로 필요한 초기화나 정리 로직을 여기에 추가할 수 있습니다.
        // 예: public virtual void Init() { }
    }
}