using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Canvasの表示を管理する配列
    public GameObject[] canvas;

    // 制限時間のカウント

    //カウントダウン
    public float countdown = 90.0f;

    //時間を表示するText型の変数
    public TextMeshProUGUI timeText;

    // スコアを表示するText型の変数
    public TextMeshProUGUI scoreText;

    // 結果スコアを表示するText型の変数
    public TextMeshProUGUI resultScoreText;

    // メッセージを表示するText型の変数
    public TextMeshProUGUI resultMessage;

    // スコアの変数
    public int score = 0;

    // 表示するholidayマークの配列
    public GameObject[] holidayMark;

    // 最初に生成されるholidayの数
    public int startHolidayCount = 4;

    // 1週間の配列を生成(holiday判定　0:weekday　1:holiday)
    public int[] week = new int[] { 0, 0, 0, 0, 0, 0, 0 };

    // 現在のholidayの数
    public int holidayCount = 0;

    // アルファベット配列
    private char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    // 1週間のアルファベットを代入する配列
    public char[] weekAlphabet = new char[7];

    // アルファベット表示用の配列
    public TMP_Text[] alphabetText;

    // ゲームスタートのフラグ
    public bool gameStartFlag = false;

    void Start()
    {
        // タイトル画面だけを表示
        canvas[0].SetActive(true);
        canvas[1].SetActive(false);
        canvas[2].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && gameStartFlag == false){

            // ゲームを表示/タイトルとリザルトを非表示
            canvas[0].SetActive(false);
            canvas[1].SetActive(true);
            canvas[2].SetActive(false);

            // マーク初期化
            for (int i = 0; i < holidayMark.Length; i++)
            {
                holidayMark[i].SetActive(false);
            }

            // スコア初期化
            score = 0;

            // 制限時間の初期化
            countdown = 90.0f;

            // holidayマークを生成する処理
            GenerateHoliday();
            // アルファベットを表示
            GenerateAlphabet();

            // ゲームスタートフラグをtrueに
            gameStartFlag = true;

        }

        if(gameStartFlag == true)
        {
            // 制限時間を表示
            LimitTime();

            // スコアテキスト更新
            scoreText.text = score.ToString() + "週間";

            //時間を表示する
            timeText.text = countdown.ToString("f1") + "秒";
        }

        // enterキー押すと実行(いまだけ)
        if (Input.anyKeyDown)
        {
            string keyStr = Input.inputString;
            char keyChar = keyStr.ToCharArray()[0];

            for (int i = 0; i < weekAlphabet.Length; i++) 
            {
                if(weekAlphabet[i] == keyChar && week[i] == 0)
                {
                    week[i] = 1;
                    holidayMark[i].SetActive(true);
                    holidayCount++;
                    break;
                }
            }

            if (holidayCount == 7)
            {
                // holidayマークを生成する処理
                GenerateHoliday();
                // アルファベットを表示
                GenerateAlphabet();
                // スコアを加算
                score++;
            }
        }
    }

    // 制限時間の関数
    void LimitTime()
    {
        //時間をカウントダウンする
        countdown -= Time.deltaTime;

        // 時間切れになったら
        if (countdown <= 0)
        {
            // タイトル画面だけを表示
            canvas[0].SetActive(false);
            canvas[1].SetActive(false);
            canvas[2].SetActive(true);

            // 結果スコアを表示
            resultScoreText.text = score.ToString() + "週間";

            // メッセージ判定
            if(score < 10)
            {
                resultMessage.text = "1限目に遅刻だわ";
            }
            else if(score < 20)
            {
                resultMessage.text = "いつもより背伸びをしたけどこんな日も悪くないかも";
            }
            else if(score < 30)
            {
                resultMessage.text = "私と君の色をのせて花がパッと咲いた";
            }
            else if(score < 40)
            {
                resultMessage.text = "たまにはちょっと冒険してみない?私たちと";
            }
            else if(score < 52)
            {
                resultMessage.text = "この眼はいつも君を追いかけてる";
            }
            else if(score < 90)
            {
                resultMessage.text = "365日全部毎日がHoliday";
            }
            else
            {
                resultMessage.text = "あたし、センパイと残陽したんだっ！";
            }

            // ゲームスタートフラグをfalseに
            gameStartFlag = false;
        }
    }

    // ランダムにHolidayをつける
    void GenerateHoliday()
    {
        // 現在のholidayの数の初期化
        holidayCount = 0;

        // マークの初期化
        for (int i = 0; i < holidayMark.Length; i++)
        {
            holidayMark[i].SetActive(false);
        }

        startHolidayCount = 4;

        // 配列の初期化(すべて0に)
        for(int i = 0; i < week.Length; i++)
        {
            week[i] = 0;
        }

        // holidayマークをつけるループ
        for (int i = 0; i < week.Length; i++)
        {
            if (startHolidayCount > 0)
            {
                if (i < 7 - startHolidayCount)
                {
                    week[i] = Random.Range(0, 2);
                    if (week[i] == 1)
                    {
                        holidayMark[i].SetActive(true);
                        startHolidayCount--;
                        holidayCount++;
                    }
                }
                else if (i == 7 - startHolidayCount)
                {
                    week[i] = 1;
                    holidayMark[i].SetActive(true);
                    startHolidayCount--;
                    holidayCount++;
                }
            }       
            
        }
    }

    // ランダムにアルファベットを生成
    void GenerateAlphabet()
    {
        // マークの初期化
        for (int i = 0; i < weekAlphabet.Length; i++)
        {
            weekAlphabet[i] = '0';
        }

        // アルファベットを重複なしで格納して表示
        for (int i = 0; i < weekAlphabet.Length; i++)
        {
            weekAlphabet[i] = alphabet[Random.Range(0, 26)];

            for(int j = 0; j < i; j++)
            {
                if (weekAlphabet[i] == weekAlphabet[j])
                {
                    i--;
                }
            }

            alphabetText[i].text = weekAlphabet[i].ToString();
        }
    }
}
