using UnityEngine;
using Base.Utils;

public class InGameContext : MonoBehaviour, ISceneContext
{
    public void OnSetup()
    {
        DevLog.Info("인게임 씬 설정 시작: 유닛 생성 및 데이터 로드");
        // 예: PoolManager.Instance.Pop<Player>(playerPrefab);
    }

    public void OnCleanup()
    {
        DevLog.Info("인게임 씬 정리 시작: 스코어 저장 및 몬스터 풀 반환");
        // 예: 인게임 전용 데이터 캐시 삭제 등
    }
}
