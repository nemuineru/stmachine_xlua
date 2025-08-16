
local Entity = CS.Entity;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

function QueuedStateID(in_entity)
    verd = {}
    -- Debug.Log("Checking Entity Jumping");
    selfStTime =  LC:CheckStateTime(in_entity)
    selfOnGrd_f = LC:isEntityOnGround(in_entity)
    -- Debug.Log(selfStTime);
    if(selfOnGrd_f == true) then
        table.insert( verd, 1 )
    end
    -- idleのanimを指定する
    if(selfStTime == 0) then
        -- Debug.Log("Jumping Vect");
        table.insert( verd, 50 ) 
    end
    return verd
end