
local Debug = CS.UnityEngine.Debug;

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
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimTime = LC:CurrentAnimTime(in_entity);
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentR == 0  and not (CurrentAnimID == 5050) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if ( CurrentR > 12 and selfOnGrd) then 
            table.insert( verd, 1 )
        end
        Debug.Log("Loaded PlayerStates")
    return verd
end 