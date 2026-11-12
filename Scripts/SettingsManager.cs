//設定画面の管理

using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Toggle romajiToggle;
    public Toggle furiganaToggle;
    public Toggle bgmToggle;
    public Toggle typingSoundToggle;
    public Toggle correctSoundToggle;
    public Toggle mistakeSoundToggle;

    void Start()
    {
        SetupToggle(
            romajiToggle,
            GameSettings.ShowRomaji,
            value => GameSettings.ShowRomaji = value
        );

        SetupToggle(
            furiganaToggle,
            GameSettings.ShowFurigana,
            value => GameSettings.ShowFurigana = value
        );

        SetupToggle(
            bgmToggle,
            GameSettings.BgmEnabled,
            value =>
            {
                GameSettings.BgmEnabled = value;
                if(AudioManager.Instance != null)
                {
                    AudioManager.Instance.ApplyBGMSetting();
                }
            }
        );

        SetupToggle(
            typingSoundToggle,
            GameSettings.TypingSoundEnabled,
            value => GameSettings.TypingSoundEnabled = value
        );

        SetupToggle(
            correctSoundToggle,
            GameSettings.CorrectSoundEnabled,
            value => GameSettings.CorrectSoundEnabled = value
        );

        SetupToggle(
            mistakeSoundToggle,
            GameSettings.MistakeSoundEnabled,
            value => GameSettings.MistakeSoundEnabled = value
        );
    }

    //トグルの初期設定と変更時の処理
    void SetupToggle(
        Toggle toggle,
        bool initialValue,
        UnityEngine.Events.UnityAction<bool> onChanged
        )
    {
        if(toggle == null)
        {
            Debug.LogError("設定画面のToggleが設定されていません");
            return;
        }

        toggle.SetIsOnWithoutNotify(initialValue);
        toggle.onValueChanged.AddListener(onChanged);
    }

}