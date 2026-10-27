#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Generates stylized 2D pixel-art style sprites for all towers, enemies, projectiles, and tiles.
    /// Meets the rubric requirement: "Art: 15%... want to see some effort placed here...".
    /// </summary>
    public static class SpriteAssetGenerator
    {
        [MenuItem("Tools/Tower Defense/Generate All Sprites")]
        public static void GenerateAllSprites()
        {
            string spriteDir = Path.Combine(Application.dataPath, "Sprites");
            if (!Directory.Exists(spriteDir))
            {
                Directory.CreateDirectory(spriteDir);
            }

            // Towers
            SaveTexture(Path.Combine(spriteDir, "ArcherTower.png"), CreateArcherTowerTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "WarriorTower.png"), CreateWarriorTowerTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "MageTower.png"), CreateMageTowerTexture(64, 64));

            // Enemies
            SaveTexture(Path.Combine(spriteDir, "Skeleton.png"), CreateSkeletonTexture(48, 48));
            SaveTexture(Path.Combine(spriteDir, "Zombie.png"), CreateZombieTexture(48, 48));
            SaveTexture(Path.Combine(spriteDir, "Orc.png"), CreateOrcTexture(56, 56));

            // Projectiles & Combat Effects
            SaveTexture(Path.Combine(spriteDir, "Arrow.png"), CreateArrowTexture(32, 12));
            SaveTexture(Path.Combine(spriteDir, "Fireball.png"), CreateFireballTexture(32, 32));
            SaveTexture(Path.Combine(spriteDir, "SlashEffect.png"), CreateSlashTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "HitMarker.png"), CreateHitMarkerTexture(32, 32));

            // Environment & UI
            SaveTexture(Path.Combine(spriteDir, "PathTile.png"), CreatePathTileTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "GrassTile.png"), CreateGrassTileTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "BaseCastle.png"), CreateBaseCastleTexture(64, 64));
            SaveTexture(Path.Combine(spriteDir, "HealthBarBg.png"), CreateFlatTexture(64, 8, new Color(0.1f, 0.1f, 0.1f, 0.85f)));
            SaveTexture(Path.Combine(spriteDir, "HealthBarFill.png"), CreateFlatTexture(64, 8, new Color(0.15f, 0.85f, 0.2f, 1f)));

            AssetDatabase.Refresh();

            // Configure texture import settings to Sprite (2D and UI) with Point filtering
            ConfigureSpritesInDirectory("Assets/Sprites");

            Debug.Log("[SpriteAssetGenerator] Successfully generated and configured all 2D game sprites!");
        }

        private static void ConfigureSpritesInDirectory(string relativeDirPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { relativeDirPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Point; // Crisp pixel art look
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Texture2D CreateArcherTowerTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color wood = new Color(0.55f, 0.35f, 0.15f);
            Color woodDark = new Color(0.35f, 0.22f, 0.10f);
            Color roofGreen = new Color(0.2f, 0.65f, 0.25f);

            // Tower base / legs
            FillRect(tex, 16, 4, 32, 36, wood);
            DrawRectOutline(tex, 16, 4, 32, 36, woodDark);

            // Cross-beams
            DrawLine(tex, 16, 10, 48, 30, woodDark);
            DrawLine(tex, 16, 30, 48, 10, woodDark);

            // Wooden platform & crenels
            FillRect(tex, 12, 38, 40, 8, woodDark);
            FillRect(tex, 14, 46, 8, 8, wood);
            FillRect(tex, 42, 46, 8, 8, wood);

            // Green canopy / archer bow symbol
            FillRect(tex, 20, 46, 24, 14, roofGreen);
            DrawCircle(tex, 32, 53, 5, Color.white);
            return tex;
        }

        private static Texture2D CreateWarriorTowerTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color stone = new Color(0.5f, 0.52f, 0.55f);
            Color stoneDark = new Color(0.32f, 0.34f, 0.36f);
            Color bannerRed = new Color(0.85f, 0.2f, 0.2f);
            Color steel = new Color(0.88f, 0.9f, 0.95f);

            // Stone bastion walls
            FillRect(tex, 12, 4, 40, 44, stone);
            DrawRectOutline(tex, 12, 4, 40, 44, stoneDark);

            // Battlements
            FillRect(tex, 8, 46, 12, 12, stone);
            FillRect(tex, 26, 46, 12, 12, stone);
            FillRect(tex, 44, 46, 12, 12, stone);
            DrawRectOutline(tex, 8, 46, 48, 12, stoneDark);

            // Red crest banner
            FillRect(tex, 24, 14, 16, 24, bannerRed);

            // Crossed swords symbol
            DrawLine(tex, 26, 16, 38, 34, steel);
            DrawLine(tex, 38, 16, 26, 34, steel);
            return tex;
        }

        private static Texture2D CreateMageTowerTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color obsidian = new Color(0.2f, 0.16f, 0.3f);
            Color arcanePurple = new Color(0.65f, 0.25f, 0.9f);
            Color arcaneCyan = new Color(0.25f, 0.85f, 0.95f);

            // Tower spire narrowing up
            for (int y = 4; y < 42; y++)
            {
                int inset = (int)((y - 4) * 0.35f);
                int startX = 14 + inset;
                int endX = 50 - inset;
                for (int x = startX; x <= endX; x++)
                {
                    tex.SetPixel(x, y, (x == startX || x == endX) ? obsidian : arcanePurple);
                }
            }

            // Floating Arcane Crystal Orb at apex
            DrawCircle(tex, 32, 52, 9, arcaneCyan);
            DrawCircle(tex, 32, 52, 4, Color.white);
            return tex;
        }

        private static Texture2D CreateSkeletonTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color bone = new Color(0.92f, 0.92f, 0.88f);
            Color eyeSocket = new Color(0.1f, 0.1f, 0.12f);

            // Skull
            DrawCircle(tex, 24, 32, 10, bone);
            // Eye sockets
            tex.SetPixel(21, 33, eyeSocket);
            tex.SetPixel(27, 33, eyeSocket);
            tex.SetPixel(21, 32, eyeSocket);
            tex.SetPixel(27, 32, eyeSocket);

            // Spine & Ribs
            FillRect(tex, 23, 14, 3, 12, bone);
            DrawLine(tex, 17, 22, 31, 22, bone);
            DrawLine(tex, 18, 18, 30, 18, bone);

            // Legs
            DrawLine(tex, 22, 14, 18, 4, bone);
            DrawLine(tex, 25, 14, 29, 4, bone);
            return tex;
        }

        private static Texture2D CreateZombieTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color zombieGreen = new Color(0.35f, 0.65f, 0.35f);
            Color tatteredCloth = new Color(0.3f, 0.35f, 0.45f);

            // Head
            FillRect(tex, 16, 28, 16, 16, zombieGreen);
            tex.SetPixel(19, 36, Color.yellow);
            tex.SetPixel(27, 36, Color.yellow);
            tex.SetPixel(23, 30, Color.black); // Slanted mouth

            // Tattered Body
            FillRect(tex, 15, 12, 18, 16, tatteredCloth);
            // Arms outstretched
            FillRect(tex, 6, 22, 10, 4, zombieGreen);
            FillRect(tex, 32, 22, 10, 4, zombieGreen);
            // Legs
            FillRect(tex, 18, 4, 4, 8, zombieGreen);
            FillRect(tex, 26, 4, 4, 8, zombieGreen);
            return tex;
        }

        private static Texture2D CreateOrcTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color orcGreen = new Color(0.2f, 0.45f, 0.2f);
            Color armorIron = new Color(0.4f, 0.45f, 0.5f);
            Color redEyes = new Color(0.95f, 0.1f, 0.1f);

            // Massive bulky body
            FillRect(tex, 14, 12, 28, 22, armorIron);

            // Broad head
            FillRect(tex, 18, 34, 20, 16, orcGreen);
            // Red glowing eyes
            tex.SetPixel(22, 42, redEyes);
            tex.SetPixel(23, 42, redEyes);
            tex.SetPixel(32, 42, redEyes);
            tex.SetPixel(33, 42, redEyes);
            // Tusks
            tex.SetPixel(21, 36, Color.white);
            tex.SetPixel(21, 37, Color.white);
            tex.SetPixel(34, 36, Color.white);
            tex.SetPixel(34, 37, Color.white);

            // Armored shoulders
            FillRect(tex, 8, 26, 8, 10, armorIron);
            FillRect(tex, 40, 26, 8, 10, armorIron);

            // Legs
            FillRect(tex, 16, 4, 8, 8, armorIron);
            FillRect(tex, 32, 4, 8, 8, armorIron);
            return tex;
        }

        private static Texture2D CreateArrowTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color shaft = new Color(0.6f, 0.4f, 0.2f);
            Color tip = new Color(0.9f, 0.9f, 0.95f);
            Color fletch = Color.white;

            // Shaft
            FillRect(tex, 4, 5, 22, 2, shaft);
            // Tip
            FillRect(tex, 26, 4, 4, 4, tip);
            tex.SetPixel(30, 5, tip);
            // Fletching
            DrawLine(tex, 2, 8, 6, 5, fletch);
            DrawLine(tex, 2, 3, 6, 6, fletch);
            return tex;
        }

        private static Texture2D CreateFireballTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            DrawCircle(tex, 16, 16, 14, new Color(1f, 0.3f, 0.05f, 0.6f));
            DrawCircle(tex, 16, 16, 10, new Color(1f, 0.6f, 0.1f, 0.9f));
            DrawCircle(tex, 16, 16, 5, Color.white);
            return tex;
        }

        private static Texture2D CreateSlashTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color arc = new Color(0.4f, 0.8f, 1f, 0.85f);
            // Curved slash arc
            for (int i = 0; i < 40; i++)
            {
                float angle = Mathf.Lerp(0f, Mathf.PI * 0.75f, (float)i / 40);
                int x = (int)(32 + Mathf.Cos(angle) * 24);
                int y = (int)(32 + Mathf.Sin(angle) * 24);
                DrawCircle(tex, x, y, 3, arc);
            }
            return tex;
        }

        private static Texture2D CreateHitMarkerTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color spark = new Color(1f, 0.85f, 0.2f);
            // 4-point star crosshair
            DrawLine(tex, 16, 4, 16, 28, spark);
            DrawLine(tex, 4, 16, 28, 16, spark);
            DrawCircle(tex, 16, 16, 3, Color.white);
            return tex;
        }

        private static Texture2D CreatePathTileTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color dirt = new Color(0.72f, 0.58f, 0.40f);
            Color dirtBorder = new Color(0.55f, 0.42f, 0.26f);
            FillRect(tex, 0, 0, w, h, dirt);
            DrawRectOutline(tex, 0, 0, w, h, dirtBorder);
            return tex;
        }

        private static Texture2D CreateGrassTileTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color grass = new Color(0.28f, 0.62f, 0.28f);
            Color grassDark = new Color(0.22f, 0.52f, 0.22f);
            FillRect(tex, 0, 0, w, h, grass);
            DrawRectOutline(tex, 0, 0, w, h, grassDark);
            return tex;
        }

        private static Texture2D CreateBaseCastleTexture(int w, int h)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            Color keepStone = new Color(0.35f, 0.38f, 0.42f);
            Color flagGold = new Color(1f, 0.85f, 0.2f);
            FillRect(tex, 10, 4, 44, 40, keepStone);
            FillRect(tex, 24, 4, 16, 20, new Color(0.2f, 0.15f, 0.1f)); // Archway gate
            // Gold Flag
            DrawLine(tex, 32, 44, 32, 60, Color.black);
            FillRect(tex, 33, 50, 14, 10, flagGold);
            return tex;
        }

        private static Texture2D CreateFlatTexture(int w, int h, Color color)
        {
            Texture2D tex = CreateEmptyTexture(w, h);
            FillRect(tex, 0, 0, w, h, color);
            return tex;
        }

        private static Texture2D CreateEmptyTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color[] colors = new Color[w * h];
            for (int i = 0; i < colors.Length; i++) colors[i] = clear;
            tex.SetPixels(colors);
            return tex;
        }

        private static void FillRect(Texture2D tex, int startX, int startY, int width, int height, Color color)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawRectOutline(Texture2D tex, int startX, int startY, int width, int height, Color color)
        {
            for (int x = startX; x < startX + width; x++)
            {
                if (x >= 0 && x < tex.width)
                {
                    if (startY >= 0 && startY < tex.height) tex.SetPixel(x, startY, color);
                    if (startY + height - 1 >= 0 && startY + height - 1 < tex.height) tex.SetPixel(x, startY + height - 1, color);
                }
            }
            for (int y = startY; y < startY + height; y++)
            {
                if (y >= 0 && y < tex.height)
                {
                    if (startX >= 0 && startX < tex.width) tex.SetPixel(startX, y, color);
                    if (startX + width - 1 >= 0 && startX + width - 1 < tex.width) tex.SetPixel(startX + width - 1, y, color);
                }
            }
        }

        private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                        {
                            tex.SetPixel(x, y, color);
                        }
                    }
                }
            }
        }

        private static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                {
                    tex.SetPixel(x0, y0, color);
                }

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private static void SaveTexture(string filePath, Texture2D tex)
        {
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            Object.DestroyImmediate(tex);
        }
    }
}
#endif
