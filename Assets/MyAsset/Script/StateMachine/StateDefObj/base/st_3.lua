
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 3 ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 1000 and CurrentTime <= 12 and AttackCmd_b ) then 
            table.insert( verd, 2 )
        end
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
    return verd
end 

-- 地上ハイキック時（コンボフィニッシャー）のステートコマンド..
    function QueuedStateID_GrdCombo3_Finisher(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 4 ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
    return verd
end 

-- ステート変更のファンクション
    function QueuedStateID_J_Knife(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        selfOnGrd = LC:isEntityOnGround(in_entity)
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 7 ) then
            table.insert( verd, 3 ) 
        end
        if ( selfOnGrd ) then 
            table.insert( verd, 1 )
        end
    return verd
end 

-- ステート変更のファンクション
    function QueuedStateID_J(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        selfOnGrd = LC:isEntityOnGround(in_entity)
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime == 5 ) then
            table.insert( verd, 3 ) 
        end
        if ( selfOnGrd ) then 
            table.insert( verd, 1 )
        end
    return verd
end 
