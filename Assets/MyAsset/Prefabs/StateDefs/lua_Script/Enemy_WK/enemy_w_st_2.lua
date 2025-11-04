
local Debug = CS.UnityEngine.Debug;
local Vector3 = CS.UnityEngine.Vector3;

-- ステート変更のファンクション(瓶投げ)
    function QueuedStateID(in_entity)

        verd = {}
        --スキを見せるように.
        AnimTime = LC:CheckAnimTime(in_entity)
        AnimEndTime = LC:CheckAnimEndTime(in_entity)
        CurrentTime = LC:CheckStateTime(in_entity)
        if ( AnimEndTime - AnimTime < 8 and CurrentTime > 10) then 
            table.insert( verd, 2 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        --Throwing判定
        if( CurrentTime == 48) then
            table.insert( verd, 1 ) 
        end
    return verd
end 


function ThrowPos(in_entity)
    outs = {}
    thrower_fw = in_entity.transform.forward
    table.insert(outs ,thrower_fw * 4 + Vector3.up * 4)
    thrower_hand = LC:getEntityBoneTransform(in_entity,"hand.r")
    table.insert(outs ,thrower_hand.position)
    return outs
end