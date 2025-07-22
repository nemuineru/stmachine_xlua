using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("MyAsset")]
// A behaviour that is attached to a playable
public class humanInput : Action
{
    //check for connected Entity
    Entity entity;
    [SerializeField]
    SharedInt debuggerInt;
    public override void OnAwake()
    {
        if (entity == null)
        {
            entity = gameObject.GetComponent<Entity>();
        }
        base.OnAwake();
    }

    //プレイヤー専用.
    public override void OnStart()
    {
        if (entity != null)
        {
            debuggerInt = entity.entityInput.Execute_Entity_Player(0);
        }
        //return base.OnUpdate();
    }
}
