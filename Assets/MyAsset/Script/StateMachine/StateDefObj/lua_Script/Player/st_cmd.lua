
local Entity = CS.Entity;
local EntityStatus = CS.Entity.EntityStatus;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- COMMAND FUNCTION
function Queue_Cmd(in_entity)
    selfOnGrd = LC:isEntityOnGround(in_entity)
    JumpCommand = LC:CheckButtonPressed(in_entity, "a_")
    AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")

    -- xの離し判定
    AttackCmd_x = LC:CheckButtonPressed(in_entity, "x_")
    -- xの押し判定
    AttackCmd_x_Pressed = LC:CheckButtonPressed(in_entity, "x")
    AttackCmd_x_Released = LC:CheckButtonPressed(in_entity, "x^")

    selfStTime = LC:CheckStateTime(in_entity) 
    stateID = in_entity.CurrentStateID
    chargeVal = in_entity.status.ChargeTime

    verd = {}
    -- combo_1 cmd
    if(selfOnGrd == true and AttackCmd_b == true and stateID == 0) then
        table.insert( verd, 1) 
    end

    -- combo_2 cmd
    if(selfOnGrd == true and AttackCmd_b == true and stateID == 1 and 
    selfStTime > 4 and in_entity.attrs.isStateHit == true) then
        table.insert( verd, 2) 
    end

    -- combo_finisher cmd
    if(selfOnGrd == true and AttackCmd_b == true and stateID == 2 
    and selfStTime > 4 and in_entity.attrs.isStateHit == true) then
        table.insert( verd, 3) 
    end

    -- Hard_1 cmd
    if (selfOnGrd == true and AttackCmd_x_Released == true and stateID == 0) then 
        table.insert( verd, 5 )
    end

    -- air_combo cmd
    if( selfOnGrd == false and AttackCmd_b == true and stateID == 50 ) then
        table.insert( verd, 20 ) 
    end

    -- air_Hard cmd

    if( selfOnGrd == false and AttackCmd_x_Released == true and stateID == 50 ) then
        table.insert( verd, 25 ) 
    end    

    -- chargeUp Checker
    if( AttackCmd_x_Pressed == true and stateID < 5000 ) then
        table.insert( verd, 30 ) 
    end    

    --chargeUp Releasement Checks
    if( AttackCmd_x_Released == true or stateID > 5000 ) then
    table.insert( verd, 31 ) 
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
