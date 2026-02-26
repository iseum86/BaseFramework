#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.Editor
{
    public static class FrameworkLinker
    {
        // 1. 하이어라키 우클릭 메뉴 통합
        [MenuItem("GameObject/Base Framework/Auto Link All Components", false, 0)]
        public static void AutoLinkFromHierarchy()
        {
            var selectedGo = Selection.activeGameObject;
            if (selectedGo == null) return;

            var components = selectedGo.GetComponents<MonoBehaviour>();
            var undoGroup = $"Auto Link Components for {selectedGo.name}";

            // 데이터 안전을 위한 Undo 기록
            Undo.RecordObject(selectedGo, undoGroup);

            foreach (var comp in components)
            {
                if (comp == null) continue;
                Link(comp);
            }

            Debug.Log($"<color=cyan>[FrameworkLinker]</color> {selectedGo.name}의 모든 컴포넌트 연결 완료");
        }

        // 2. 개별 컴포넌트용 핵심 로직 (BaseMonoBehaviour 등에서 호출)
        public static void Link(MonoBehaviour target)
        {
            if (target == null) return;

            var targetType = target.GetType();
            // private/public 인스턴스 필드 검색
            var fields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                // [SerializeField]이 있거나 Public인 필드만 대상
                if (Attribute.IsDefined(field, typeof(SerializeField)) || field.IsPublic)
                {
                    // 규칙: 시작 언더바 제거 후 이름 매칭 (_hpBar -> hpBar)
                    var searchName = field.Name.TrimStart('_');
                    var childTransform = FindChildRecursive(target.transform, searchName);

                    if (childTransform == null) continue;

                    // 안전 처리: 타입에 따른 할당 분기
                    if (field.FieldType == typeof(GameObject))
                    {
                        field.SetValue(target, childTransform.gameObject);
                    }
                    else if (typeof(Component).IsAssignableFrom(field.FieldType))
                    {
                        var component = childTransform.GetComponent(field.FieldType);
                        if (component != null)
                        {
                            field.SetValue(target, component);
                        }
                    }
                }
            }

            // 프리팹/씬 데이터 저장 보장
            EditorUtility.SetDirty(target);
        }

        // 재귀적 자식 탐색 유틸리티
        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;

            for (var i = 0; i < parent.childCount; i++)
            {
                var result = FindChildRecursive(parent.GetChild(i), name);
                if (result != null) return result;
            }

            return null;
        }
    }
}
#endif