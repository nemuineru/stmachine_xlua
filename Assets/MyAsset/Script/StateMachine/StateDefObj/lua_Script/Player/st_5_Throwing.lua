
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション (10)
function QueuedStateID_Throw(in_entity)
    verd = {}
    CurrentR = LC:CheckStateTime(in_entity)
    CurrentAnimID = LC:CheckAnimID(in_entity)
    CurrentAnimTime = LC:CurrentAnimTime(in_entity);
    CurrentTime = LC:CheckStateTime(in_entity)
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    if(selfOnGrd == true) then
        table.insert( verd, 1 )
    end
    return verd
end

-- ステート変更のファンクション (11)
    function QueuedStateID_ThrowHit_Player(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimTime = LC:CurrentAnimTime(in_entity);
        CurrentAnimID = LC:CheckAnimID(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity);
        if( CurrentR == 0 ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        -- change to idle. also damage them and check the rest frametime
        if( AnimEndTime - CurrentAnimTime < 2) then
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
    return verd
end 