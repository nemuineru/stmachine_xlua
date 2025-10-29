
local Debug = CS.UnityEngine.Debug;
local Vector3 = CS.UnityEngine.Vector3;

-- ステート変更のファンクション(投げ_1)
    function QueuedStateID(in_entity)

        verd = {}
        --スキを見せるように.
        AnimTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentTime = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if ( math.abs(AnimTime - 14) < 1) then 
            table.insert( verd, 1 )
        end
        if ( math.abs(AnimTime - 16) < 3 and in_entity.attrs.isStateHit == 0 and CurrentAnimID == 1) then 
            table.insert( verd, 2 )
        end
        --Init判定
        if( AnimEndTime - AnimTime < 1) then
            table.insert( verd, 10 ) 
        end
    return verd
end 


function LuaOutput(in_entity)    
    outs = {}

    Physvel3 = Vector3(0,0,0)

    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    Physvel3.x = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).x  * 120
    Physvel3.z = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).z  * 120

    table.insert(outs, Physvel3)
    return outs
end
