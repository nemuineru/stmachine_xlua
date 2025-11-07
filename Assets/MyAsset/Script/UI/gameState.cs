using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
//using UnityEditor.SearchService;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using TMPro;
using UnityEngine.SceneManagement;

public class gameState : MonoBehaviour
{
    //操作対象のEntity.
    internal Entity Player;

    [SerializeField]
    TMP_Text WaveBigName;

    //全部のスクリプトからアクセスするように.
    //
    static public gameState self;

    //敵のHP管理バー.
    public GameObject HPUI;

    public GameObject defaultEff;
    public GameObject defaultDeathEff;

    [SerializeField]
    TMP_Text KillValue_Text;
    internal int KillValue = 0;

    public AudioSource inGameAuds;

    //ゲーム前かゲーム中かそうでないか
    internal enum GameStateDesc
    {
        PreGame,
        InGame,
        GameOver,
        Finished,
        PauseMenu
    }
    [SerializeField]
    internal GameStateDesc gDesc = GameStateDesc.InGame;

    void Awake()
    {
        if (self == null)
        {
            self = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Player = GameObject.FindWithTag("Player").GetComponent<Entity>();
    }

    //ゲームスタート・ゲームオーバーの時
    bool isGameStartUIShown = false;
    bool isGameOverUIShown = false;

    float GameStartBy = 3.0f;

    public float elapsedTime = 0;
    void Update()
    {
        entityList = FindObjectsByType<Entity>(FindObjectsSortMode.None)
        .OrderBy(t => !t.attrs.alive)
        .ThenBy(t => Vector3.Magnitude(t.transform.position - Player.transform.position))
        .ToList();
        GameDescApply();
    }

    void GameDescApply()
    {
        switch (gDesc)
        {
            case GameStateDesc.PreGame:
                {
                    PreGameUI.SetActive(true);
                    GameStartBy -= Time.deltaTime;
                    if (GameStartBy < 0)
                    {
                        gDesc = GameStateDesc.InGame;
                    }
                    break;
                }
            case GameStateDesc.InGame:
                {
                    Time.timeScale = Mathf.Lerp(Time.timeScale, 1.0f, 0.3f);
                    GameOverCams.enabled = false;
                    pauseGameUI.SetActive(false);
                    PreGameUI.SetActive(false);
                    InGameUI.SetActive(true);
                    if(inGameAuds != null)
                    inGameAuds.pitch = Mathf.Lerp(inGameAuds.pitch, 1f, 0.08f);
                    elapsedTime += Time.deltaTime;
                    break;
                }
            case GameStateDesc.GameOver:
                {
                    InGameUI.SetActive(false);
                    pauseGameUI.SetActive(false);
                    GameOverCams.enabled = true;
                    Time.timeScale = Mathf.Lerp(Time.timeScale, 0.001f, 0.025f);
                    if (!isGameOverUIShown)
                    {
                        GameOverUI.SetActive(true);
                        isGameOverUIShown = true;
                    }
                    if(inGameAuds != null)
                    inGameAuds.pitch = Mathf.Lerp(inGameAuds.pitch, 0.001f, 0.025f);
                    break;
                }
            case GameStateDesc.Finished:
                {
                    InGameUI.SetActive(false);
                    pauseGameUI.SetActive(false);
                    FinishedCams.enabled = true;
                    if (!isGameOverUIShown)
                    {
                        FinishedUI.SetActive(true);
                        isGameOverUIShown = true;
                    }
                    break;
                }
            case GameStateDesc.PauseMenu:
                {
                    InGameUI.SetActive(false);
                    pauseGameUI.SetActive(true);
                    GameOverCams.enabled = true;
                    Time.timeScale = Mathf.Lerp(Time.timeScale, 0.00f, 0.025f);
                    if(inGameAuds != null)
                    inGameAuds.pitch = Mathf.Lerp(inGameAuds.pitch, 0.00f, 0.025f);
                    break;
                }
        }

    }

    public GameObject PreGameUI;
    public GameObject InGameUI;
    public GameObject GameOverUI;
    public GameObject pauseGameUI;
    public GameObject FinishedUI;

    public Cinemachine.CinemachineVirtualCamera GameOverCams;
    public Cinemachine.CinemachineVirtualCamera FinishedCams;

    public List<Entity> entityList;

    //HitDefを発火する際のイベント
    public bool ProvokeHitDef_Entity(Entity calledEntity, hitDefParams hitDefParams)
    {
        bool ret = false;
        int refNumRemaining;
        hitDefParams useParam = new hitDefParams();
        if (hitDefParams != null)
        {
            useParam = hitDefParams;
        }
        refNumRemaining = useParam.maxEntityHits;
        foreach (Entity e in entityList)
        {
            //selfには反応しない. また当たる数が設定されているなら0にならない限り設定される.
            if (e != calledEntity && e.tag != calledEntity.tag && refNumRemaining > 0)
            {
                //それぞれのentityの現在再生中のAnimatorが持つClssに対して衝突判定.
                //また、entityの無敵判定に関しても考える.
                bool f = calledEntity.hitCheck(e, out Vector3 HitPt);
                bool isContactable = hitDefParams.HitMoveFlag.Contains(e.moveType.ToString()) &&
                hitDefParams.HitPhysFlag.Contains(e.physicsType.ToString()) &&
                !hitDefParams.HitExcludeList.Contains(e.CurrentStateID);
                //hitしたなら一先ずAnim番号を5000に飛ばしたい. ChangeState(5000)の最優先Queueとして組み込む.
                if (f == true && isContactable)
                {
                    ret = true;
                    hitDefApply(e, calledEntity, useParam, HitPt);
                    //当てた分キャラ指定の値が減少..
                    refNumRemaining--;
                    calledEntity.status.currentEnergy += 3;
                }
            }
        }
        return ret;
    }

    public bool ProvokeHitDef_Projs(Entity calledEntity, clssSetting sets, Transform trfs, hitDefParams H_params)
    {
        bool ret = false; int refNumRemaining;
        hitDefParams useParam = new hitDefParams();
        if (H_params != null)
        {
            useParam = H_params;
        }
        refNumRemaining = useParam.maxEntityHits;
        foreach (Entity e in entityList)
        {
            //selfには反応しない. また当たる数が設定されているなら0にならない限り設定される.
            if ((calledEntity == null || (e != calledEntity && e.tag != calledEntity.tag)) && refNumRemaining > 0)
            {
                bool f = false;
                Vector3 HitPt = Vector3.zero;
                //それぞれのentityの現在再生中のAnimatorが持つClssに対して衝突判定.
                //また、entityの無敵判定に関しても考える.
                clssSetting cEnemy = e.MainAnimMixer.MainAnimDef.clssSetting;
                f = sets.clssCollided(out var v1, out var v2, out var dist, clssDef.ClssType.Attack, cEnemy, .1f);
                //hitしたなら一先ずAnim番号を5000に飛ばしたい. ChangeState(5000)の最優先Queueとして組み込む.
                if (f == true)
                {
                    Debug.LogWarning("Proj Collided");
                    HitPt = (v1 + v2) / 2f;
                    ret = true;
                    hitDefApply(e, trfs, useParam, HitPt);
                    //当てた分キャラ指定の値が減少..
                    refNumRemaining--;
                }
            }
        }
        return ret;
    }

    void hitDefApply(Entity beatenEntity, Entity calledEntity,
    hitDefParams calledEParam, Vector3 hitContactPoint)
    {
        //stateChangeを設定..
        beatenEntity.isStateChanged = true;

        //一先ず、プレースホルダーとして入れる
        //stateTimeをリセット.
        beatenEntity.CurrentStateID = calledEParam.ChangeState_Enemy;
        beatenEntity.stateTime = 0;
        //攻撃を当てた対象にコントロールされる場合は相手のステートマップの読み出しを設定
        //設定されたEntityはselfStateが読み出されない限り読み出す.
        if (calledEParam.enemyRefsPlayerNum == true)
        {
            beatenEntity.controlledEntity = calledEntity;
        }
        //placeholder for velocity
        //currently its barebone
        Vector3 HitVect = Vector3.ProjectOnPlane
        (beatenEntity.transform.position - calledEntity.transform.position, Vector3.up);

        //hitpause
        (calledEntity.HitPauseTime, beatenEntity.HitPauseTime) = (calledEParam.hitStopTime.x, calledEParam.hitStopTime.y);
        DamageApply(beatenEntity, HitVect, calledEParam, calledEntity.status.BaseAttackPerc);
        Instantiate
        ((calledEParam.HitEff != null ? calledEParam.HitEff : defaultEff), hitContactPoint, Quaternion.identity);

        //playerのchangestateが0以上なら変更.
        if (calledEParam.ChangeState_Player > -1)
        {
            calledEntity.isStateChanged = true;
            calledEntity.CurrentStateID = calledEParam.ChangeState_Player;
        }
    }

    void hitDefApply(Entity beatenEntity, Transform calledPoint,
    hitDefParams calledEParam, Vector3 hitContactPoint)
    {
        //stateChangeを設定..
        beatenEntity.isStateChanged = true;

        //一先ず、プレースホルダーとして入れる
        //stateTimeをリセット.
        beatenEntity.CurrentStateID = calledEParam.ChangeState_Enemy;
        beatenEntity.stateTime = 0;

        //現状、Projectileに関してはステート奪取を考えないことにする.
        // if (calledEParam.enemyRefsPlayerNum == true)
        // {
        //     beatenEntity.controlledEntity = calledEntity;
        // }

        //placeholder for velocity
        //currently its barebone
        Vector3 HitVect = Vector3.ProjectOnPlane
        (beatenEntity.transform.position - calledPoint.position, Vector3.up);

        //hitpause
        (beatenEntity.HitPauseTime) = (calledEParam.hitStopTime.y);
        DamageApply(beatenEntity, HitVect, calledEParam, 100f);
        Instantiate
        ((calledEParam.HitEff != null ? calledEParam.HitEff : defaultEff), hitContactPoint, Quaternion.identity);
    }



    void DamageApply(Entity beatenEntity, Vector3 HitVect, hitDefParams calledEParam, float atkParams)
    {
        //SetSpeed
        beatenEntity.rigid.velocity = HitVect.normalized * calledEParam.velset.x + Vector3.up * calledEParam.velset.y;


        //shapepositions
        beatenEntity.transform.DOShakePosition(beatenEntity.HitPauseTime * Time.fixedDeltaTime, 0.25f, 40, 45);
        //beatenEntity.transform.DOShakeScale(1f, 3f, 30, 90f, true);
        beatenEntity.ChangeAnim(.1f);

        //hitpoint damage
        beatenEntity.status.currentHP -= calledEParam.Damage * (atkParams / Mathf.Max(1.0f,beatenEntity.status.BaseDefencePerc));

        //placeholder for rotation
        beatenEntity.transform.rotation =
        Quaternion.Lerp(beatenEntity.transform.rotation, Quaternion.LookRotation(-HitVect, Vector3.up), 0.6f);
        Debug.Log("Hit : " + beatenEntity.gameObject.name);
    }

    internal void ClearChars()
    {
        foreach (Entity ent in entityList)
        {
            if (ent.tag != "Player")
            {
                ent.status.currentHP = 0;
                ent.attrs.alive = false;
            }
        }
    }

    internal void AddKillValue()
    {
        KillValue++;
        KillValue_Text.text = KillValue.ToString();
    }

    bool checkHit(string checkState, Entity checkEntity)
    {
        bool ret = false;
        if (checkState.Contains((char)checkEntity.checkHitStates()))
        {
            ret = true;
        }
        return ret;
    }

    public IEnumerator OneShotSlo_mo(float SlowValue)
    {
        float remTimeMax = SlowValue;
        float remTime = 0;
        while (remTime < remTimeMax)
        {
            remTime += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(0.0f, 1f, Mathf.Min(1f, MathF.Pow(remTime / remTimeMax, 2.0f)));
            yield return 0;
        }
    }

    public IEnumerator ShowWaveNames(string Names)
    {
        if (WaveBigName != null)
        {
            WaveBigName.enabled = true;
            float remTimeMax = 3f;
            float remTime = remTimeMax;
            while (remTime > 0)
            {
                int WaveNameIndexes = Mathf.Max(0, Mathf.CeilToInt((Names.Length) * ((remTimeMax - remTime) / (remTimeMax * 0.5f))));
                WaveNameIndexes = Mathf.Min(WaveNameIndexes, Names.Length);
                WaveBigName.text = Names.Substring(0, WaveNameIndexes);
                remTime -= Time.fixedDeltaTime;
                yield return 0;
                if (remTime <= 0.8f)
                {
                    WaveBigName.enabled = !WaveBigName.enabled;
                }
            }
            WaveBigName.enabled = false;
        }
    }


    //ゲームダンジョンに間に合わせるためのポーズメニュー設定.
    //後で消そう..
    public void TogglePauseMode()
    {
        if (gDesc == GameStateDesc.InGame)
        {
            gDesc = GameStateDesc.PauseMenu;
        }
        else if (gDesc == GameStateDesc.PauseMenu)
        { 
            gDesc = GameStateDesc.InGame;
        }
    }

    //ゲームダンジョンに間に合わせるためのメニュー呼び出し設定.
    //後で消そう..
    public void ReturnToMainMenu()
    {
        if (gDesc != GameStateDesc.InGame && gDesc != GameStateDesc.PreGame)
        {
            SceneManager.LoadScene("Title");
        }
    }
}


//後々にこのhitDefParamsの項目を表示・非表示設定可能にしたい
//非表示にした項目は値未設定ならデフォルト値を使用
[SerializeField]
[System.Serializable]
public class hitDefParams
{
    public float Damage;
    public Vector3 velset;
    [SerializeField]
    public Vector2 hitStopTime;
    [SerializeField]
    public GameObject HitEff;
    //当てた敵のステート変更情報(負の数以下で変更しない)
    public int ChangeState_Enemy = 5000;

    //当たる数(1がデフォ)
    public int maxEntityHits = 1;

    //敵がプレイヤーのステート名を参照するか？
    public bool enemyRefsPlayerNum = false;

    //プレイヤーのステート変更情報(負の数以下で変更しない)
    public int ChangeState_Player = -1;

    //どういう姿勢に当たるか？　など. "S"tanding "A"ir, "L"aying の頭文字指定
    //また、"F"は Fall状態のフラッグがあるキャラにHit, "E"veryはフラッグ問わず全部当たる.
    public string HitPhysFlag = "SA";

    //どういう動きに当たるか？　など やられ判定のときに追撃しないようにしたりとか.
    public string HitMoveFlag = "IA";

    public List<int> HitExcludeList;
}

