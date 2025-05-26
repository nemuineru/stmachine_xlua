
-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        if ( LC:isEntityOnGround(in_entity) == true ) then 
            table.insert( verd, 1 )
        end
        if( LC:CheckStateTime(in_entity) > 0 ) then
            table.insert( verd, 0 ) 
        end
    return verd
end 