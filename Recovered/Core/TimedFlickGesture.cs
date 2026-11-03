using System.Numerics;
namespace IQuarters.Core;

// Adapt variable-rate UIKit samples to the port's established 60 Hz input scale.
// The original strength/aim formula remains in FlickGesture.
public sealed class TimedFlickGesture
{
    public const double ReferenceInterval=1d/60;
    readonly FlickGesture gesture=new();
    Vector2 pending,total;
    double previous,windowTime;
    bool tracking,completed;
    public void Begin(double timestamp){Cancel();if(!double.IsFinite(timestamp))return;previous=timestamp;tracking=true;completed=false;gesture.Begin();}
    public void Cancel(){tracking=false;completed=false;pending=total=default;windowTime=0;gesture.Cancel();}
    public bool Sample(Vector2 delta,double timestamp,out float power,out float aim)
    {
        power=aim=0;
        if(!tracking||completed||!double.IsFinite(timestamp)||timestamp<=previous||!float.IsFinite(delta.X)||!float.IsFinite(delta.Y))return false;
        total+=delta;
        double duration=timestamp-previous;previous=timestamp;
        if(windowTime+duration<ReferenceInterval-1e-9){pending+=delta;windowTime+=duration;return false;}
        double first=Math.Max(0,ReferenceInterval-windowTime);
        pending+=delta*(float)(first/duration);
        bool fired=gesture.Sample(pending,out power,out aim);pending=default;windowTime=0;
        if(fired){completed=true;return true;}
        double remaining=Math.Max(0,duration-first);
        if(remaining>=ReferenceInterval){
            // Every full window inside this linear segment has the same displacement.
            // Evaluate once, then skip identical windows (including long stationary holds).
            if(gesture.Sample(delta*(float)(ReferenceInterval/duration),out power,out aim))return true;
            remaining%=ReferenceInterval;
        }
        pending=delta*(float)(remaining/duration);windowTime=remaining;
        return false;
    }
    public bool End(out float power,out float aim)
    {
        power=aim=0;
        // A fast swipe can end between window boundaries. Normalize its remaining time.
        bool fired=tracking&&windowTime>1e-9&&gesture.Sample(pending*(float)(ReferenceInterval/windowTime),out power,out aim);
        // Desktop mouse drags are often reported at a much lower event rate than touch.
        // If no 60 Hz sample crossed the threshold, evaluate the complete upward drag once.
        if(!fired&&tracking)fired=gesture.Sample(total,out power,out aim);
        Cancel();return fired;
    }
}
