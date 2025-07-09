
local Entity = CS.Entity;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- COMMAND FUNCTION
function Queue_Cmd(in_entity)
    selfOnGrd = LC:isEntityOnGround(in_entity)
    JumpCommand = LC:CheckButtonPressed(in_entity, "a_")
    AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
    AttackCmd_x = LC:CheckButtonPressed(in_entity, "x_")
    selfStTime = LC:CheckStateTime(in_entity) 
    stateID = in_entity.CurrentStateID

    verd = {}
    -- jump cmd
    if(selfOnGrd == true and AttackCmd_b == true and stateID == 0) then
        table.insert( verd, 1) 
    end

    if (selfOnGrd == true and AttackCmd_x == true and stateID == 0) then 
        table.insert( verd, 2 )
    end

    if (selfOnGrd == true and AttackCmd_b == true 
    and stateID == 1 and selfStTime > 6) then 
        table.insert( verd, 4 )
    end
    if( selfOnGrd == false and AttackCmd_x == true and stateID == 50 ) then
        table.insert( verd, 3 ) 
    end

    if( selfOnGrd == false and AttackCmd_b == true and stateID == 50 ) then
        table.insert( verd, 5 ) 
    end
    return verd
end


function LuaOutput(in_entity)    
    outs = {}

    vel2 = Vector2(0,0)

    --オブジェクトのRigidBodyを取得する.
    vel3 = in_entity.rigid.velocity    
    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    vel_relate_r = in_entity.transform.right
    vel2.x = Vector3.Dot(vel3,vel_relate_r)
    vel2.y = Vector3.Dot(vel3,vel_relate_f)


    table.insert(outs, vel2)
    return outs
end
