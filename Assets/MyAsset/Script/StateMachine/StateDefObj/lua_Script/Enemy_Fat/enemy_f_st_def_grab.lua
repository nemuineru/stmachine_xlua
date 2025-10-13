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
    if( C_animTime == 7) then
        table.insert( verd, 3 ) 
    end
    
    -- check hitdef at duration
    if(C_animTime > 7 and C_animTime < 18) then
        table.insert( verd, 1 )
    end
    -- animation end
    if( AnimEndTime - C_animTime < 6 and CurrentAnimID == 2) then
        table.insert( verd, 2 )
    end
    return verd
end

-- ステート変更のファンクション (11)
    function QueuedStateID_ThrowHit_Player(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        C_animTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentTime == 0 ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        -- change to idle. also damage them and check the rest frametime
        if( AnimEndTime - C_animTime < 2) then
            table.insert(verd , 1)
        end
        if(AnimEndTime - C_animTime > 1) then
            table.insert(verd, 2)
        end
    return verd
end 

-- ステート変更のファンクション (12)
-- this state is edited for enemy
    function QueuedStateID_ThrowHit_Enemy(in_entity)
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
        if( Enemy_AnimEndTime - Enemy_C_animTime < 15) then
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
    if( AnimEndTime - C_animTime < 20 and AnimEndTime - C_animTime > 14) then
        retVec = trf * 20  
    end
    if( CurrentTime == 0) then
        retVec = -trf * 100
    end
    table.insert(outs,retVec)
    return outs
end

--Get Choke hand positions.
function ChockAnim_Track(in_entity)  
    outs = {}
    ControlledEntity = in_entity.controlledEntity

    --tracks hand position.
    tr_Choked = LC:getEntityBoneTransform(in_entity,"neck")
    tr_Choker = LC:getEntityBoneTransform(ControlledEntity,"hand.l")
    Enemy_C_animTime = LC:CheckAnimTime(ControlledEntity)
    Enemy_AnimEndTime = LC:CheckAnimEndTime(ControlledEntity)
    diffPos = tr_Choker.position - tr_Choked.position

    --tracks throwing vect.
    ThrowingVect = 
    Vector3.ProjectOnPlane(ControlledEntity.transform.forward, Vector3.up).normalized
    -- Debug.Log(ThrowingVect)
    Debug.Log(Enemy_AnimEndTime - Enemy_C_animTime)

    if( Enemy_AnimEndTime - Enemy_C_animTime < 16) then
        diffPos = ThrowingVect * 0.2 + Vector3.up * 0.1
    end

    ThrowingVect = (ThrowingVect * 1 + Vector3.up * .2) * 220


    table.insert(outs, diffPos)
    table.insert(outs, ThrowingVect)
    return outs
end