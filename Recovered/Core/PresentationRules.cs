namespace IQuarters.Core;

public static class PresentationRules
{
    public static int ReplayCamera(int round,int requested)=>round switch {
        3 or 6=>3,8 when requested==1=>2,10 or 12 when requested==3=>1,_=>requested
    };
    // Native FixedUpdate 0x257f34–0x258050: Classic and the last success of the round.
    public static bool RoundStinger(bool practice,int madeBeforeShot,int multiplier)=>!practice&&multiplier>0&&madeBeforeShot==2;
    public static double HalfHeight(double width,double height)=>Math.Max(100,200d/3*height/Math.Max(1,width));
    public static double VerticalFieldOfView(double width,double height,double original=55)=>
        2*Math.Atan(Math.Tan(original*Math.PI/360)*Math.Max(1,(2d/3)*height/Math.Max(1,width)))*180/Math.PI;
    // Convert screen-pixel movement to the original 320x480 input coordinate scale.
    // Use the actual orthographic height so widescreen displays do not make flicks too weak.
    public const float DesktopMouseSensitivity=2f;
    public static float TouchScale(double width,double height)=>(float)(7.2*HalfHeight(width,height)/Math.Max(1,height));
}
