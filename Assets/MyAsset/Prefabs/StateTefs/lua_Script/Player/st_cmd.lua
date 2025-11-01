
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
    AttackCmd_x = LC:CheckButtonPressed(in_entity, "x_")

    -- bの離し判定
    AttackCmd_b_Released = LC:CheckButtonPressed(in_entity, "b^")
    -- bの押し判定
    AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
    AttackCmd_b_Pressed = LC:CheckButtonPressed(in_entity, "b")
    AttackCmd_b_x_doublePress = LC:CheckButtonPressed(in_entity, "z_")

    selfStTime = LC:CheckStateTime(in_entity) 
    stateID = in_entity.CurrentStateID
    chargeVal = in_entity.status.ChargeTime

    isStateIDCombo = (stateID >= 0 and stateID <= 3)

    verd = {}
    -- combo_1 cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 0) then
        table.insert( verd, 1) 
    end

    -- combo_2 cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 1 and 
    selfStTime > 4 and in_entity.attrs.isStateHit > 0) then
        table.insert( verd, 2) 
    end

    -- combo_finisher cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 2 
    and selfStTime > 4 and in_entity.attrs.isStateHit > 0) then
        table.insert( verd, 3) 
    end

    -- Hard_1 cmd
    if (selfOnGrd == true and AttackCmd_b_Released == true 
    and isStateIDCombo and chargeVal < 0.5) then 
        table.insert( verd, 5 )
    end

    -- Hard_2 cmd
    if (selfOnGrd == true and AttackCmd_b_Released == true and isStateIDCombo and chargeVal >= 0.5 and chargeVal < 1.0) then 
        table.insert( verd, 6 )
    end

    -- Hard_3 cmd
    if (selfOnGrd == true and AttackCmd_b_Released == true and isStateIDCombo and chargeVal >= 1.0 and chargeVal < 2.0) then 
        table.insert( verd, 7 )
    end

    -- throwing command
    if( selfOnGrd == true and AttackCmd_b_x_doublePress == true) then
        table.insert( verd, 10)
    end

    -- air_combo cmd
    if( (selfOnGrd == false and AttackCmd_x == true and stateID == 50) or 
        (stateID == 20 and AttackCmd_x == true and selfStTime > 4 and in_entity.attrs.isStateHit > 3)) then
        table.insert( verd, 20 ) 
    end

    -- air_Hard cmd
    if(( selfOnGrd == false and AttackCmd_b_Released == true and stateID == 50 ) or 
        (stateID == 20 and AttackCmd_b_Released == true and selfStTime > 4 and in_entity.attrs.isStateHit > 3)) then
        table.insert( verd, 25 ) 
    end    

    -- chargeUp Checker
    if( AttackCmd_b_Pressed == true and stateID < 5000 ) then
        table.insert( verd, 30 ) 
    end    

    --chargeUp Releasement Checks
    if( AttackCmd_b_Released == true or stateID > 5000 ) then
    table.insert( verd, 31 ) 
    end

    --jump and Jumpcancel on Cmd
    JumpCommand = LC:CheckButtonPressed(in_entity, "a_")
    if( stateID >= 1 and stateID <= 3 and in_entity.attrs.isStateHit > 0 and JumpCommand) then
    table.insert( verd, 50 ) 
    end

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
