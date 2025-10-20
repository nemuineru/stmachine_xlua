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
    public int currentLevel = 0;

    waveDesc currentDesc;
    List<Entity> spawnedEntity = new List<Entity>();
    int currentSpawnIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

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
            if (comfirmableDesc.Count != 0)
            {
                //ランダムセレクト.
                currentDesc = comfirmableDesc[Random.Range(0,comfirmableDesc.Count - 1)];
                //生成インデックスのリセット.
                currentSpawnIndex = 0;
                isWaveChanged = true;
            }
        }
        //現在生成数を確認.
        int countSpawned = spawnedEntity.FindAll(et => et != null).Count;
        if (currentDesc != null)
        {
            Debug.Log("Decriptor found - LV." + currentDesc.minlevel + " - " + currentDesc.maxlevel );
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
                currentLevel++;
                StartCoroutine(gameState.self.ShowWaveNames("Wave " + currentLevel.ToString()));
                isWaveChanged = false;
            }
        }
        else
        {
            Debug.Log("Decriptor not found.");
        }
        LevelUI.text = currentLevel != 0 ? "Wave " + (currentLevel).ToString() : "Practice";
    }
}

