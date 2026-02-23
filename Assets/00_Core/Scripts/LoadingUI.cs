using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void SetProgress(float progress)
    {
        if (_progressSlider != null)
            _progressSlider.value = progress;

        if (_loadingText != null)
            _loadingText.text = $"Loading... {(progress * 100f):F0}%";
    }

    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }
}