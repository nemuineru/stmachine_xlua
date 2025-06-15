
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        Debug.Log(LC:CheckStateTime(in_entity))
        if ( LC:CheckStateTime(in_entity) > 10 ) then 
            table.insert( verd, 1 )
        end
        if( LC:CheckStateTime(in_entity) == 0 ) then
            table.insert( verd, 0 ) 
        end
    return verd
end 