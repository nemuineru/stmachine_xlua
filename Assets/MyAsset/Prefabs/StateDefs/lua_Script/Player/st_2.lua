
local Vector3 = CS.UnityEngine.Vector3;
local Transform = CS.UnityEngine.Transform;
local Debug = CS.UnityEngine.Debug;

-- ステート変更のファンクション
    function QueuedStateID(in_entity)

        SoundTime = in_entity.attrs.isSoundNotPlayed == 0
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
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

-- ステート変更のファンクション ナイフ版
    function QueuedStateID_Knife(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        if ( CurrentTime > 20 ) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        if( math.abs(CurrentAnimTime - 5) < 2  and in_entity.attrs.isStateHit == 0) then
            table.insert( verd, 10 ) 
        end
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 


-- ステート変更のファンクション ハチェット
    function QueuedStateID_Axe(in_entity)

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0 and CurrentTime > 16 and CurrentAnimID == 6
        -- hitdef
        if( math.abs(CurrentAnimTime - 15) < 4 and in_entity.attrs.isStateHit == 0 and CurrentAnimID == 6) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        -- physics
        if( CurrentTime == 16 ) then
            table.insert( verd, 3 ) 
        end
        -- endanimdef
        if(  CurrentTime > 3 and AnimEndTime - CurrentAnimTime < 8) then
            table.insert(verd , 2)
        end
        -- sound play
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 

function Accel_Start(in_entity)    
    outs = {}

    Physvel3 = Vector3(0,0,0)

    --オブジェクトの正面方向・右方向を考え、Dotで計算.
    vel_relate_f = in_entity.transform.forward
    Physvel3.x = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).x  * 220
    Physvel3.z = Vector3.ProjectOnPlane(vel_relate_f,Vector3.up).z  * 220

    table.insert(outs, Physvel3)
    return outs
end

-- ステート変更のファンクション ハンマー
    function QueuedStateID_Hammer(in_entity)

        verd = {}
        CurrentAnimTime = LC:CheckAnimTime(in_entity);
        AnimEndTime = LC:CheckAnimEndTime(in_entity);

        CurrentTime = LC:CheckStateTime(in_entity)
        CurrentAnimID = LC:CheckAnimID(in_entity)
        SoundTime = in_entity.attrs.isSoundNotPlayed == 0 and CurrentAnimTime > 14 and CurrentAnimID == 7
        -- hitdef
        if ( math.abs(CurrentAnimTime - 17) <  2 and in_entity.attrs.isStateHit == 0 and CurrentAnimID == 7) then 
            table.insert( verd, 1 )
        end
        if( CurrentTime == 0 ) then
            table.insert( verd, 0 ) 
        end
        -- endanimdef
        if(  CurrentTime > 3 and AnimEndTime - CurrentAnimTime < 2) then
            table.insert(verd , 2)
        end
        -- sound play
        if(SoundTime) then
            table.insert( verd, 100)
        end
    return verd
end 