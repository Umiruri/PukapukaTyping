//データ保存用

public static class ResultData
{
    //正解タイプ数
    public static int CorrectKeyCount;

    //ミスタイプ数
    public static int MistakeCount;

    //実際にプレイした時間
    public static float PlayTime;

    //平均入力速度（正解タイプ数 ÷ プレイ時間）
    public static float AverageSpeed
    {
        get
        {
            if(PlayTime <= 0f ||
               float.IsNaN(PlayTime) ||
               float.IsInfinity(PlayTime))
            {
                return 0f;
            }

            if(CorrectKeyCount <= 0) return 0f;

            return CorrectKeyCount / PlayTime;
        }
    }
}