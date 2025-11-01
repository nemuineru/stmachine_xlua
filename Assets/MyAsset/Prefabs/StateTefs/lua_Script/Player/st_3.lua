
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 2 and math.abs(CurrentTime - 3) < 2 and in_entity.attrs.isStateHit == 0 ) then
            table.insert( verd, 3 ) 
        end
        if ( CurrentTime > 1000 and CurrentTime <= 12 and AttackCmd_b ) then 
            table.insert( verd, 2 )
        end
        if ( CurrentTime > 12 ) then 
            table.insert( verd, 1 )
        end
        -- sound sound..
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

-- 地上ハイキック時（コンボフィニッシャー）のステートコマンド..
    function QueuedStateID_GrdCombo3_Finisher(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        AttackCmd_b = LC:CheckButtonPressed(in_entity, "b_")
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 2 and math.abs(CurrentTime - 4) < 3 and in_entity.attrs.isStateHit == 0 ) then
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
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        selfOnGrd = LC:isEntityOnGround(in_entity)
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 5 and math.abs(CurrentTime - 7) < 3  and in_entity.attrs.isStateHit == 0) then
            table.insert( verd, 3 ) 
        end
        if ( selfOnGrd ) then 
            table.insert( verd, 1 )
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

-- ステート変更のファンクション
    function QueuedStateID_J(in_entity)
        verd = {}
        CurrentTime = LC:CheckStateTime(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0 and CurrentTime > 6
        selfOnGrd = LC:isEntityOnGround(in_entity)
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( CurrentTime > 4 and math.abs(CurrentTime - 8) < 2  and in_entity.attrs.isStateHit == 0) then
            table.insert( verd, 3 ) 
        end
        if ( selfOnGrd ) then 
            table.insert( verd, 1 )
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 
