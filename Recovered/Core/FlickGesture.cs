using System.Numerics;
namespace IQuarters.Core;

// Original ARMv7 QuarterTrigger.FixedUpdate 0x25685c–0x256cec.
// Saved field values in physics.json override constructor defaults.
public sealed class FlickGesture
{
    public const float Sensitivity = 1.8f; // debugTargetRange, divisor, not firing threshold
    public const float Exponent = .1f; // debugFlickPower, not shotPowerPower (shake only)
    public const float VerticalScale = .02f;
    public const float TriggerPower = .9f; // magThreshFlick
    public static readonly float MaximumPower = MathF.Pow(360 * VerticalScale / Sensitivity, Exponent);
    bool tracking, fired;
    public void Begin() { tracking=true; fired=false; }
    public void Cancel() { tracking=false; fired=false; }
    public bool Sample(Vector2 delta, out float power, out float sideways)
    {
        power=sideways=0;
        if(!tracking || fired || !float.IsFinite(delta.X) || !float.IsFinite(delta.Y) || delta.Y<=0)return false;
        float upward=Math.Min(delta.Y,360);
        power=MathF.Pow(upward*VerticalScale/Sensitivity,Exponent);
        sideways=4*delta.X/upward;
        if(!float.IsFinite(sideways)||power<=TriggerPower)return false;
        fired=true;return true;
    }
}
