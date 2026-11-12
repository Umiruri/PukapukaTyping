//難易度定義と選択した難易度保存・難易度に応じた制限時間
//選択項目

public enum Difficulty{ Easy,Normal,Hard }

public static class GameSettings
{
    //設定項目
    public static bool ShowRomaji = true;
    public static bool ShowFurigana = false;

    public static bool BgmEnabled = true;
    public static bool TypingSoundEnabled = true;
    public static bool CorrectSoundEnabled = true;
    public static bool MistakeSoundEnabled = true;

    // シーン名
    public const string TitleScene = "TitleScene";
    public const string HowToPlayScene = "HowToPlayScene";
    public const string SettingsScene = "SettingsScene";
    public const string DifficultyScene = "DifficultyScene";
    public const string ConfirmScene = "ConfirmScene";
    public const string GameScene = "GameScene";
    public const string ResultScene = "ResultScene";

    //選択中の難易度
    public static Difficulty SelectedDifficulty = Difficulty.Easy;

    //難易度に応じた制限時間
    public static int TimeLimit => SelectedDifficulty switch
    {
        Difficulty.Easy => 60,
        Difficulty.Normal => 90,
        Difficulty.Hard => 120,
        _ => 60 //選択されなかったときはEasyに（エラー防止）
    };
    
}