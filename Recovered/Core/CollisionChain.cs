namespace IQuarters.Core;
// QuarterTrigger.AddColliderToList / BuildCollisionChain: collider transitions,
// then rigid-body transitions with a 10 ms filter and an explicit final hit.
public sealed class CollisionChain
{
    readonly List<(int Collider,int Body,float Time)> hits=[];
    int lastCollider=int.MinValue;
    public void Add(int collider,int body,float time){if(collider==lastCollider)return;lastCollider=collider;if(hits.Count<15)hits.Add((collider,body,time));}
    public int Ricochets {
        get {int count=0,previous=0;float previousTime=0;
            for(int i=0;i<hits.Count;i++){var h=hits[i];if(h.Body==0)continue;if(i!=0&&(previous==h.Body||(i!=hits.Count-1&&h.Time-previousTime<=.01f)))continue;count++;previous=h.Body;previousTime=h.Time;if(count==10)break;}
            return Math.Max(0,count-1);}
    }
}
