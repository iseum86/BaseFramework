using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Base.Utils;

public class FadeManager : BaseManager<FadeManager>
{
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private float _defaultFadeDuration = 0.5f;

    public override void Init()
    {
        base.Init();
        // 런타임에 페이드용 캔버스가 없다면 생성하는 로직을 추가할 수 있습니다.
        if (_fadeCanvasGroup == null)
        {
            var go = new GameObject("@FadeCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // 최상단

            var image = new GameObject("FadeImage").AddComponent<Image>();
            image.transform.SetParent(go.transform);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.color = Color.black;

            _fadeCanvasGroup = go.AddComponent<CanvasGroup>();
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
            
            DontDestroyOnLoad(go);
        }
    }

    public void FadeOut(float duration = -1f, Action onComplete = null)
    {
        var time = duration < 0 ? _defaultFadeDuration : duration;
        StartCoroutine(CoFade(0f, 1f, time, onComplete));
    }

    public void FadeIn(float duration = -1f, Action onComplete = null)
    {
        var time = duration < 0 ? _defaultFadeDuration : duration;
        StartCoroutine(CoFade(1f, 0f, time, onComplete));
    }

    private IEnumerator CoFade(float start, float end, float duration, Action onComplete)
    {
        _fadeCanvasGroup.blocksRaycasts = true;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _fadeCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        _fadeCanvasGroup.alpha = end;
        if (end <= 0f) _fadeCanvasGroup.blocksRaycasts = false;

        onComplete?.Invoke();
    }

    public override void OnSceneExit() { }
    public override void OnSceneEnter() { }
}