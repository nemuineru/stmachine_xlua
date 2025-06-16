
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        selfJump = LC:CheckButtonPressed()
        if ( CurrentTime > 9 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end

        -- Kick Combo
        if( CurrentTime == 9 and selfJump == true) then
            table.insert( verd, 2 ) 
        end
    return verd
end 