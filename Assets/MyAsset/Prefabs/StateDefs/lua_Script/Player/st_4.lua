
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション (50)
function QueuedStateID_0(in_entity)
    verd = {}
    selfOnGrd = LC:isEntityOnGround(in_entity)
    CurrentTime = LC:CheckStateTime(in_entity)
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    if(selfOnGrd == true) then
        table.insert( verd, 1 )
    end
    return verd
end

-- ステート変更のファンクション (5000)
    function QueuedStateID_Hit_Def(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentR == 0  and not (CurrentAnimID == 5000) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if ( CurrentR > 12 and selfOnGrd) then 
            table.insert( verd, 1 )
        end
            -- on death, don't revive.
        if ( CurrentR > 6 and in_entity.attrs.alive == false) then 
            table.insert( verd, 2 )
        end
    return verd
end 

-- ステート変更のファンクション,BlowOut (5050)
    function QueuedStateID_Hit_Air(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        CurrentR = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        if( CurrentR == 0  and not (CurrentAnimID == 5050) ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if ( CurrentR > 6 and selfOnGrd) then 
            table.insert( verd, 1 )
        end
    return verd
end 