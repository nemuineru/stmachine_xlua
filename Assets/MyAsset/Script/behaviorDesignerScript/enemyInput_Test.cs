using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.VisualScripting;

public class enemyInput_Test : Action
{
    //ターゲット先・動作方向先
    [SerializeField]
    SharedVector3 v3_WalkTo;

    [SerializeField]
    SharedGameObject Target;

    //Commandに応じて、入力を与える.
    [SerializeField]
    SharedString Command;


    Vector3 v3_Target = Vector3.zero;

    int currentTick = 0;
    //パスの処理レート設定
    const int mapRouteFindTickRate = 10;
    const float rand_Prov = 0.001f;
    const float hitboxDist = 2f;

    Vector3 Dist = Vector3.zero;
    Vector2 Input = Vector2.zero;

//動かすEntity. これが付いたExternalBehaviorは必ずこれが付いてるはず
    Entity AIEntity;

    public override void OnAwake()
    {
        AIEntity = gameObject.GetComponent<Entity>();
    }


    public override TaskStatus OnUpdate()
    {
        if (currentTick > mapRouteFindTickRate)
        {
            //設定されたbool値がTrue・Rand01値が設定値以上ならコマンド送信 ->
            //今回は "a_"を入力するとした
            //AIEntity.entityInput.RecordInput_Enemy();
        }
        else
        {
            currentTick++;
        }
        return base.OnUpdate();
    }


}

namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject
{
    [TaskCategory("Unity/GameObject")]
    [TaskDescription("Finds a GameObject by tag only on Awake. Returns success if an object is found. this method is Expanded by nem")]
    public class FindWithTag_OnAwake : Action
    {
        [Tooltip("The tag of the GameObject to find")]
        public SharedString tag;
        [Tooltip("Should a random GameObject be found?")]
        public SharedBool random;
        [Tooltip("The object found by name")]
        [RequiredField]
        public SharedGameObject storeValue;

        public override void OnAwake()
        {
            if (random.Value) {
                var gameObjects = GameObject.FindGameObjectsWithTag(tag.Value);
                if (gameObjects == null || gameObjects.Length == 0) { return; }
                storeValue.Value = gameObjects[Random.Range(0, gameObjects.Length)];
            } else {
                storeValue.Value = GameObject.FindWithTag(tag.Value);
            }
            return;
        }

        public override void OnReset()
        {
            tag.Value = null;
            storeValue.Value = null;
        }
    }
}