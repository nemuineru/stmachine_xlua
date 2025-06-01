
local Entity = CS.Entity;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
function QueuedStateID(in_entity)
    selfOnGrd = LC:isEntityOnGround(in_entity)
    selfJump = LC:CheckButtonPressed()
    selfStTime = LC:CheckStateTime(in_entity) 

    verd = {}
    if (selfOnGrd == true and selfJump == true) then 
        table.insert( verd, 1 )
    end

    if(selfOnGrd == true) then
        table.insert( verd, 2 )
    end

    if( LC:CheckStateTime(in_entity) > 0 ) then
        table.insert( verd, 0 ) 
    end
    return verd
end

function LuaOutput(in_entity)    
    outs = {}

    vel2 = Vector2(0,0)

    --オブジェクトのRigidBodyを取得する.
    vel3 = in_entity.rigid.velocity
    
    vel2.x = vel3.x
    vel2.y = vel3.z
    table.insert(outs, vel2)
    return outs
end
