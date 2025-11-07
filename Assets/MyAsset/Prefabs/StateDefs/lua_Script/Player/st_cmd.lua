
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

    -- yの離し判定
    AttackCmd_y_Released = LC:CheckButtonPressed(in_entity, "y^")
    -- yの押し判定
    AttackCmd_y = LC:CheckButtonPressed(in_entity, "y_")
    AttackCmd_y_Pressed = LC:CheckButtonPressed(in_entity, "y")

    -- bの押し判定 (ボンバー.)
    AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")

    -- 投げ技. 今はいらないかも..
    AttackCmd_b_x_doublePress = LC:CheckButtonPressed(in_entity, "z_")

    selfStTime = LC:CheckStateTime(in_entity) 
    stateID = in_entity.CurrentStateID
    chargeVal = in_entity.status.ChargeTime

    isStateIDCombo = (stateID >= 0 and stateID <= 3)

    isBombChargeFull = (in_entity.status.currentEnergy >= 25) and not (stateID == 100)
    isAlive = in_entity.attrs.alive

    verd = {}
    -- combo_1 cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 0 and isAlive) then
        table.insert( verd, 1) 
    end

    -- combo_2 cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 1 and 
    selfStTime > 4 and in_entity.attrs.isStateHit > 0 and isAlive) then
        table.insert( verd, 2) 
    end

    -- combo_finisher cmd
    if(selfOnGrd == true and AttackCmd_x == true and stateID == 2 
    and selfStTime > 4 and in_entity.attrs.isStateHit > 0 and isAlive) then
        table.insert( verd, 3) 
    end

    -- Hard_1 cmd
    if (selfOnGrd == true and AttackCmd_y_Released == true 
    and isStateIDCombo and chargeVal < 0.5 and isAlive) then 
        table.insert( verd, 5 )
    end

    -- Hard_2 cmd
    if (selfOnGrd == true and AttackCmd_y_Released == true and isStateIDCombo and chargeVal >= 0.5 and chargeVal < 1.0 and isAlive) then 
        table.insert( verd, 6 )
    end

    -- Hard_3 cmd
    if (selfOnGrd == true and AttackCmd_y_Released == true and isStateIDCombo and chargeVal >= 1.0 and isAlive) then 
        table.insert( verd, 7 )
    end

    -- Bomber Command
    if(isBombChargeFull and AttackCmd_b and in_entity.controlledEntity == nil and in_entity.attrs.alive and isAlive) then 
        table.insert( verd, 100 )        
    end

    -- throwing command
    -- if( selfOnGrd == true and AttackCmd_b_x_doublePress == true) then
    --    table.insert( verd, 10)
    -- end

    -- air_combo cmd
    if( (selfOnGrd == false and AttackCmd_x == true and stateID == 50) or 
        (stateID == 20 and AttackCmd_x == true and selfStTime > 4 and in_entity.attrs.isStateHit > 3) and isAlive) then
        table.insert( verd, 20 ) 
    end

    -- air_Hard cmd
    if(( selfOnGrd == false and AttackCmd_y_Released == true and stateID == 50 ) or 
        (stateID == 20 and AttackCmd_y_Released == true and selfStTime > 4 and in_entity.attrs.isStateHit > 3) and isAlive) then
        table.insert( verd, 25 ) 
    end    

    -- chargeUp Checker
    if( AttackCmd_y_Pressed == true and stateID < 5000 ) then
        table.insert( verd, 30 ) 
    end    

    --chargeUp Releasement Checks
    if( AttackCmd_y_Released == true or stateID >= 5000 ) then
    table.insert( verd, 31 ) 
    end

    --jump and Jumpcancel on Cmd
    JumpCommand = LC:CheckButtonPressed(in_entity, "a_")
    if( stateID >= 1 and stateID <= 3 and in_entity.attrs.isStateHit > 0 and JumpCommand and isAlive) then
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
