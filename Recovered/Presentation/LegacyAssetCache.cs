using System.Text.Json;
using SceneKit;
namespace IQuarters.iOS;

// One loading operation owns this cache; its two scenes share immutable source assets.
// Mutable node geometry/materials are copied by LegacyScene before use.
public sealed class LegacyAssetCache
{
    internal readonly Dictionary<string,SCNGeometry> Meshes=[];
    readonly Dictionary<string,JsonElement> documents=[];
    readonly Dictionary<string,UIImage> textures=[];
    internal JsonElement Read(string file)
    {
        if(!documents.TryGetValue(file,out var value)){
            using var doc=JsonDocument.Parse(File.ReadAllBytes(LegacyScene.Resource(file)));
            documents[file]=value=doc.RootElement.Clone();
        }
        return value;
    }
    internal UIImage Texture(string path)
    {
        if(!textures.TryGetValue(path,out var image))textures[path]=image=UIImage.FromFile(path)!;
        return image;
    }
}
