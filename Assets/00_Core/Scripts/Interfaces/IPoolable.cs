
public interface IPoolable
{
    bool IsUsing { get; }
    void OnPop();   // Pop 되어 활성화될 때 호출
    void OnPush();  // Push 되어 비활성화될 때 호출
}
