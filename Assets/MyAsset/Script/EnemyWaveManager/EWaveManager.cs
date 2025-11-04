using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class EWaveManager : MonoBehaviour
{
    [SerializeField]
    TMP_Text LevelUI;
    
    public GameObject EnemySpawnEffect;

    public List<Transform> EnemySpawnPoints;

    public enum GameType
    {
        Practice,
        WaveMode
    }

    [SerializeField]
    public GameType gameType; 

    [System.Serializable]
    public class waveDesc
    {
        public List<Entity> spawnEntity;
        public int minlevel = 0;
        public int maxlevel = 1;
        public float remainTime = 90f;
        public int maxEntityNum = 3;
    }

    public List<waveDesc> waveLists;
    public bool isWaveChanged = false;

    public int MaxLevel = 12;
    public int currentLevel = 0;

    waveDesc currentDesc;
    List<Entity> spawnedEntity = new List<Entity>();
    int currentSpawnIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
                //シード値の完全Random(日時指定.)
                Random.InitState(System.DateTime.Now.Millisecond);
    }
    float spawnSec = 0.14f;
    float currentSpawnSec = 0;
    // Update is called once per frame
    void Update()
    {
        //ウェーブ変化時にエンティティオブジェクト生成.
        if (isWaveChanged == false)
        {
            //レベル設定.
            Debug.Log("finding Level");

            //設定したCurrentDescをリセット
            currentDesc = null;

            List<waveDesc> comfirmableDesc =
            waveLists.FindAll(wLs => wLs.minlevel <= currentLevel && wLs.maxlevel >= currentLevel);
            //Debug.Log(" descCount : " + comfirmableDesc.Count);
            if (comfirmableDesc.Count != 0)
            {
                gameState.self.Player.status.currentHP =
                Mathf.Min(Mathf.Lerp(gameState.self.Player.status.currentHP ,gameState.self.Player.status.maxHP, 0.12f) + 2f,
                gameState.self.Player.status.maxHP);
                //ランダムセレクト.
                currentDesc = comfirmableDesc[Random.Range(0, comfirmableDesc.Count)];
                //生成インデックスのリセット.
                currentSpawnIndex = 0;
                isWaveChanged = true;
                //Debug.Log(" Selected as : " + currentDesc.spawnEntity.Count);
            }
        }
        //現在生成数を確認.
        int countSpawned = spawnedEntity.FindAll(et => et != null).Count;
        if (currentDesc != null && gameState.self.gDesc == gameState.GameStateDesc.InGame && currentSpawnSec > spawnSec && EnemySpawnPoints.Count > 0)
        {
            Debug.Log("Decriptor found - LV." + currentDesc.minlevel + " - " + currentDesc.maxlevel);
            //一度に生成するエンティティ量が指定の量を超えないまでは..
            if (countSpawned < currentDesc.maxEntityNum && currentSpawnIndex < currentDesc.spawnEntity.Count)
            {
                int RandomPos = Random.Range(0, EnemySpawnPoints.Count - 1);

                GameObject spawned =
                Instantiate
                (currentDesc.spawnEntity[currentSpawnIndex].gameObject,
                EnemySpawnPoints[RandomPos].position, Quaternion.identity);
                if (EnemySpawnEffect != null)
                {
                    //生成時のエフェクト生成.
                    Instantiate(EnemySpawnEffect,
                EnemySpawnPoints[RandomPos].position, Quaternion.identity);
                }
                spawnedEntity.Add(spawned.GetComponent<Entity>());
                currentSpawnIndex++;
            }
            //エンティティ全滅なら次のレベルへ.
            else if (currentSpawnIndex >= currentDesc.spawnEntity.Count && countSpawned == 0)
            {
                if (gameType == GameType.WaveMode)
                {
                    currentLevel++;
                    if (currentLevel > MaxLevel)
                    {
                        gameState.self.gDesc = gameState.GameStateDesc.Finished;
                    }
                    else
                    {
                        StartCoroutine(gameState.self.ShowWaveNames("Wave " + currentLevel.ToString()));
                    }
                }
                isWaveChanged = false;
            }
            currentSpawnSec = 0;
        }
        else
        {
            //Debug.Log("Decriptor not found.");
            currentSpawnSec += Time.fixedDeltaTime;
        }
        LevelUI.text = currentLevel != 0 ? "Wave " + (currentLevel).ToString() + " / " + MaxLevel.ToString() : "Practice";
    }
}

