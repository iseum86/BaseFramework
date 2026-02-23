
public interface ISceneContext
{
    // 씬 로드 직후 호출될 초기화 로직
    void OnSetup();
    
    // 다음 씬으로 넘어가기 직전 호출될 정리 로직
    void OnCleanup();
}
