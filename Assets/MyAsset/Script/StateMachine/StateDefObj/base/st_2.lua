
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 4  and not in_entity.isStateHit) then
            table.insert( verd, 10 ) 
        end
    return verd
end 

-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Knife(in_entity)

        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 5  and not in_entity.isStateHit) then
            table.insert( verd, 10 ) 
        end
    return verd
end 