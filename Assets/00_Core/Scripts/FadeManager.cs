using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Base.Utils;

public class FadeManager : BaseManager<FadeManager>
{
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private float _defaultDuration = 0.5f;

    // 개별 페이드 작업을 제어하기 위한 토큰
    private CancellationTokenSource _fadeCts;

    public override void Init()
    {
        base.Init();
        if (_fadeCanvasGroup == null)
        {
            // (기존 코드와 동일하게 런타임에 Fade UI 생성 로직)
        }
    }

    public async UniTask FadeOutAsync(float duration = -1f)
    {
        var time = duration < 0 ? _defaultDuration : duration;
        await PlayFadeAsync(0f, 1f, time);
    }

    public async UniTask FadeInAsync(float duration = -1f)
    {
        var time = duration < 0 ? _defaultDuration : duration;
        await PlayFadeAsync(1f, 0f, time);
    }

    private async UniTask PlayFadeAsync(float start, float end, float duration)
    {
        // 이전 작업이 진행 중이라면 취소
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = new CancellationTokenSource();

        var token = _fadeCts.Token;
        _fadeCanvasGroup.blocksRaycasts = true;
        var elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                // 취소 요청 시 즉시 중단
                token.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                _fadeCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
                
                // 코루틴의 yield return null과 동일하지만 더 가벼움
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _fadeCanvasGroup.alpha = end;
            if (end <= 0f) _fadeCanvasGroup.blocksRaycasts = false;
        }
        catch (System.OperationCanceledException)
        {
            DevLog.Info("Fade 작업이 안전하게 취소되었습니다.");
        }
    }

    public override void OnSceneExit()
    {
        _fadeCts?.Cancel();
    }

    public override void OnSceneEnter() { }
}