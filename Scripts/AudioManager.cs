//BGMと効果音の管理

using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    //AudioManagerをどこからでも使えるようにする
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioSource bgmSource;
    public AudioClip titleBGM;
    public AudioClip gameBGM;
    public AudioClip resultBGM;

    [Header("効果音")]
    public AudioSource seSource;
    public AudioSource mistakeSource;
    public AudioClip typingSound;
    public AudioClip correctSound;
    public AudioClip mistakeSound;

    void Awake()
    {
        //すでにAudioManagerが存在する場合
        if(Instance!=null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }

        //このAudioManagerを唯一のものとして登録
        Instance = this;

        //シーンが変わっても破棄されないようにする
        DontDestroyOnLoad(gameObject);

        //シーン切り替え時のイベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        //最初に開いているシーンのBGMを設定
        ChangeBGM(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        //自分自身が登録されている場合だけ解除
        if(Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    //シーンが読み込まれたときに呼ばれる
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ChangeBGM(scene.name);
    }

    // シーンに応じてBGMを切り替える
    void ChangeBGM(string sceneName)
    {
        switch(sceneName)
        {
            case GameSettings.TitleScene:
            case GameSettings.DifficultyScene:
            case GameSettings.ConfirmScene:
                PlayBGM(titleBGM);
                break;

            case GameSettings.GameScene:
                PlayBGM(gameBGM);
                break;

            case GameSettings.ResultScene:
                PlayBGM(resultBGM);
                break;
        }
    }

    //BGMを再生
    public void PlayBGM(AudioClip clip)
    {
        if(bgmSource == null)
        {
            Debug.LogError("BGM用AudioSourceが設定されていません");
            return;
        }

        if(clip == null)
        {
            Debug.LogWarning("再生するBGMが設定されていません");
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        if(!GameSettings.BgmEnabled)
        {
            bgmSource.Stop();
            return;
        }

        //同じBGMがすでに再生されている場合は再スタートしない
        if(bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    //入力音
    public void PlayTypingSound()
    {
        if(!GameSettings.TypingSoundEnabled) return;

        if(seSource == null || typingSound == null)
        {
            Debug.LogWarning("入力音のAudioSourceまたはAudioClipが設定されていません");
            return;
        }

        seSource.PlayOneShot(typingSound);
    }

    //正解音
    public void PlayCorrectSound()
    {
        if(!GameSettings.CorrectSoundEnabled) return;

        if(seSource == null || correctSound == null)
        {
            Debug.LogWarning("正解音のAudioSourceまたはAudioClipが設定されていません");
            return;
        }

        seSource.PlayOneShot(correctSound);
    }

    //ミス音
    public void PlayMistakeSound()
    {
        if(!GameSettings.MistakeSoundEnabled) return;

        if(mistakeSource == null || mistakeSound == null)
        {
            Debug.LogWarning("ミス音のAudioSourceまたはAudioClipが設定されていません");
            return;
        }

        mistakeSource.PlayOneShot(mistakeSound);

        CancelInvoke(nameof(StopMistakeSound));
        Invoke(nameof(StopMistakeSound), 0.20f);
    }

    //ミス音を停止
    void StopMistakeSound()
    {
        if(mistakeSource == null) return;

        mistakeSource.Stop();
    }

    //BGM設定の変更を反映
    public void ApplyBGMSetting()
    {
        if(bgmSource == null)
        {
            Debug.LogError("BGM用AudioSourceが設定されていません");
            return;
        }

        //BGMがOFFなら停止
        if(!GameSettings.BgmEnabled)
        {
            bgmSource.Stop();
            return;
        }

        //現在のBGMが設定されている場合は再生
        if(bgmSource.clip != null)
        {
            PlayBGM(bgmSource.clip);
        }
        else
        {
            //BGMが未設定なら現在のシーンに合わせて設定
            ChangeBGM(SceneManager.GetActiveScene().name);
        }
    }

}