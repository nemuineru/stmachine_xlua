
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        selfJump = LC:CheckButtonPressed(in_entity, "b_")
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 3 ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 1000 and CurrentTime <= 12 and selfJump ) then 
            table.insert( verd, 2 )
        end
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
    return verd
end 