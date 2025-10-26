
local Entity = CS.Entity;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
function QueuedStateID(in_entity)
    selfOnGrd = LC:isEntityOnGround(in_entity)
    AttackCmd_attack = LC:CheckButtonPressed(in_entity, "a")
    AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
    ShiftCMD = LC:CheckButtonPressed(in_entity, "y")
    selfStTime = LC:CheckStateTime(in_entity) 

    verd = {}

    -- jump cmd
    if(selfOnGrd == true and AttackCmd_attack == true) then
        table.insert( verd, 3) 
    end

    if(selfOnGrd == true) then
        table.insert( verd, 2 )
    end

    if( LC:CheckStateTime(in_entity) > 1 ) then
        table.insert( verd, 0 ) 
    end
    -- シフト移動の登録.
    if( ShiftCMD == false ) then
        table.insert( verd, 800 ) 
    end
    -- idleのanimを指定する
    -- entityに登録されたmixerの数が0のときは緊急。
    if( LC:CheckStateTime(in_entity) == 1 or LC:CheckAnimationsListNum(in_entity) == 0) then
        -- Debug.Log("Init Anim Loaded")
        table.insert( verd, 100 ) 
    end
    if(not in_entity.attrs.alive) then
        table.insert(verd , 5100)
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
