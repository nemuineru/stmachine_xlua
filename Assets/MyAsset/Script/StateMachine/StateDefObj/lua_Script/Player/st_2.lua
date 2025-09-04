
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
        if( math.abs(CurrentTime - 4) < 2  and not in_entity.attrs.isStateHit) then
            table.insert( verd, 10 ) 
        end
    return verd
end 

-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Knife(in_entity)

        verd = {}
        table.insert (verd, 10000);
        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( math.abs(CurrentTime - 5) < 2  and not in_entity.attrs.isStateHit) then
            table.insert( verd, 10 ) 
        end
    return verd
end 