using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip collisionClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float uiVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float menuMusicVolume = 0.6f;
    [Range(0f, 1f)] public float gameMusicVolume = 0.6f;

    private AudioSource uiSource;
    private AudioSource sfxSource;
    private AudioSource musicSource;

    private bool isMenuScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.loop = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HandleScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene);
    }

    private void HandleScene(Scene scene)
    {
        isMenuScene = scene.name.ToLowerInvariant().Contains("menu");

        musicSource.clip = isMenuScene ? mainMenuMusic : gameMusic;
        musicSource.volume = isMenuScene ? menuMusicVolume : gameMusicVolume;

        if (musicSource.clip != null)
            musicSource.Play();
        else
            musicSource.Stop();
    }

    public void PlayUIClick()
    {
        if (uiClickClip == null) return;
        uiSource.PlayOneShot(uiClickClip, Mathf.Clamp01(uiVolume));
    }

    public void PlayJumpSFX()
    {
        if (jumpClip == null) return;
        sfxSource.PlayOneShot(jumpClip, Mathf.Clamp01(sfxVolume));
    }

    public void PlayCollisionSFX()
    {
        if (collisionClip == null) return;
        sfxSource.PlayOneShot(collisionClip, Mathf.Clamp01(sfxVolume));
    }

    public void SetUIVolume(float value)
    {
        uiVolume = Mathf.Clamp01(value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    public void SetMenuMusicVolume(float value)
    {
        menuMusicVolume = Mathf.Clamp01(value);

        if (isMenuScene)
            musicSource.volume = menuMusicVolume;
    }

    public void SetGameMusicVolume(float value)
    {
        gameMusicVolume = Mathf.Clamp01(value);

        if (!isMenuScene)
            musicSource.volume = gameMusicVolume;
    }
}
