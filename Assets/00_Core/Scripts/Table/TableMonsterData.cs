using System;
using System.Collections.Generic;
using System.Linq;
using Base.Data;

public enum MonsterType
{
    None = 0,
    Normal = 1,
    Elite = 2,
    Boss = 3
}

namespace Base.Data
{
    [Serializable]
    public class TableMonsterDataItem : SerializableTableData
    {
        public string monsterName;
        public int hp;
        public float moveSpeed;
        public MonsterType monsterType;
        
        // [참고] 부모 클래스인 SerializableTableData에 'tblidx'가 이미 정의되어 있습니다.
    }
    
    public class TableMonsterData : TableContainer<TableMonsterDataItem>
    {
        public override string TableName => "table_monster";

        protected override void UnLoadCustomData()
        {
            // 필요 시 추가적인 메모리 해제 로직 작성
        }
        
        public List<TableMonsterDataItem> GetMonstersByType(MonsterType type)
        {
            return _dicTableData.Values.Where(item => item.monsterType == type).ToList();
        }

        public TableMonsterDataItem GetMonsterByName(string name)
        {
            return _dicTableData.Values.FirstOrDefault(item => item.monsterName == name);
        }
    }
}
