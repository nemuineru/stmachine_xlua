using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

// A behaviour that is attached to a playable
public class humanInput : Action
{
    //check for connected Entity
    Entity entity;
    public override TaskStatus OnUpdate()
    {
        if (entity != null)
        {
            entity = gameObject.GetComponent<Entity>();
        }
        return base.OnUpdate();
    }
}
