"""Create the static GitHub Pages site from the .NET browser build and recovered assets."""
from pathlib import Path
import json, shutil, argparse
root=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser();parser.add_argument('--output',default='dist/web');args=parser.parse_args()
output=root/args.output;output.mkdir(parents=True,exist_ok=True)
project=root/'Recovered/Web'
bundles=list((project/'bin/Release').glob('*/publish/wwwroot'))
if not bundles: raise SystemExit('Publish Recovered/Web in Release first (publish/wwwroot not found).')
bundle=max(bundles,key=lambda p:p.stat().st_mtime)
if (output/'_framework').exists(): shutil.rmtree(output/'_framework')
shutil.copytree(bundle,output,dirs_exist_ok=True)
shutil.copytree(project/'wwwroot',output,dirs_exist_ok=True)
assets=root/'Recovery/converted'
data={str(p.relative_to(assets)):json.loads(p.read_text()) for p in sorted(assets.rglob('*.json'))}
data['_textures']=[p.name for p in sorted((assets/'textures').glob('*.png'))]
(output/'assets.json').write_text(json.dumps(data,separators=(',',':')))
for directory in ['textures','audio']:shutil.copytree(assets/directory,output/'assets'/directory,dirs_exist_ok=True)
(output/'.nojekyll').touch()
print(f'Static site: {output}')
