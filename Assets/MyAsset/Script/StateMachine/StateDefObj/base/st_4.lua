
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション (50)
function QueuedStateID_0(in_entity)
    verd = {}
    selfOnGrd = LC:isEntityOnGround(in_entity)
    CurrentTime = LC:CheckStateTime(in_entity)
    if( CurrentTime == 0 ) then
        table.insert( verd, 0 ) 
    end
    if(selfOnGrd == true) then
        table.insert( verd, 1 )
    end
    return verd
end

-- ステート変更のファンクション (5000)
    function QueuedStateID_Hit(in_entity)
        verd = {}
        selfOnGrd = LC:isEntityOnGround(in_entity)
        CurrentR = LC:CheckStateTime(in_entity)
        if( CurrentR == 0 ) then
            -- Debug.Log("damage Amim")
            table.insert( verd, 0 ) 
        end
        if ( CurrentR > 12 and selfOnGrd) then 
            table.insert( verd, 1 )
        end
    return verd
end 