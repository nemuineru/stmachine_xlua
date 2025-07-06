
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 9 ) then
            table.insert( verd, 10 ) 
        end

        -- Kick Combo
        if( CurrentTime > 12 and AttackCmd_b == true) then
            table.insert( verd, 2 ) 
        end

        -- Jab Combo
        if( CurrentTime > 8 and CurrentTime <= 12 and AttackCmd_b == true) then
            table.insert( verd, 3 ) 
        end
    return verd
end 