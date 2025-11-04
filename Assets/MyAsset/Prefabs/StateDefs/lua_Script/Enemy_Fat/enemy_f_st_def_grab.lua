local Entity = CS.Entity;
local Debug = CS.UnityEngine.Debug;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;

-- ステート変更のファンクション (10)
function QueuedStateID_Throw_Start(in_entity)
    verd = {}
    CurrentTime = LC:CheckStateTime(in_entity)
    C_animTime = LC:CheckAnimTime(in_entity)
    AnimEndTime = LC:CheckAnimEndTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    -- Accels.
    if( C_animTime > 7 and C_animTime < 8) then
        table.insert( verd, 3 ) 
    end
    
    -- check hitdef at duration
    if(C_animTime > 7 and C_animTime < 18 and CurrentAnimID == 2) then
        table.insert( verd, 1 )
    end
    -- animation end
    if( AnimEndTime - C_animTime < 6 and CurrentAnimID == 2) then
        table.insert( verd, 2 )
    end
    return verd
end

-- ステート変更のファンクション (11)
    function QueuedStateID_ThrowHit_Player_OnProcess(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        CurrentTime = LC:CheckStateTime(in_entity)
        C_animTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentTime == 0 ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        -- change to idle. also damage them and check the rest frametime
        if(  C_animTime > 4 and C_animTime < 5 and selfOnGrd == true) then
            table.insert(verd , 1)
        end
        if(AnimEndTime - C_animTime < 1 and selfOnGrd == true and CurrentAnimID == 3) then
            table.insert(verd, 2)
        end
    return verd
end 

-- ステート変更のファンクション (11)
    function QueuedStateID_ThrowHit_Player_EndProcess(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        C_animTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentTime == 0 ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if(AnimEndTime - C_animTime < 7 and CurrentAnimID == 4) then
            table.insert(verd, 1)
        end
    return verd
end 

-- ステート変更のファンクション (8)
-- this state is edited for enemy
    function QueuedStateID_ThrowHit_Enemy_OnProcess(in_entity)
        verd = {}
        
        ControlledEntity = in_entity.controlledEntity
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        Enemy_C_EntityStateID = LC:CheckStateDefID(ControlledEntity)
        if( CurrentR == 0  and not (CurrentAnimID == 5) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if( Enemy_C_EntityStateID == 7) then
            table.insert( verd, 1 )
        else 
            table.insert( verd, 2 )
        end
    return verd
end 

-- ステート変更のファンクション (8)
-- this state is edited for enemy
    function QueuedStateID_ThrowHit_Enemy_EndProcess(in_entity)
        verd = {}
        
        ControlledEntity = in_entity.controlledEntity
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        Enemy_C_animTime = LC:CheckAnimTime(ControlledEntity)
        Enemy_AnimEndTime = LC:CheckAnimEndTime(ControlledEntity)
        if( CurrentR == 0  and not (CurrentAnimID == 5050) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if( CurrentR > 15) then
            table.insert( verd, 1 )
        else 
            table.insert( verd, 2 )
        end
    return verd
end 

function Accel_Start(in_entity)    
    outs = {}

    Physvel3 = Vector3(0,0,0)

    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    Physvel3.x = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).x  * 300
    Physvel3.z = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).z  * 300

    table.insert(outs, Physvel3)
    return outs
end

function ChokerSped(in_entity)
    outs = {}
    CurrentTime = LC:CheckStateTime(in_entity)
    C_animTime = LC:CheckAnimTime(in_entity)
    AnimEndTime = LC:CheckAnimEndTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    trf = Vector3.ProjectOnPlane(in_entity.transform.forward, Vector3.up).normalized
    
    retVec = Vector3(0,0,0)
    retVec = (trf) * 10 + Vector3.up * 90
    table.insert(outs,retVec)
    return outs
end

--Get Choke hand positions.
function ChockAnim_Track(in_entity)  
    outs = {}
    ControlledEntity = in_entity.controlledEntity

    --tracks hand position.
    tr_Choked = LC:getEntityBoneTransform(in_entity,"spine")
    tr_Choker_L = LC:getEntityBoneTransform(ControlledEntity,"hand.l").position
    tr_Choker_R = LC:getEntityBoneTransform(ControlledEntity,"hand.r").position
    Enemy_C_animTime = LC:CheckAnimTime(ControlledEntity)
    Enemy_AnimEndTime = LC:CheckAnimEndTime(ControlledEntity)
    tr_Choker_All = ((tr_Choker_L + tr_Choker_R) / 2)
    diffPos =  tr_Choker_All - tr_Choked.position

    --tracks throwing vect.
    ThrowingVect = 
    Vector3.ProjectOnPlane(ControlledEntity.transform.forward, Vector3.up).normalized
    -- Debug.Log(ThrowingVect)
    Debug.Log(Enemy_AnimEndTime - Enemy_C_animTime)

    ThrowingVect = (ThrowingVect * 1 + Vector3.up * .2) * 10


    table.insert(outs, diffPos)
    table.insert(outs, ThrowingVect)
    return outs
end