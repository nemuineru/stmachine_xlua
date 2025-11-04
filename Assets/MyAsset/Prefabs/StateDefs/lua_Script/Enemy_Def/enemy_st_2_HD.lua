local Entity = CS.Entity;
local Debug = CS.UnityEngine.Debug;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;


-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        --スキを見せるように.
        CurrentTime = LC:CheckStateTime(in_entity)
        AnimTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0 and  math.abs(AnimTime - 13) < 3 and CurrentAnimID == 1
        if ( AnimEndTime - AnimTime < 6 ) then 
            table.insert( verd, 3 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        -- HitDef発生判定
        if( math.abs(AnimTime - 14) < 2  and in_entity.attrs.isStateHit == 0 and CurrentAnimID == 1) then
            table.insert( verd, 10 ) 
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Knife(in_entity)

        verd = {}
        table.insert (verd, 10000);
        CurrentTime = LC:CheckStateTime(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( math.abs(CurrentTime - 5) < 2  and not in_entity.attrs.isStateHit) then
            table.insert( verd, 10 ) 
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 


function LuaOutput(in_entity)
    outs = {}
    CurrentTime = LC:CheckStateTime(in_entity)
    C_animTime = LC:CheckAnimTime(in_entity)
    AnimEndTime = LC:CheckAnimEndTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    trf = Vector3.ProjectOnPlane(in_entity.transform.forward, Vector3.up).normalized
    
    retVec = Vector3(0,0,0)
    retVec = (trf) * 230
    table.insert(outs,retVec)
    return outs
end