local Entity = CS.Entity;
local Debug = CS.UnityEngine.Debug;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;

-- ステート変更のファンクション (100)
function QueuedStateID_100(in_entity)
    SoundTime = in_entity.attrs.isSoundNotPlayed == 0
    verd = {}
    CurrentAnimTime = LC:CheckAnimTime(in_entity);
    AnimEndTime = LC:CheckAnimEndTime(in_entity);

    CurrentTime = LC:CheckStateTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    if( math.abs(CurrentAnimTime - 9) <= 6 and CurrentAnimID == 100) then
        table.insert( verd, 2 ) 
    end
    if( (AnimEndTime - CurrentAnimTime) < 16 and CurrentAnimID == 100) then
        table.insert( verd, 10 ) 
    end
    if( math.abs(CurrentAnimTime - 4) < 1 and CurrentAnimID == 100 and SoundTime) then
        table.insert( verd, 100 ) 
    end
    return verd
end

function Accel(in_entity)
    outs = {}
    CurrentTime = LC:CheckStateTime(in_entity)
    C_animTime = LC:CheckAnimTime(in_entity)
    AnimEndTime = LC:CheckAnimEndTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    trf = Vector3.ProjectOnPlane(in_entity.transform.forward, Vector3.up).normalized
    
    retVec = Vector3(0,0,0)
    if( CurrentTime == 0) then
        retVec = trf * 200  
    end
    table.insert(outs,retVec)
    return outs
end
