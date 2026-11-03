using System.Numerics;
using CoreGraphics;
namespace SceneKit;
public struct SCNVector3(float x,float y,float z){public float X=x,Y=y,Z=z;public static implicit operator Vector3(SCNVector3 v)=>new(v.X,v.Y,v.Z);public static implicit operator SCNVector3(Vector3 v)=>new(v.X,v.Y,v.Z);public static SCNVector3 TransformPosition(SCNVector3 p,SCNMatrix4 m)=>Vector3.Transform(p,m.Value);}
public struct SCNQuaternion(float x,float y,float z,float w){public float X=x,Y=y,Z=z,W=w;public static implicit operator Quaternion(SCNQuaternion q)=>new(q.X,q.Y,q.Z,q.W);public static implicit operator SCNQuaternion(Quaternion q)=>new(q.X,q.Y,q.Z,q.W);}
public struct SCNMatrix4(Matrix4x4 value){public Matrix4x4 Value=value;public static SCNMatrix4 Identity=>new(Matrix4x4.Identity);public static SCNMatrix4 CreateTranslation(float x,float y,float z)=>new(Matrix4x4.CreateTranslation(x,y,z));}
public sealed class SCNScene {public SCNNode RootNode {get;}=new();}
public sealed class SCNNode {
 public static readonly Dictionary<int,SCNNode> All=[];static int serial;public readonly int WebId=++serial;
 public int SourceId;public string? Name;public bool Hidden;public float Opacity=1;public nuint CategoryBitMask;public SCNGeometry? Geometry;public SCNCamera? Camera;
 public SCNVector3 Position,Scale=new(1,1,1);public SCNQuaternion Orientation=new(0,0,0,1);
 public SCNNode? ParentNode;readonly List<SCNNode> children=[];public SCNNode[] ChildNodes=>children.ToArray();
 public SCNNode(){All[WebId]=this;}
 public SCNVector3 EulerAngles {get {Quaternion q=Orientation;return new(MathF.Asin(Math.Clamp(2*(q.W*q.X-q.Y*q.Z),-1,1)),MathF.Atan2(2*(q.W*q.Y+q.X*q.Z),1-2*(q.X*q.X+q.Y*q.Y)),MathF.Atan2(2*(q.W*q.Z+q.X*q.Y),1-2*(q.X*q.X+q.Z*q.Z)));}set=>Orientation=Quaternion.CreateFromYawPitchRoll(value.Y,value.X,value.Z);}
 public Matrix4x4 Matrix=>Matrix4x4.CreateScale(Scale)*Matrix4x4.CreateFromQuaternion(Orientation)*Matrix4x4.CreateTranslation(Position);
 public Matrix4x4 WorldMatrix=>ParentNode==null?Matrix:Matrix*ParentNode.WorldMatrix;
 public SCNVector3 WorldPosition=>Vector3.Transform(Vector3.Zero,WorldMatrix);
 public void AddChildNode(SCNNode n){n.RemoveFromParentNode();children.Add(n);n.ParentNode=this;}
 public void RemoveFromParentNode(){ParentNode?.children.Remove(this);ParentNode=null;}
 public SCNNode? FindChildNode(string name,bool recursively)=>children.FirstOrDefault(n=>n.Name==name)??(recursively?children.Select(n=>n.FindChildNode(name,true)).FirstOrDefault(n=>n!=null):null);
 public SCNVector3 ConvertPositionFromNode(SCNVector3 p,SCNNode from){Matrix4x4.Invert(WorldMatrix,out var inv);return Vector3.Transform(p,from.WorldMatrix*inv);}
 public SCNMatrix4 ConvertTransformToNode(SCNMatrix4 m,SCNNode to){Matrix4x4.Invert(to.WorldMatrix,out var inv);return new(m.Value*WorldMatrix*inv);}
 public void Look(SCNVector3 target,SCNVector3 up,SCNVector3 forward)=>Orientation=IQuarters.Core.CameraFollow.LevelLook((Vector3)target-(Vector3)WorldPosition,Orientation);
}
public sealed class SCNCamera {public double FieldOfView=55,ZNear=.05,ZFar=1100,OrthographicScale=100;public bool UsesOrthographicProjection;}
public sealed class SCNGeometry {public string MeshKey="";public SCNMaterial[] Materials=[];public SCNMaterial? FirstMaterial{get=>Materials.FirstOrDefault();set{if(value==null)return;if(Materials.Length==0)Materials=[value];else Materials[0]=value;}}public object Copy()=>new SCNGeometry{MeshKey=MeshKey,Materials=(SCNMaterial[])Materials.Clone()};}
public sealed class SCNMaterialProperty {public object? Contents;public SCNMatrix4 ContentsTransform=SCNMatrix4.Identity;public SCNMaterialProperty Copy()=>new(){Contents=Contents,ContentsTransform=ContentsTransform};}
public enum SCNLightingModel {Constant,Phong,Lambert}public enum SCNTransparencyMode {AOne}
public sealed class SCNMaterial {
 public string Key="";public bool DoubleSided,WritesToDepthBuffer=true;public double Transparency=1;public SCNTransparencyMode TransparencyMode;public SCNLightingModel LightingModelName;
 public SCNMaterialProperty Diffuse=new();
 public object Copy()=>new SCNMaterial{Key=Key,DoubleSided=DoubleSided,WritesToDepthBuffer=WritesToDepthBuffer,Transparency=Transparency,LightingModelName=LightingModelName,Diffuse=Diffuse.Copy()};
}
public sealed class SCNHitTestOptions {public bool IgnoreHiddenNodes;}
public sealed class SCNHitTestResult(SCNNode node){public SCNNode Node=node;}
public class SCNView : UIView {
 public SCNScene Scene=null!;public SCNNode? PointOfView;public bool AutoenablesDefaultLighting,Opaque;public double ContentScaleFactor;
 public SCNView(CGRect frame):base(frame){}
 public Task<bool> PrepareAsync(SCNScene[] scenes)=>Task.FromResult(true);
 public SCNHitTestResult[] HitTest(CGPoint point,SCNHitTestOptions options){var result=WebBridge.Hit(Scene.RootNode.WebId,PointOfView!.WebId,point.X,point.Y);return result.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Where(SCNNode.All.ContainsKey).Select(id=>new SCNHitTestResult(SCNNode.All[id])).ToArray();}
 public SCNVector3 ProjectPoint(SCNVector3 p){var a=WebBridge.Project(PointOfView!.WebId,p.X,p.Y,p.Z);return new((float)a[0],(float)a[1],(float)a[2]);}
}
