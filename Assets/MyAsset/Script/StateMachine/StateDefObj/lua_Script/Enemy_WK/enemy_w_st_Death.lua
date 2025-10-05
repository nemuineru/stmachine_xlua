
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション (5300, KOed)
function QueuedStateID_0(in_entity)
    verd = {}
    selfOnGrd = LC:isEntityOnGround(in_entity)
    CurrentTime = LC:CheckStateTime(in_entity)
    --アニメーション : KOアニメを再生
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    -- 地面につくまで再生する.
    if(selfOnGrd == true) then
        table.insert( verd, 1 )
    end
    return verd
end
