using IQuarters.Core;
namespace IQuarters.Verification;
public static class VerificationSuite {
public static int Run(string[] args,bool exhaustive=true) {
int checks=0;
void Check(bool ok, string name) { if (!ok) throw new Exception(name); checks++;Console.WriteLine("PASS " + name); }
var coinBody=CoinBody.Read(File.ReadAllText(Path.Combine(args.Length>0?args[0]:"Recovery/converted","physics.json")));
var single = new GameSession();
Check(single.GetCurrentShotsLeft() == 40, "Initial reserve is 40");
single.UpdateShotCount(0);
Check(single.GetCurrentShotsLeft() == 39 && single.curRound == 0, "Miss consumes one reserve coin");
for (int i = 0; i < 3; i++) single.UpdateShotCount(1);
Check(single.curRound == 1 && single.GetCurrentShotsLeft() == 39, "Three successes advance round without consuming reserve");
var multi = new GameSession(2);
for (int i = 0; i < 3; i++) multi.UpdateShotCount(1);
Check(multi.curPlayer == 1 && multi.curRound == 0, "Second player gets same round");
for (int i = 0; i < 3; i++) multi.UpdateShotCount(1);
Check(multi.curPlayer == 0 && multi.curRound == 1, "Round advances after both players");
var secret = new GameSession(); secret.curRound = 8; secret.curMadeShotsThisRound = 2;
foreach (var key in new[] {1,2,3,1,2,0}) secret.StoreSecretRoundUnlockCode(key);
Check(secret.secretRoundUnlocked, "Secret sequence unlocks at required state");
secret.UpdateShotCount(1); Check(secret.curRound == 12, "Secret round inserted after round index 8");
for (int i = 0; i < 3; i++) secret.UpdateShotCount(1);
Check(secret.curRound == 9, "Secret round returns to round index 9");
var exhausted = new GameSession(); exhausted.SetCurrentShotsLeft(1);
int flags = exhausted.UpdateShotCount(0);
Check((flags & exhausted.flagGameOver) != 0, "Last reserve ends single-player game");
var finished = new GameSession(); finished.curRound = 11;
for (int i = 0; i < 2; i++) finished.UpdateShotCount(1);
flags = finished.UpdateShotCount(1);
Check((flags & finished.flagGameOver) != 0, "Twelfth round completes game");
Check(single.curRound == 1, "Sessions do not share state");

var v = ShotSimulation.ClosestPoint(new System.Numerics.Vector3(.2f,1,.2f), System.Numerics.Vector3.Zero, System.Numerics.Vector3.UnitX, System.Numerics.Vector3.UnitZ);
Check(System.Numerics.Vector3.Distance(v,new(.2f,0,.2f))<.0001f,"Triangle projection stays on surface");
var free = new ShotSimulation(new(0,2.47596f,-4.31044f),.8f,50,0,[]);
for(int i=0;i<1000 && !free.Finished;i++)free.Advance(1f/60);
Check(free.Finished && free.Multiplier==0 && float.IsFinite(free.Position.Y),"Uncaught coin times out as a miss");
var flick = new FlickGesture();
flick.Begin();Check(!flick.Sample(new(1,1),out _,out _),"Small touch jitter does not fire");
Check(!flick.Sample(new(100,0),out _,out _) && !flick.Sample(new(0,-100),out _,out _),"Sideways and downward swipes cannot fire");
Check(flick.Sample(new(12,40),out var flickPower,out var flickAim) && Math.Abs(flickAim-1.2f)<.00001f,"Native flick aim is four times horizontal/upward movement");
Check(Math.Abs(flickPower-MathF.Pow(40*.02f/1.8f,.1f))<.00001f,"Native flick uses vertical scale, divisor and exponent from the IPA");
Check(!flick.Sample(new(0,80),out _,out _),"One contact cannot fire twice");
flick.Cancel();Check(!flick.Sample(new(0,80),out _,out _),"Cancelled touch cannot shoot");
flick.Begin();Check(flick.Sample(new(0,360),out var stronger,out _) && Math.Abs(stronger-MathF.Pow(4,.1f))<.00001f,"Strong flick retains original narrow power range");
var diagonal=new FlickGesture();diagonal.Begin();diagonal.Sample(new(80,40),out var diagonalPower,out var diagonalAim);
Check(Math.Abs(diagonalPower-flickPower)<.00001f&&Math.Abs(diagonalAim-8)<.00001f,"Horizontal movement changes aim without strengthening the flick or an invented aim clamp");
var timing = new ShotSimulation(new(0,20,0),1,50,0,[]);
timing.Advance(.076f);Check(timing.Velocity.Y< -16,"Coin starts in the downward phase");
timing.Advance(.0041f);Check(timing.Velocity.Y>0 && Math.Abs(timing.Elapsed-.08f)<.0001f,"Upward launch occurs after four recovered fixed ticks");
float oldVertical=timing.Velocity.Y;timing.Advance(.0201f);
Check(Math.Abs(timing.Velocity.Y-oldVertical+19.64f*.02f)<.0002f,"Airborne gravity matches the IPA PhysicsManager");
Check(coinBody.Pieces.Length==2 && coinBody.Radius>.3f && coinBody.Radius<.4f,"Coin uses the IPA box plus cylinder compound instead of a small sphere");
var spin=new ShotSimulation(new(0,20,0),1,50,0,[],body:coinBody);spin.Advance(.1f);
Check(spin.AngularVelocity.Length()>0 && spin.AngularVelocity.Length()<=7.001f && Math.Abs(spin.Orientation.Length()-1)<.00001f,"Launch torque produces bounded, normalized coin rotation");
var chain=new CollisionChain();chain.Add(1,10,0);chain.Add(2,20,.1f);chain.Add(3,10,.2f);
Check(chain.Ricochets==2,"Returning to a previously hit body counts a second ricochet");
var duplicate=new CollisionChain();duplicate.Add(1,10,0);duplicate.Add(2,10,.1f);duplicate.Add(3,20,.101f);
Check(duplicate.Ricochets==1,"Colliders on one body merge; final body transition remains counted");
Check(!ShakeGesture.Sample(new(0,-1,0),out _,out _),"Stationary upright gravity cannot fire a shake shot");
Check(ShakeGesture.Sample(new(3,-1,-3),out var shakePower,out var shakeAim)&&Math.Abs(shakeAim-2.5f)<.00001f&&Math.Abs(shakePower-MathF.Pow(2.95f/1.975f,.9f))<.00001f,"Shake strength and aim follow the native acceleration projection");
CollisionShape Floor(float bounce,System.Numerics.Vector3? launch=null)=>new(){Owner=10,ColliderId=1,ReactionOwner=20,LaunchVelocity=launch,Material=new(){Bounce=bounce,DynamicFriction=0,StaticFriction=0,BounceCombine=3},Triangles=[ [new(-100,0,-100),new(100,0,-100),new(0,0,100)] ]};
ShotSimulation Drop(CollisionShape floor){floor.Prepare();var sim=new ShotSimulation(new(0,1,0),1,50,0,[floor],body:coinBody);sim.Advance(.0601f);return sim;}
var ordinary=Drop(Floor(0));var elastic=Drop(Floor(1));
Check(ordinary.ContactCount>0&&elastic.Velocity.Y>ordinary.Velocity.Y,"Recovered restitution changes the measured rebound velocity");
var launcher=Floor(0,new(0,25,0));launcher.Prepare();int reacted=0;var launchedByProp=new ShotSimulation(new(0,1,0),1,50,0,[launcher],body:coinBody);launchedByProp.ReactionTriggered+=_=>reacted++;launchedByProp.Advance(.0601f);
Check(reacted==1&&launchedByProp.Velocity.Y>20,"A top contact invokes the prop reaction and applies its launch velocity once");
if(args.Length>0&&exhaustive) {
    using var sceneDoc=System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"scene.json")));
    using var collisionDoc=System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(args[0],"collision.json")));
    var scene=sceneDoc.RootElement;
    var enabled=scene.GetProperty("rounds")[0].EnumerateArray().Select(v=>v.GetInt32()).ToHashSet();
    foreach(var n in scene.GetProperty("nodes").EnumerateArray()) if(n.GetProperty("name").GetString() is "TableTop" or "table_00")enabled.Add(n.GetProperty("id").GetInt32());
    List<CollisionShape> ShapesFor(HashSet<int> enabled){var shapes=new List<CollisionShape>();
    foreach(var c in collisionDoc.RootElement.EnumerateArray()) {
        if(!c.GetProperty("ancestors").EnumerateArray().Any(v=>enabled.Contains(v.GetInt32())))continue;
        var shape=new CollisionShape {Owner=c.GetProperty("body").GetInt32(),ColliderId=c.GetProperty("collider").GetInt32(),Material=SurfaceMaterial.Read(c.GetProperty("material")),Multiplier=c.GetProperty("multiplier").GetInt32(),Triangles=c.GetProperty("triangles").EnumerateArray().Select(t=>t.EnumerateArray().Select(v=>new System.Numerics.Vector3(v[0].GetSingle(),v[1].GetSingle(),v[2].GetSingle())).ToArray()).ToArray()};shape.Prepare();shapes.Add(shape);
    }
    return shapes;}
    var shapes=ShapesFor(enabled);
    int successes=0;float firstPower=0, firstAim=0;
    for(float power=.4f;power<1.41f;power+=.025f) for(float aim=-1.5f;aim<=1.5f;aim+=.3f) {
        var simulation=new ShotSimulation(new(0,2.4759626f,-4.3104444f),power,50,aim,shapes,body:coinBody);
        for(int i=0;i<300 && !simulation.Finished;i++)simulation.Advance(1f/60);
        CheckFinite(simulation.Position);
        if(simulation.Multiplier>0){successes++;if(successes==1){firstPower=power;firstAim=aim;}}
    }
    Check(successes>0,$"Recovered first-round colliders allow scoring ({successes} shots; example power {firstPower:F3}, aim {firstAim:F2})");
    int reachable=0;
    for(float y=32;y<=160;y+=8)for(float x=-12;x<=12;x+=1){
        var gesture=new FlickGesture();gesture.Begin();if(!gesture.Sample(new(x,y),out var p,out var aim))continue;
        var sim=new ShotSimulation(new(0,2.4759626f,-4.3104444f),p,50,aim,shapes,body:coinBody);
        for(int i=0;i<300&&!sim.Finished;i++)sim.Advance(1f/60);
        if(sim.Multiplier>0)reachable++;
    }
    Check(reachable>0,$"Actual flick samples can score in the recovered first round ({reachable} samples)");
    foreach(var (round,index) in scene.GetProperty("rounds").EnumerateArray().Select((r,i)=>(r,i))){
        var active=round.EnumerateArray().Select(v=>v.GetInt32()).ToHashSet();
        foreach(var n in scene.GetProperty("nodes").EnumerateArray())if(n.GetProperty("name").GetString() is "TableTop" or "table_00")active.Add(n.GetProperty("id").GetInt32());
        var roundShapes=ShapesFor(active);
        foreach(float aim in new[]{-1f,0,1f}){var sim=new ShotSimulation(new(0,2.4759626f,-4.3104444f),1,50,aim,roundShapes,body:coinBody);for(int frame=0;frame<400&&!sim.Finished;frame++)sim.Advance(1f/60);CheckFinite(sim.Position);if(!sim.Finished)throw new Exception("Round shot did not settle");}
        Check(roundShapes.Count>0,$"Round {index+1}: three static-scene trajectories remain finite and terminate");
    }


}
// Device adaptation must preserve apparent horizontal framing and fractional flicks.
foreach(var viewport in new[]{(320d,480d),(320d,568d),(375d,812d),(440d,956d),(810d,1080d)}){
    var (width,height)=viewport;
    double half=PresentationRules.HalfHeight(width,height);
    Check(half*width/height>=200d/3-.00001,$"HUD fits width at {width}x{height}");
    var g=new FlickGesture();g.Begin();float touchScale=PresentationRules.TouchScale(width,height);g.Sample(new(12,40),out var power,out var aim);
    Check(Math.Abs(power-flickPower)<.00001f&&Math.Abs(aim-1.2f)<.00001f,$"Relative flick strength/aim preserved at {width}x{height}");
    if(width>=800){var mouse=new FlickGesture();mouse.Begin();Check(mouse.Sample(new(0,40*touchScale*PresentationRules.DesktopMouseSensitivity),out _,out _),$"Comfortable desktop mouse flick launches at {width}x{height}");var touch=new FlickGesture();touch.Begin();Check(!touch.Sample(new(0,40*touchScale),out _,out _),$"Touchscreen sensitivity remains unchanged at {width}x{height}");}
}
Check(Math.Abs(PresentationRules.VerticalFieldOfView(320,480)-55)<.00001,"Original phone framing retained at 2:3");
Check(Math.Abs(PresentationRules.VerticalFieldOfView(810,1080,70)-70)<.00001,"Original iPad 70-degree camera retained");
int[][] expectedCameras=[[1,2,3],[1,2,3],[1,2,3],[3,3,3],[1,2,3],[1,2,3],[3,3,3],[1,2,3],[2,2,3],[1,2,3],[1,2,1],[1,2,3],[1,2,1]];
Check(expectedCameras.SelectMany((r,i)=>r.Select((c,j)=>PresentationRules.ReplayCamera(i,j+1)==c)).All(x=>x),"All 39 replay selections match native round exceptions");
Check(!PresentationRules.RoundStinger(false,0,1)&&!PresentationRules.RoundStinger(false,1,4)&&PresentationRules.RoundStinger(false,2,1)&&!PresentationRules.RoundStinger(true,2,1)&&!PresentationRules.RoundStinger(false,2,0),"Round stinger only on Classic final successful shot");
// Reproduce a rolled camera followed by extreme throws, then reset to the next coin.
var cameraRotation=System.Numerics.Quaternion.CreateFromYawPitchRoll(1.1f,-.5f,.7f);
bool level=true,finite=true;
foreach(var direction in new[]{new System.Numerics.Vector3(10,4,-1),new(-10,2,4),new(0,100,0),new(0,-100,0),new(0,-1.33f,-1.81f)}){
    for(int frame=0;frame<120;frame++){
        cameraRotation=CameraFollow.Follow(cameraRotation,direction,.05f);
        var right=System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX,cameraRotation);
        level &= Math.Abs(right.Y)<.0001f;
        finite &= float.IsFinite(cameraRotation.X)&&float.IsFinite(cameraRotation.Y)&&float.IsFinite(cameraRotation.Z)&&float.IsFinite(cameraRotation.W);
    }
}
Check(level&&finite,"Camera stays level through lateral/vertical throws and a previously rolled orientation");
var resetDirection=new System.Numerics.Vector3(0,-1.33f,-1.81f);
var resetRotation=CameraFollow.LevelLook(resetDirection,cameraRotation);
Check(System.Numerics.Vector3.Distance(System.Numerics.Vector3.Transform(-System.Numerics.Vector3.UnitZ,resetRotation),System.Numerics.Vector3.Normalize(resetDirection))<.0001f,"Next-coin camera reset faces the spawn immediately with world up");
Check(CameraFollow.LevelLook(System.Numerics.Vector3.Zero,resetRotation)==resetRotation,"Coin at camera position retains a finite orientation");
float CameraBlendAt(int hz){float value=0;for(int i=0;i<hz/10;i++)value+=(1-value)*CameraFollow.Blend(.2f,1f/hz);return value;}
Check(Math.Abs(CameraBlendAt(30)-CameraBlendAt(60))<.00001f&&Math.Abs(CameraBlendAt(120)-CameraBlendAt(60))<.00001f,"Camera smoothing has equal response at 30, 60 and 120 Hz");
ShotSimulation FrameRateShot(int hz){var floor=Floor(.65f);floor.Prepare();var sim=new ShotSimulation(new(0,2.4759626f,-4.3104444f),1,50,.3f,[floor],body:coinBody);for(int i=0;i<hz*10&&!sim.Finished;i++)sim.Advance(1f/hz);return sim;}
var shot60=FrameRateShot(60);var shot120=FrameRateShot(120);
Check(shot60.Finished&&shot120.Finished&&shot60.Elapsed==shot120.Elapsed&&shot60.Position==shot120.Position&&shot60.Orientation==shot120.Orientation&&shot60.ContactCount==shot120.ContactCount,"60/120 Hz updates produce the identical settled physical shot and contacts");
// Same physical stroke delivered at different touch sampling rates must produce one identical shot.
(float Power,float Aim,int Fired) TimedSwipe(int hz,float upward=40){var g=new TimedFlickGesture();g.Begin(10);float power=0,aim=0;int fired=0;for(int i=1;i<=hz/5;i++){if(g.Sample(new(12*5f/hz,upward*5f/hz),10+(double)i/hz,out var p,out var a)){power=p;aim=a;fired++;}}if(g.End(out var ep,out var ea)){power=ep;aim=ea;fired++;}return(power,aim,fired);}
foreach(int rate in new[]{30,60,90,120,240}){var swipe=TimedSwipe(rate);Check(swipe.Fired==1&&Math.Abs(swipe.Power-flickPower)<.00001f&&Math.Abs(swipe.Aim-1.2f)<.00001f,$"Same physical swipe has identical strength/aim at {rate} Hz touch sampling");}
Check(new[]{30,60,120,240}.All(rate=>TimedSwipe(rate,10).Fired==0),"Slow drags remain below the flick threshold at every sampling rate");
var shortFlick=new TimedFlickGesture();shortFlick.Begin(0);Check(!shortFlick.Sample(new(6,20),1d/120,out _,out _)&&shortFlick.End(out var shortPower,out var shortAim)&&Math.Abs(shortPower-flickPower)<.00001f&&Math.Abs(shortAim-1.2f)<.00001f,"Finger-up evaluates a short final window using its duration");
var cancelledFlick=new TimedFlickGesture();cancelledFlick.Begin(0);cancelledFlick.Sample(new(6,20),1d/120,out _,out _);cancelledFlick.Cancel();Check(!cancelledFlick.End(out _,out _)&&!cancelledFlick.Sample(new(12,40),1d/60,out _,out _),"Touch cancellation discards pending movement without launching");
var irregularFlick=new TimedFlickGesture();irregularFlick.Begin(0);double lastTime=0;int irregularShots=0;float irregularPower=0,irregularAim=0;
foreach(double time in new[]{.003,.007,.012,.022,.027,.044,.081}){float elapsed=(float)(time-lastTime);if(irregularFlick.Sample(new(720*elapsed,2400*elapsed),time,out var p,out var a)){irregularShots++;irregularPower=p;irregularAim=a;}lastTime=time;}
Check(irregularShots==1&&Math.Abs(irregularPower-flickPower)<.00001f&&Math.Abs(irregularAim-1.2f)<.00001f,"Irregular/batched touch intervals preserve shot power and aim");
var duplicateFlick=new TimedFlickGesture();duplicateFlick.Begin(5);Check(!duplicateFlick.Sample(new(0,500),5,out _,out _)&&!duplicateFlick.Sample(new(0,500),4,out _,out _)&&duplicateFlick.Sample(new(12,40),5+1d/60,out var dupPower,out _)&&Math.Abs(dupPower-flickPower)<.00001f,"Duplicate/out-of-order timestamps cannot create a stronger flick");
static void CheckFinite(System.Numerics.Vector3 p) {if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||!float.IsFinite(p.Z))throw new Exception("Nonfinite physics position");}

return checks;
}
}
