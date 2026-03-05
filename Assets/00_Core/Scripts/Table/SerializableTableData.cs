using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Base.Data
{
    [Serializable]
    public abstract class SerializableTableData
    {
        public TblIndex tblidx;

        // 타입별 필드 정보를 캐싱하여 성능 저하 방지
        private static readonly Dictionary<Type, List<FieldInfo>> _fieldCacheMgr = new();
        
        public virtual bool IsValid() => tblidx != TblIndex.Invalid;

        public void Read(TableBinaryReader reader)
        {
            // 1. 베이커가 가장 먼저 쓰는 tblidx를 먼저 읽습니다.
            reader.Read(out tblidx);

            // 2. 캐싱된 나머지 필드들을 순차적으로 읽습니다.
            var type = GetType();
            if (!_fieldCacheMgr.TryGetValue(type, out var fields))
            {
                fields = GetHierarchyFields(type); // 여기서도 Public 플래그가 포함된 GetHierarchyFields 사용
                _fieldCacheMgr.Add(type, fields);
            }

            foreach (var field in fields)
            {
                // [중요] 부모의 tblidx는 이미 위에서 읽었으므로 리플렉션 루프에서는 스킵합니다.
                if (field.Name == nameof(tblidx)) continue;

                var value = reader.ReadField(field.FieldType);
                field.SetValue(this, value);
            }
        }

        private List<FieldInfo> GetHierarchyFields(Type type)
        {
            var fieldList = new List<FieldInfo>();
            var currentType = type;

            while (currentType != null && currentType != typeof(object))
            {
                // DeclaredOnly로 해당 클래스에 정의된 것만 가져와서 스택에 쌓음
                var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
                // 역순으로 삽입하여 부모 -> 자식 순서 유지
                for (var i = fields.Length - 1; i >= 0; i--)
                {
                    if (fieldList.Any(f => f.Name == fields[i].Name)) continue;
                    fieldList.Insert(0, fields[i]);
                }
                currentType = currentType.BaseType;
            }
            return fieldList;
        }
    }
}