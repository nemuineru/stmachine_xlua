
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 2 and math.abs(CurrentTime - 3) < 2 and not in_entity.attrs.isStateHit ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 1000 and CurrentTime <= 12 and AttackCmd_b ) then 
            table.insert( verd, 2 )
        end
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

-- 地上ハイキック時（コンボフィニッシャー）のステートコマンド..
    function QueuedStateID_GrdCombo3_Finisher(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 2 and math.abs(CurrentTime - 4) < 3 and not in_entity.attrs.isStateHit ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if(SoundTime) then
            table.insert( verd, 100)
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
        if( CurrentTime > 5 and math.abs(CurrentTime - 7) < 3  and not in_entity.isStateHit) then
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
        if( CurrentTime > 4 and math.abs(CurrentTime - 5) < 3  and not in_entity.isStateHit) then
            table.insert( verd, 3 ) 
        end
        if ( selfOnGrd ) then 
            table.insert( verd, 1 )
        end
    return verd
end 
