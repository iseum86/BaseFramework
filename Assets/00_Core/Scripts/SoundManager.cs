using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;
using Base.Utils;

public class SoundManager : BaseManager<SoundManager>
{
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private List<AudioSource> _sfxSources = new();
    [SerializeField] private int _sfxPoolCount = 10;
    [SerializeField] private AudioMixer _audioMixer;

    public AudioSource BgmSource => _bgmSource;
    public List<AudioSource> SfxSources => _sfxSources;

    public override void Init()
    {
        base.Init();

        // BGM용 오디오 소스 설정
        if (_bgmSource == null)
        {
            var bgmGo = new GameObject("@BGM_Source");
            bgmGo.transform.SetParent(transform);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        // SFX 풀 생성
        if (_sfxSources.Count == 0)
        {
            var sfxRoot = new GameObject("@SFX_Sources");
            sfxRoot.transform.SetParent(transform);
            for (var i = 0; i < _sfxPoolCount; i++)
            {
                var source = sfxRoot.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxSources.Add(source);
            }
        }
    }

    public async UniTask PlayBgmAsync(string key, float fadeDuration = 1.0f)
    {
        var clip = await ResourceManager.Instance.LoadAssetAsync<AudioClip>(key);
        if (clip == null) return;

        if (_bgmSource.isPlaying)
        {
            await FadeVolume(_bgmSource, 0f, fadeDuration);
        }

        _bgmSource.clip = clip;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        await FadeVolume(_bgmSource, 1.0f, fadeDuration);
    }

    public async UniTask PlaySfxAsync(string key)
    {
        var clip = await ResourceManager.Instance.LoadAssetAsync<AudioClip>(key);
        if (clip == null) return;

        var source = GetAvailableSfxSource();
        if (source != null)
        {
            source.PlayOneShot(clip);
        }
    }

    private AudioSource GetAvailableSfxSource()
    {
        // var 우선 사용 규칙 준수
        for (var i = 0; i < _sfxSources.Count; i++)
        {
            if (!_sfxSources[i].isPlaying) return _sfxSources[i];
        }

        return null;
    }

    private async UniTask FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        var startVolume = source.volume;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            await UniTask.Yield();
        }

        source.volume = targetVolume;
        if (targetVolume <= 0f) source.Stop();
    }

    public override void OnSceneExit()
    {
        for (var i = 0; i < _sfxSources.Count; i++)
        {
            if (_sfxSources[i] != null) _sfxSources[i].Stop();
        }
    }

    public override void OnSceneEnter() { }
}