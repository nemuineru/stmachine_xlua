using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.VisualScripting;

public class enemyInput_Test : Action
{
    const string Name = "NavMesh Surface";
    [SerializeField]
    string findTags = "Player";
    NavMeshPath cPaths;

    GameObject mainObj;
    NavMeshSurface MainSurface;

    //パスの処理レート設定
    const int mapRouteFindTickRate = 10;
    const float rand_Prov = 0.001f;

    int currentTick = 0;

    //このメソッドクラスの起動時に一度だけ呼び出す.
    public override void OnAwake()
    {
        //最初にNavMeshSurfaceを発見する. 
        mainObj = GameObject.Find(Name);
        if (mainObj != null)
        {
            MainSurface = mainObj.GetComponent<NavMeshSurface>();
        }
    }

    Vector3 Dist = Vector3.zero;
    Vector2 Input = Vector2.zero;

    public override TaskStatus OnUpdate()
    {
        if (currentTick == mapRouteFindTickRate)
        {
            currentTick = 0;
            //path定義
            cPaths = new NavMeshPath();
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas);
            Vector3 nearPoint = hit.position;

            NavMesh.CalculatePath(nearPoint, GameObject.FindGameObjectWithTag(findTags).transform.position
            , NavMesh.AllAreas, cPaths);
        }
        else
        {
            currentTick++;
        }
        return base.OnUpdate();
    }


}
