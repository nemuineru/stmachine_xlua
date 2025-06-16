
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)
        verd = {}
        if( LC:CheckStateTime(in_entity) == 0 ) then
            table.insert( verd, 0 ) 
        end
        if ( LC:CheckStateTime(in_entity) > 8 ) then 
            table.insert( verd, 1 )
        end
    return verd
end 