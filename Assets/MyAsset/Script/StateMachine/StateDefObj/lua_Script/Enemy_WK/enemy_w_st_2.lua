
local Debug = CS.UnityEngine.Debug;
local Vector3 = CS.UnityEngine.Vector3;

-- ステート変更のファンクション(瓶投げ)
    function QueuedStateID(in_entity)

        verd = {}
        --スキを見せるように.
        AnimTime = LC:CheckAnimTime(in_entity)
        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 24 ) then 
            table.insert( verd, 2 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        --Throwing判定
        if( CurrentTime == 16) then
            table.insert( verd, 1 ) 
        end
    return verd
end 


function ThrowPos(in_entity)
    table = {}
    thrower_fw = in_entity.transform.forward
    table.insert(table ,thrower_fw + Vector3.up)
    thrower_hand = LC:getEntityBoneTransform(in_entity,"hand.r")
    table.insert(table ,thrower_hand.position)
    return table
end