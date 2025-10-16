using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWaveManager : MonoBehaviour
{
    [System.Serializable]
    public class waveDesc
    {
        public List<Entity> spawnEntity;
        public int level = 0;
        public float remainTime = 90f;
        public int maxEntityNum = 3;
    }

    public List<waveDesc> waveLists;
    public bool isWaveChanged = false;
    int currentLevel = 0;

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
            currentDesc = waveLists.Find(wLs => wLs.level == currentLevel);
            //生成インデックスのリセット.
            currentSpawnIndex = 0;
            isWaveChanged = true;
        }
        //現在生成数を確認.
        int countSpawned = spawnedEntity.FindAll(et => et != null).Count;
        if (currentDesc != null)
        {
            //一度に生成するエンティティ量が指定の量を超えないまでは..
            if (countSpawned < currentDesc.maxEntityNum && currentSpawnIndex < currentDesc.spawnEntity.Count)
            {
                GameObject spawned =
                Instantiate(currentDesc.spawnEntity[currentSpawnIndex].gameObject, Vector3.zero, Quaternion.identity);
                currentSpawnIndex++;
            }
            //エンティティ全滅なら次のレベルへ.
            else if (currentSpawnIndex >= currentDesc.spawnEntity.Count && countSpawned == 0)
            {
                currentLevel++;
                isWaveChanged = false;
            }
        }
    }
}

