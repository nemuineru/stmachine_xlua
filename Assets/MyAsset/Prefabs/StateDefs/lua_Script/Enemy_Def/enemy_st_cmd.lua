
local Entity = CS.Entity;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- COMMAND FUNCTION
function Queue_Cmd(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        JumpCommand = LC:CheckButtonPressed(in_entity, "a_")
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        selfStTime = LC:CheckStateTime(in_entity) 
        stateID = in_entity.CurrentStateID
        isAlive = in_entity.attrs.alive

        -- combo_1 cmd
        if(selfOnGrd == true and AttackCmd_b == true and stateID == 0 and isAlive) then
            table.insert( verd, 1) 
        end
        if(not isAlive and stateID <= 1) then 
            table.insert (verd, 5000)
        end

        -- air_combo cmd
        -- if( selfOnGrd == false and AttackCmd_b == true and stateID == 50 ) then
        --    table.insert( verd, 20 ) 
        -- end
    return verd
end


function LuaOutput(in_entity)    
    outs = {}

    vel3 = Vector2(0,0)

    --オブジェクトのRigidBodyを取得する.
    vel3 = in_entity.rigid.velocity    
    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    vel_relate_r = in_entity.transform.right
    vel3.x = Vector3.Dot(vel3,vel_relate_r)
    vel3.y = Vector3.Dot(vel3,vel_relate_f)


    table.insert(outs, vel3)
    return outs
end
