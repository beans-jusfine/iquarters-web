using System.Numerics;
namespace IQuarters.Core;

public static class ShakeGesture
{
    // QuarterTrigger.FixedUpdate 0x256384–0x256858: time-weighted raw acceleration,
    // including gravity, projected onto (0, -.25, -.9). Serialized field values.
    public static bool Sample(Vector3 acceleration,out float power,out float sideways)
    {
        power=sideways=0;
        if(!float.IsFinite(acceleration.X)||!float.IsFinite(acceleration.Y)||!float.IsFinite(acceleration.Z))return false;
        float projection=Vector3.Dot(new(0,-.25f,-.9f),acceleration);
        if(projection<=0)return false;
        power=MathF.Pow(projection/1.975f,.9f);
        sideways=Math.Clamp(acceleration.X,-2,2)*1.25f;
        return power>.8f;
    }
}
