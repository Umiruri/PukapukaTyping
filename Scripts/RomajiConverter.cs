//かなからローマ字への変換

using System.Collections.Generic;
using UnityEngine;

public class RomajiConverter
{
    //かなとローマ字の対応表
    private Dictionary<string, string[]> romajiRules =
        new Dictionary<string, string[]>();

    
    //CSVを読み込む
    public void LoadRules(TextAsset csvFile)
    {
        romajiRules.Clear();

        //基本のかなを登録
        AddBasicRules();

        if(csvFile == null)
        {
            Debug.LogError("InputRules.csv が設定されていません");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        //1行目は見出し
        for(int i = 1; i<lines.Length; i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(',');

            if(columns.Length < 2)
            {
                Debug.LogWarning((i + 1) + "行目：列が不足しているためスキップします");
                continue;
            }

            string kana = columns[0].Trim();

            //かなが空欄なら読み込まない
            if(string.IsNullOrEmpty(kana))
            {
                Debug.LogWarning((i + 1) + "行目：かなが空欄のためスキップします");
                continue;
            }

            string[] candidates = columns[1].Split('/');

            List<string> validCandidates = new List<string>();

            //空欄ではない候補だけを登録
            foreach(string candidate in candidates)
            {
                string trimmedCandidate = candidate.Trim().ToLower();

                if(!string.IsNullOrEmpty(trimmedCandidate)) validCandidates.Add(trimmedCandidate); 
            }

            //有効な候補が1つもない場合
            if(validCandidates.Count == 0)
            {
                Debug.LogWarning((i + 1) + "行目：ローマ字候補が空欄のためスキップします");
                continue;
            }

            //CSVのルールを優先する
            romajiRules[kana] = validCandidates.ToArray();
        }

        Debug.Log("ローマ字ルールを読み込みました：" + romajiRules.Count + "種類");
    }

    //基本のかなを登録
    void AddBasicRules()
    {
        string[] kanaList =
        {
            "あ","い","う","え","お",
            "か","き","く","け","こ",
            "さ","し","す","せ","そ",
            "た","ち","つ","て","と",
            "な","に","ぬ","ね","の",
            "は","ひ","ふ","へ","ほ",
            "ま","み","む","め","も",
            "や","ゆ","よ",
            "ら","り","る","れ","ろ",
            "わ","を","ん",
            "が","ぎ","ぐ","げ","ご",
            "ざ","じ","ず","ぜ","ぞ",
            "だ","ぢ","づ","で","ど",
            "ば","び","ぶ","べ","ぼ",
            "ぱ","ぴ","ぷ","ぺ","ぽ"
        };

        string[] romajiList =
        {
            "a","i","u","e","o",
            "ka","ki","ku","ke","ko",
            "sa","si","su","se","so",
            "ta","ti","tu","te","to",
            "na","ni","nu","ne","no",
            "ha","hi","hu","he","ho",
            "ma","mi","mu","me","mo",
            "ya","yu","yo",
            "ra","ri","ru","re","ro",
            "wa","wo","nn",
            "ga","gi","gu","ge","go",
            "za","zi","zu","ze","zo",
            "da","di","du","de","do",
            "ba","bi","bu","be","bo",
            "pa","pi","pu","pe","po"
        };

        for (int i = 0; i < kanaList.Length; i++)
        {
            AddRule(kanaList[i], romajiList[i]);
        }

        //拗音
        AddRule("きゃ", "kya");
        AddRule("きゅ", "kyu");
        AddRule("きょ", "kyo");

        AddRule("ぎゃ", "gya");
        AddRule("ぎゅ", "gyu");
        AddRule("ぎょ", "gyo");

        AddRule("しゃ", "sya");
        AddRule("しゅ", "syu");
        AddRule("しょ", "syo");

        AddRule("じゃ", "zya");
        AddRule("じゅ", "zyu");
        AddRule("じょ", "zyo");

        AddRule("ちゃ", "tya");
        AddRule("ちゅ", "tyu");
        AddRule("ちょ", "tyo");

        AddRule("にゃ", "nya");
        AddRule("にゅ", "nyu");
        AddRule("にょ", "nyo");

        AddRule("ひゃ", "hya");
        AddRule("ひゅ", "hyu");
        AddRule("ひょ", "hyo");

        AddRule("びゃ", "bya");
        AddRule("びゅ", "byu");
        AddRule("びょ", "byo");

        AddRule("ぴゃ", "pya");
        AddRule("ぴゅ", "pyu");
        AddRule("ぴょ", "pyo");

        AddRule("みゃ", "mya");
        AddRule("みゅ", "myu");
        AddRule("みょ", "myo");

        AddRule("りゃ", "rya");
        AddRule("りゅ", "ryu");
        AddRule("りょ", "ryo");
    }

    //ルールを登録
    void AddRule(string kana, string romaji)
    {
        romajiRules[kana] = new string[] { romaji };
    }

    //表示用のローマ字を作る
    public string ConvertToRomaji(string hiragana)
    {
        if(string.IsNullOrEmpty(hiragana)) return "";

        string result = "";
        int position = 0;

        while(position < hiragana.Length)
        {
            //っの処理
            if(hiragana[position] == 'っ')
            {
                string nextRomaji = GetNextRomaji(hiragana, position + 1);

                if(!string.IsNullOrEmpty(nextRomaji)
                    && IsConsonant(nextRomaji[0]))
                {
                    result += nextRomaji[0];
                }

                position++;
                continue;
            }

            //長音
            if(hiragana[position] == 'ー')
            {
                result += "-";
                position++;
                continue;
            }

            string matchedKana = FindLongestKana(hiragana, position);

            if(matchedKana == "")
            {
                Debug.LogWarning("変換できないかな：" + hiragana[position]);
                result += hiragana[position];
                position++;
                continue;
            }

            //ローマ字候補を取得
            if(!romajiRules.TryGetValue(matchedKana, out string[] candidates) ||
                candidates == null ||
                candidates.Length == 0 ||
                string.IsNullOrEmpty(candidates[0]))
            {
                Debug.LogWarning("有効なローマ字候補がありません：" + matchedKana);
                result += hiragana[position];
                position += matchedKana.Length;
                continue;
            }

            //んは表示用にnnとする
            if(matchedKana == "ん") result += "nn";
            else result += candidates[0];

            position += matchedKana.Length;
        }

        return result.ToUpper();
    }

    //入力判定用：その位置のローマ字候補を取得
    public string[] GetCandidates(
        string hiragana,
        int position,
        out int consumedLength)
    {
        consumedLength = 0;

        if(string.IsNullOrEmpty(hiragana) ||
        position < 0 || 
        position >= hiragana.Length)
        {
            return new string[0];
        }

        //っは次のかなの先頭子音を使う
        if (hiragana[position] == 'っ')
        {
            string nextRomaji = GetNextRomaji(hiragana, position + 1);

            if(!string.IsNullOrEmpty(nextRomaji) && IsConsonant(nextRomaji[0]))
            {
                consumedLength = 1;
                return new string[] { nextRomaji[0].ToString() };
            }

            return new string[0];
        }

        //長音
        if(hiragana[position] == 'ー')
        {
            consumedLength = 1;
            return new string[] { "-" };
        }

        string matchedKana = FindLongestKana(hiragana, position);

        if(matchedKana == "") return new string[0];

        consumedLength = matchedKana.Length;

        //んの入力候補
        if(matchedKana == "ん")
        {
            bool followedByVowelOrY = false;

            int nextPosition = position + 1;

            if(nextPosition < hiragana.Length)
            {
                char nextKana = hiragana[nextPosition];

                string vowels = "あいうえおやゆよ";

                followedByVowelOrY = vowels.IndexOf(nextKana) >= 0;
            }

            if(followedByVowelOrY) return new string[] { "nn" };

            return new string[] { "nn", "n" };
        }

        //ローマ字候補を取得
        if(!romajiRules.TryGetValue(matchedKana, out string[] candidates) ||
            candidates == null ||
            candidates.Length == 0)
        {
            Debug.LogWarning("有効なローマ字候補がありません：" + matchedKana);
            consumedLength = 0;
            return new string[0];
        }

        return candidates;
    }

    //最も長く一致するかなを探す
    string FindLongestKana(string hiragana, int position)
    {
        for(int length = 4; length >= 1; length--)
        {
            if(position + length > hiragana.Length) continue;

            string part = hiragana.Substring(position, length);

            if(romajiRules.ContainsKey(part)) return part;
        }

        return "";
    }

    // 次のかなの先頭ローマ字を取得
    string GetNextRomaji(string hiragana, int position)
    {
        //文字列が空、または位置が範囲外なら終了
        if(string.IsNullOrEmpty(hiragana) ||
            position < 0 ||
            position >= hiragana.Length)
        {
            return "";
        }

        string matchedKana = FindLongestKana(hiragana, position);

        //一致するかながなければ終了
        if(matchedKana == "") return "";

        //ローマ字ルールが存在するか確認
        if(!romajiRules.TryGetValue(matchedKana, out string[] candidates)) return "";

        //候補が空、または先頭候補が空欄なら終了
        if(candidates == null ||
            candidates.Length == 0 ||
            string.IsNullOrEmpty(candidates[0]))
        {
            return "";
        }

        return candidates[0];
    }

    //子音かどうか
    bool IsConsonant(char character)
    {
        return character >= 'a' && character <= 'z'
            && "aiueo".IndexOf(character) == -1;
    }
    
}