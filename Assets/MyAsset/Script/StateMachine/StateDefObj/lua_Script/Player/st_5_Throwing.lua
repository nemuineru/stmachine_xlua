local Entity = CS.Entity;
local Debug = CS.UnityEngine.Debug;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;

-- ステート変更のファンクション (10)
function QueuedStateID_Throw(in_entity)
    verd = {}
    CurrentTime = LC:CheckStateTime(in_entity)
    C_animTime = LC:CheckAnimTime(in_entity)
    AnimEndTime = LC:CheckAnimEndTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    
    -- check hitdef
    if(C_animTime == 6) then
        table.insert( verd, 1 )
    end
    -- animation end
    if( AnimEndTime - C_animTime < 1 and CurrentAnimID == 10) then
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
        Debug.Log(f)
        if( CurrentR == 0  and not (CurrentAnimID == 5050) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if( Enemy_AnimEndTime - Enemy_C_animTime < 15) then
            table.insert( verd, 1 )
        end
    return verd
end 

--Get Choke hand positions.
function ChockAnim_Track(in_entity)    
    outs = {}
    ControlledEntity = in_entity.controlledEntity

    tr_Choked = in_entity.getBoneTransform("neck")
    tr_Choker = ControlledEntity.getBoneTransform("hand.L")

    diffPos = tr_Choked.position - tr_Choker.position    

    table.insert(outs, diffPos)
    return outs
end