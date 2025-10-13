
local Entity = CS.Entity;
local EntityStatus = CS.Entity.EntityStatus;
local Vector2 = CS.UnityEngine.Vector2;
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- FALL FUNCTIONS
function stID_FallDown(in_entity)
    verd = {}
    selfOnGrd = LC:isEntityOnGround(in_entity)
    CurrentTime = LC:CheckStateTime(in_entity)
    CurrentAnimTime = LC:CheckAnimTime(in_entity);
    CurrentAnimID =  LC:CheckAnimID(in_entity);
    AnimEndTime = LC:CheckAnimEndTime(in_entity);
    -- load the anims
    if( CurrentTime == 0 ) then
        table.insert( verd, 0 ) 
    end
    -- change to recovery. the check the rest frametime
    -- also check not dead
    if( CurrentAnimID == 5100 and 
    AnimEndTime - CurrentAnimTime < 8 and in_entity.attrs.alive) then
        table.insert(verd , 1)
    end    

    if( in_entity.attrs.alive == false) then
        table.insert(verd , 2)
    end
    return verd
end

-- FALL FUNCTIONS
function stID_FallRecov(in_entity)
    verd = {}
    selfOnGrd = LC:isEntityOnGround(in_entity)
    CurrentTime = LC:CheckStateTime(in_entity)
    CurrentAnimTime = LC:CheckAnimTime(in_entity);
    CurrentAnimID =  LC:CheckAnimID(in_entity);
    AnimEndTime = LC:CheckAnimEndTime(in_entity);
    -- load the first anims
    if( CurrentTime == 0) then
        table.insert( verd, 0 ) 
    end
    -- change to recovery. the check the rest frametime
    if( CurrentAnimID == 5101 and  AnimEndTime - CurrentAnimTime < 12) then
        table.insert(verd , 1)
    end
    return verd
end

function LuaOutput(in_entity)    
    outs = {}

    vel3 = Vector2(0,0)

    --オブジェクトのRigidBodyを取得する.
    vel3 = in_entity.rigid.velocity    
    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    vel_relate_r = in_entity.transform.right
    vel3.x = Vector3.Dot(vel3,vel_relate_r)
    vel3.y = Vector3.Dot(vel3,vel_relate_f)


    table.insert(outs, vel3)
    return outs
end
