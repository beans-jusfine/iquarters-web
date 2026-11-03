using System.Numerics;
namespace IQuarters.Core;

public static class CameraFollow
{
    public static float Blend(float at60Hz,float seconds)=>1-MathF.Pow(1-at60Hz,Math.Max(0,seconds)*60);

    // SceneKit cameras face -Z. Fix the up axis in world space, as Unity LookRotation does.
    public static Quaternion LevelLook(Vector3 direction,Quaternion fallback)
    {
        if(direction.LengthSquared()<1e-10f)return fallback;
        direction=Vector3.Normalize(direction);
        float horizontal=MathF.Sqrt(direction.X*direction.X+direction.Z*direction.Z);
        float yaw;
        if(horizontal<1e-5f){
            var oldForward=Vector3.Transform(-Vector3.UnitZ,fallback);
            yaw=MathF.Atan2(-oldForward.X,-oldForward.Z);
        }else yaw=MathF.Atan2(-direction.X,-direction.Z);
        float pitch=MathF.Atan2(direction.Y,horizontal);
        return Quaternion.Normalize(Quaternion.CreateFromYawPitchRoll(yaw,pitch,0));
    }
    public static Quaternion Follow(Quaternion previous,Vector3 direction,float amount)
    {
        var target=LevelLook(direction,previous);
        var smoothed=Quaternion.Slerp(previous,target,Math.Clamp(amount,0,1));
        // Interpolating yaw/pitch quaternions can introduce roll. Rebuild with fixed world up.
        return LevelLook(Vector3.Transform(-Vector3.UnitZ,smoothed),target);
    }
}
