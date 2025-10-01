
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
        if( math.abs(CurrentTime - 4) < 2  and in_entity.attrs.isStateHit == 0) then
            table.insert( verd, 10 ) 
        end
    return verd
end 

-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Knife(in_entity)

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 14 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( math.abs(CurrentAnimTime - 5) < 2  and in_entity.attrs.isStateHit == 0) then
            table.insert( verd, 10 ) 
        end
    return verd
end 


-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Axe(in_entity)

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        -- hitdef
        if ( CurrentAnimTime == 14 and in_entity.attrs.isStateHit == 0 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        -- endanimdef
        if(  CurrentTime > 3 and AnimEndTime - CurrentAnimTime < 8) then
            table.insert(verd , 2)
        end
    return verd
end 


-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Hammer(in_entity)

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        -- hitdef
        if ( math.abs(CurrentAnimTime - 17) <  3 and in_entity.attrs.isStateHit == 0 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        -- endanimdef
        if(  CurrentTime > 3 and AnimEndTime - CurrentAnimTime < 2) then
            table.insert(verd , 2)
        end
    return verd
end 