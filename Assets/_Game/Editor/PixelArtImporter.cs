using UnityEditor;
using UnityEngine;

namespace OldAlley.EditorTools
{
    /// <summary>
    /// 强制 Assets/_Game/Art 下的所有贴图走像素规则：
    /// Point、无压缩、PPU 16、Single Sprite、FullRect、Clamp、无 mipmap。
    /// </summary>
    public class PixelArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var path = assetPath.Replace('\\', '/');
            if (!path.StartsWith("Assets/_Game/Art/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            // 等距整图按 40 PPU（26×26 格 → 1040×520 像素），其余保持一致 16 PPU。
            importer.spritePixelsPerUnit = path.EndsWith("/map_ground.png") ? 40f : 16f;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }
    }
}
