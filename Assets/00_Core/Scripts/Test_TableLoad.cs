using UnityEngine;
using Base.Data;
using Base.Utils;

public class Test_TableLoad : MonoBehaviour
{
    private void OnEnable()
    {
        // 모든 준비가 완료되었을 때 실행되는 이벤트 구독
        EventBus.Subscribe<FrameworkReadyEvent>(OnFrameworkReady);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<FrameworkReadyEvent>(OnFrameworkReady);
    }

    private void OnFrameworkReady(FrameworkReadyEvent ev)
    {
        RunIntegrationTest();
    }

    private void RunIntegrationTest()
    {
        DevLog.Info("=== [통합 테스트 시작] ===");

        // 1. Director를 통해 TableManager(Mgr) 접근
        var monsterTable = Director.TableMgr.Get<TableMonsterData>();
        if (monsterTable == null)
        {
            DevLog.Error("TableMonsterData를 찾을 수 없습니다.");
            return;
        }

        // 2. 특정 데이터(Slime) 조회 테스트 (ID: 1001)
        var slimeIndex = new TblIndex(1001);
        var slimeData = monsterTable.GetData(slimeIndex);

        if (slimeData != null)
        {
            // 필드명이 컬럼명과 일치하므로 바로 접근
            DevLog.Info($"[성공] 데이터 로드 완료!");
            DevLog.Info($"이름: {slimeData.monsterName}");
            DevLog.Info($"체력(HP): {slimeData.hp}");
            DevLog.Info($"이동속도: {slimeData.moveSpeed}");
            DevLog.Info($"타입: {slimeData.monsterType}");
        }
        else
        {
            DevLog.Error($"ID {slimeIndex.Value}의 데이터를 찾을 수 없습니다.");
        }

        // 3. 전용 조회 함수 테스트 (이름으로 찾기)
        var dragon = monsterTable.GetMonsterByName("Ancient Dragon");
        if (dragon != null)
        {
            DevLog.Info($"[성공] 전용 함수 작동: {dragon.monsterName} (HP: {dragon.hp})");
        }

        DevLog.Info("=== [통합 테스트 종료] ===");
    }
}