
local Entity = CS.Entity;
local Vector3 = UnityEngine.Vector3;
local Debug = UnityEngine.Debug;
-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        selfOnGrd = LC:isEntityOnGround(in_entity)
        selfJump = LC:CheckButtonPressed()
        selfStTime = LC:CheckStateTime(in_entity) 

        verd = {}
        if (selfOnGrd == true and selfJump == true) then 
            table.insert( verd, 1 )
        end

        if(selfOnGrd == true) then
            table.insert( verd, 2 )
        end

        if( LC:CheckStateTime(in_entity) > 0 ) then
            table.insert( verd, 0 ) 
        end
    return verd
    function LuaOutput(in_entity)
        outs = {}
        Vector3 vel3 = in_entity.rigidbody.velocity
        table.insert(out)
    return out
end
