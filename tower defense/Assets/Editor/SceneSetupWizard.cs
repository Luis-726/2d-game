#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using TowerDefense.Audio;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using TowerDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Automated setup wizard that constructs the complete game scene, generates prefabs,
    /// links all audio and visual assets, and sets up UI according to the design document.
    /// </summary>
    public static class SceneSetupWizard
    {
        [MenuItem("Tools/Tower Defense/Build & Setup Entire Game Scene")]
        public static void BuildEntireGame()
        {
            // 1. Ensure assets are generated first
            SpriteAssetGenerator.GenerateAllSprites();
            SoundGenerator.GenerateAllSounds();

            string prefabDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabDir))
            {
                Directory.CreateDirectory(prefabDir);
            }

            // 2. Build Prefabs
            GameObject hitMarkerPrefab = CreateHitMarkerPrefab(prefabDir);
            GameObject slashEffectPrefab = CreateSlashEffectPrefab(prefabDir);
            GameObject arrowPrefab = CreateArrowPrefab(prefabDir);
            GameObject fireballPrefab = CreateFireballPrefab(prefabDir);

            GameObject skeletonPrefab = CreateEnemyPrefab(prefabDir, EnemyType.Skeleton, "Skeleton", 6, 3.2f, 3, 1, "Skeleton.png");
            GameObject zombiePrefab = CreateEnemyPrefab(prefabDir, EnemyType.Zombie, "Zombie", 14, 2.0f, 6, 2, "Zombie.png");
            GameObject orcPrefab = CreateEnemyPrefab(prefabDir, EnemyType.Orc, "Orc", 30, 1.2f, 12, 3, "Orc.png");

            GameObject archerPrefab = CreateArcherTowerPrefab(prefabDir, arrowPrefab);
            GameObject warriorPrefab = CreateWarriorTowerPrefab(prefabDir, slashEffectPrefab);
            GameObject magePrefab = CreateMageTowerPrefab(prefabDir, fireballPrefab);

            AssetDatabase.Refresh();

            // 3. Setup Scene
            SetupSceneHierarchy(
                archerPrefab.GetComponent<Tower>(),
                warriorPrefab.GetComponent<Tower>(),
                magePrefab.GetComponent<Tower>(),
                skeletonPrefab.GetComponent<Enemy>(),
                zombiePrefab.GetComponent<Enemy>(),
                orcPrefab.GetComponent<Enemy>(),
                hitMarkerPrefab
            );

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();

            Debug.Log("[SceneSetupWizard] Complete Tower Defense game scene constructed and saved to disk!");
        }

        private static GameObject CreateHitMarkerPrefab(string dir)
        {
            GameObject obj = new GameObject("HitMarker");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HitMarker.png");
            sr.sortingOrder = 20;

            string path = $"{dir}/HitMarker.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateSlashEffectPrefab(string dir)
        {
            GameObject obj = new GameObject("SlashEffect");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SlashEffect.png");
            sr.sortingOrder = 15;

            string path = $"{dir}/SlashEffect.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateArrowPrefab(string dir)
        {
            GameObject obj = new GameObject("Arrow");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Arrow.png");
            sr.sortingOrder = 10;

            Projectile proj = obj.AddComponent<Projectile>();
            SetPrivateField(proj, "speed", 14f);
            SetPrivateField(proj, "damage", 3);
            SetPrivateField(proj, "isAreaOfEffect", false);

            string path = $"{dir}/Arrow.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateFireballPrefab(string dir)
        {
            GameObject obj = new GameObject("Fireball");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fireball.png");
            sr.sortingOrder = 10;

            Projectile proj = obj.AddComponent<Projectile>();
            SetPrivateField(proj, "speed", 8f);
            SetPrivateField(proj, "damage", 5);
            SetPrivateField(proj, "isAreaOfEffect", true);
            SetPrivateField(proj, "splashRadius", 1.8f);

            string path = $"{dir}/Fireball.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(string dir, EnemyType type, string name, int hp, float speed, int coins, int baseDamage, string spriteName)
        {
            GameObject obj = new GameObject(name);
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.isTrigger = true;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{spriteName}");
            sr.sortingOrder = 5;

            Enemy enemy = obj.AddComponent<Enemy>();
            SetPrivateField(enemy, "enemyType", type);
            SetPrivateField(enemy, "maxHealth", hp);
            SetPrivateField(enemy, "moveSpeed", speed);
            SetPrivateField(enemy, "coinReward", coins);
            SetPrivateField(enemy, "baseDamage", baseDamage);
            SetPrivateField(enemy, "spriteRenderer", sr);

            // Add HealthBar
            GameObject hbObj = new GameObject("HealthBar");
            hbObj.transform.SetParent(obj.transform);
            hbObj.transform.localPosition = new Vector3(0, 0.65f, 0);

            GameObject bgObj = new GameObject("Bg");
            bgObj.transform.SetParent(hbObj.transform);
            bgObj.transform.localPosition = Vector3.zero;
            SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HealthBarBg.png");
            bgSr.sortingOrder = 18;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(hbObj.transform);
            fillObj.transform.localPosition = Vector3.zero;
            SpriteRenderer fillSr = fillObj.AddComponent<SpriteRenderer>();
            fillSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HealthBarFill.png");
            fillSr.sortingOrder = 19;

            HealthBar2D hb = hbObj.AddComponent<HealthBar2D>();
            SetPrivateField(hb, "enemy", enemy);
            SetPrivateField(hb, "fillBar", fillObj.transform);

            string path = $"{dir}/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateArcherTowerPrefab(string dir, GameObject arrowPrefab)
        {
            GameObject obj = new GameObject("ArcherTower");
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ArcherTower.png");
            sr.sortingOrder = 6;

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.sortingOrder = 15;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.3f, 1f, 0.4f, 0.5f);
            lr.endColor = new Color(0.3f, 1f, 0.4f, 0.5f);

            Tower tower = obj.AddComponent<Tower>();
            SetPrivateField(tower, "towerType", TowerType.Archer);
            SetPrivateField(tower, "towerName", "Archer Tower");
            SetPrivateField(tower, "buildCost", 15);
            SetPrivateField(tower, "range", 4.0f);
            SetPrivateField(tower, "attacksPerSecond", 1.2f);
            SetPrivateField(tower, "damage", 3);
            SetPrivateField(tower, "projectilePrefab", arrowPrefab);
            SetPrivateField(tower, "rangeCircleRenderer", lr);

            string path = $"{dir}/ArcherTower.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateWarriorTowerPrefab(string dir, GameObject slashEffectPrefab)
        {
            GameObject obj = new GameObject("WarriorTower");
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/WarriorTower.png");
            sr.sortingOrder = 6;

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.sortingOrder = 15;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(1f, 0.3f, 0.3f, 0.5f);
            lr.endColor = new Color(1f, 0.3f, 0.3f, 0.5f);

            Tower tower = obj.AddComponent<Tower>();
            SetPrivateField(tower, "towerType", TowerType.Warrior);
            SetPrivateField(tower, "towerName", "Warrior Bastion");
            SetPrivateField(tower, "buildCost", 20);
            SetPrivateField(tower, "range", 2.2f);
            SetPrivateField(tower, "attacksPerSecond", 1.4f);
            SetPrivateField(tower, "damage", 4);
            SetPrivateField(tower, "meleeSlashEffectPrefab", slashEffectPrefab);
            SetPrivateField(tower, "rangeCircleRenderer", lr);

            string path = $"{dir}/WarriorTower.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateMageTowerPrefab(string dir, GameObject fireballPrefab)
        {
            GameObject obj = new GameObject("MageTower");
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MageTower.png");
            sr.sortingOrder = 6;

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.sortingOrder = 15;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.8f, 0.3f, 1f, 0.5f);
            lr.endColor = new Color(0.8f, 0.3f, 1f, 0.5f);

            Tower tower = obj.AddComponent<Tower>();
            SetPrivateField(tower, "towerType", TowerType.Mage);
            SetPrivateField(tower, "towerName", "Mage Spire");
            SetPrivateField(tower, "buildCost", 30);
            SetPrivateField(tower, "range", 3.6f);
            SetPrivateField(tower, "attacksPerSecond", 0.75f);
            SetPrivateField(tower, "damage", 6);
            SetPrivateField(tower, "projectilePrefab", fireballPrefab);
            SetPrivateField(tower, "aoeRadius", 1.8f);
            SetPrivateField(tower, "rangeCircleRenderer", lr);

            string path = $"{dir}/MageTower.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static void SetupSceneHierarchy(
            Tower archer, Tower warrior, Tower mage,
            Enemy skeleton, Enemy zombie, Enemy orc,
            GameObject hitMarkerPrefab)
        {
            // Clean up all existing scene root objects (except the Camera) to guarantee no duplicate Canvases or paths
            var rootObjs = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjs)
            {
                if (root.GetComponent<Camera>() != null) continue;
                Object.DestroyImmediate(root);
            }

            // Clear or repurpose camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;
            cam.backgroundColor = new Color(0.18f, 0.22f, 0.18f); // Dark tactical slate/grass
            cam.transform.position = new Vector3(0, 0, -10);

            // Ensure EventSystem
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                esObj.AddComponent<StandaloneInputModule>();
#endif
            }

            // Waypoint Path
            GameObject pathObj = new GameObject("WaypointPath");
            WaypointPath waypointPath = pathObj.AddComponent<WaypointPath>();
            Vector3[] points = new Vector3[]
            {
                new Vector3(-8.5f, 3.0f, 0),
                new Vector3(-3.0f, 3.0f, 0),
                new Vector3(-3.0f, -2.0f, 0),
                new Vector3(3.0f, -2.0f, 0),
                new Vector3(3.0f, 2.5f, 0),
                new Vector3(7.5f, 2.5f, 0)
            };

            List<Transform> wpList = new List<Transform>();
            for (int i = 0; i < points.Length; i++)
            {
                GameObject wp = new GameObject($"Waypoint_{i}");
                wp.transform.SetParent(pathObj.transform);
                wp.transform.position = points[i];
                wpList.Add(wp.transform);
            }
            SetPrivateField(waypointPath, "waypoints", wpList);

            // Spawn visual road tiles along path segments
            SpawnPathVisuals(points);

            // Spawn Castle / Base Goal at last waypoint
            GameObject castleObj = new GameObject("BaseCastle");
            castleObj.transform.position = points[points.Length - 1];
            SpriteRenderer castleSr = castleObj.AddComponent<SpriteRenderer>();
            castleSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BaseCastle.png");
            castleSr.sortingOrder = 7;

            // GameManager
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();

            // AudioManager
            GameObject amObj = new GameObject("AudioManager");
            AudioManager am = amObj.AddComponent<AudioManager>();
            SetPrivateField(am, "playerClickHitClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ClickHit.wav"));
            SetPrivateField(am, "archerShotClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ArcherShot.wav"));
            SetPrivateField(am, "warriorSwingClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/WarriorSwing.wav"));
            SetPrivateField(am, "mageCastClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MageCast.wav"));
            SetPrivateField(am, "magicExplosionClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MagicExplosion.wav"));
            SetPrivateField(am, "skeletonDeathClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SkeletonDeath.wav"));
            SetPrivateField(am, "zombieDeathClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ZombieDeath.wav"));
            SetPrivateField(am, "orcDeathClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/OrcDeath.wav"));
            SetPrivateField(am, "towerPlacedClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/TowerPlaced.wav"));
            SetPrivateField(am, "baseDamagedClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BaseDamaged.wav"));
            SetPrivateField(am, "gameOverClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/GameOver.wav"));
            SetPrivateField(am, "victoryClip", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Victory.wav"));

            // PlayerClickManager
            GameObject clickMgrObj = new GameObject("PlayerClickManager");
            PlayerClickManager clickMgr = clickMgrObj.AddComponent<PlayerClickManager>();
            SetPrivateField(clickMgr, "clickDamage", 1);
            SetPrivateField(clickMgr, "clickCooldown", 0.15f);
            SetPrivateField(clickMgr, "clickHitMarkerPrefab", hitMarkerPrefab);

            // TowerPlacementManager
            GameObject placementObj = new GameObject("TowerPlacementManager");
            TowerPlacementManager placementMgr = placementObj.AddComponent<TowerPlacementManager>();

            // TowerSelectionManager
            GameObject selectMgrObj = new GameObject("TowerSelectionManager");
            TowerSelectionManager selectMgr = selectMgrObj.AddComponent<TowerSelectionManager>();

            // WaveManager
            GameObject waveMgrObj = new GameObject("WaveManager");
            WaveManager waveMgr = waveMgrObj.AddComponent<WaveManager>();
            SetPrivateField(waveMgr, "assignedPath", waypointPath);

            // Setup 5 Escalating Waves
            List<WaveData> waveList = new List<WaveData>();

            // Wave 1: Skeletons (Fast, low HP - ideal for manual clicking early)
            WaveData w1 = new WaveData { waveName = "Wave 1 - Skeleton Scouts" };
            w1.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = skeleton, count = 6, spawnInterval = 1.6f, delayBeforeGroup = 1f });
            waveList.Add(w1);

            // Wave 2: Skeletons & Zombies
            WaveData w2 = new WaveData { waveName = "Wave 2 - Undead Vanguard" };
            w2.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = skeleton, count = 6, spawnInterval = 1.2f, delayBeforeGroup = 0.5f });
            w2.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = zombie, count = 4, spawnInterval = 1.8f, delayBeforeGroup = 1.5f });
            waveList.Add(w2);

            // Wave 3: Zombie Horde
            WaveData w3 = new WaveData { waveName = "Wave 3 - Zombie Horde" };
            w3.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = zombie, count = 8, spawnInterval = 1.3f, delayBeforeGroup = 0.5f });
            w3.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = skeleton, count = 6, spawnInterval = 0.9f, delayBeforeGroup = 1.0f });
            waveList.Add(w3);

            // Wave 4: Orc Brutes Arrive
            WaveData w4 = new WaveData { waveName = "Wave 4 - Orc Juggernauts" };
            w4.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = orc, count = 3, spawnInterval = 3.0f, delayBeforeGroup = 0.5f });
            w4.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = zombie, count = 6, spawnInterval = 1.2f, delayBeforeGroup = 1.0f });
            waveList.Add(w4);

            // Wave 5: Final Boss Swarm
            WaveData w5 = new WaveData { waveName = "Wave 5 - The Grand Assault" };
            w5.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = orc, count = 5, spawnInterval = 2.5f, delayBeforeGroup = 0.5f });
            w5.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = zombie, count = 8, spawnInterval = 1.0f, delayBeforeGroup = 1.0f });
            w5.spawnGroups.Add(new EnemySpawnGroup { enemyPrefab = skeleton, count = 10, spawnInterval = 0.7f, delayBeforeGroup = 0.5f });
            waveList.Add(w5);

            SetPrivateField(waveMgr, "waves", waveList);

            // Canvas & UI Setup
            BuildGameCanvas(archer, warrior, mage);
        }

        private static void SpawnPathVisuals(Vector3[] points)
        {
            GameObject container = new GameObject("PathVisuals");
            Sprite pathSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PathTile.png");

            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];
                float dist = Vector3.Distance(start, end);
                int steps = Mathf.CeilToInt(dist * 1.5f);

                for (int s = 0; s <= steps; s++)
                {
                    Vector3 pos = Vector3.Lerp(start, end, (float)s / steps);
                    GameObject tile = new GameObject($"Tile_{i}_{s}");
                    tile.transform.SetParent(container.transform);
                    tile.transform.position = pos;
                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = pathSprite;
                    sr.sortingOrder = 1;
                }
            }
        }

        private static void BuildGameCanvas(Tower archer, Tower warrior, Tower mage)
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // UIManager
            UIManager uiMgr = canvasObj.AddComponent<UIManager>();
            SetPrivateField(uiMgr, "archerPrefab", archer);
            SetPrivateField(uiMgr, "warriorPrefab", warrior);
            SetPrivateField(uiMgr, "magePrefab", mage);

            // Top Bar
            GameObject topBar = CreateUIPanel(canvasObj.transform, "TopBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 90), new Color(0.1f, 0.1f, 0.15f, 0.9f));

            TextMeshProUGUI livesText = CreateUIText(topBar.transform, "LivesText", "Lives: 25", 28, new Vector2(0, 0.5f), new Vector2(250, 60), new Vector2(160, 0), TextAlignmentOptions.Left);
            TextMeshProUGUI coinsText = CreateUIText(topBar.transform, "CoinsText", "Coins: 0", 28, new Vector2(0.28f, 0.5f), new Vector2(200, 60), Vector2.zero, TextAlignmentOptions.Left);
            TextMeshProUGUI waveText = CreateUIText(topBar.transform, "WaveText", "Wave: 1 / 5", 28, new Vector2(0.50f, 0.5f), new Vector2(180, 60), Vector2.zero, TextAlignmentOptions.Left);

            // Wave Progress Bar
            GameObject barBg = CreateUIPanel(topBar.transform, "WaveProgressBar", new Vector2(0.70f, 0.5f), new Vector2(0.70f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 22), new Color(0.08f, 0.08f, 0.12f, 1f));
            GameObject barFill = CreateUIPanel(barBg.transform, "Fill", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.75f, 0.95f, 1f));
            Image fillImg = barFill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;

            Button nextWaveBtn = CreateUIButton(topBar.transform, "NextWaveButton", "Start Wave", new Vector2(0.91f, 0.5f), new Vector2(180, 50), new Color(0.85f, 0.5f, 0.15f));

            SetPrivateField(uiMgr, "livesText", livesText);
            SetPrivateField(uiMgr, "coinsText", coinsText);
            SetPrivateField(uiMgr, "waveText", waveText);
            SetPrivateField(uiMgr, "waveProgressBarFill", fillImg);
            SetPrivateField(uiMgr, "nextWaveButton", nextWaveBtn);

            // Bottom Tower Tray
            GameObject bottomBar = CreateUIPanel(canvasObj.transform, "TowerTray", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(720, 110), new Color(0.12f, 0.12f, 0.18f, 0.92f));

            Button archerBtn = CreateUIButton(bottomBar.transform, "ArcherBtn", "Archer\n15g", new Vector2(0.2f, 0.5f), new Vector2(180, 80), new Color(0.2f, 0.6f, 0.3f));
            Button warriorBtn = CreateUIButton(bottomBar.transform, "WarriorBtn", "Warrior\n20g", new Vector2(0.5f, 0.5f), new Vector2(180, 80), new Color(0.7f, 0.25f, 0.25f));
            Button mageBtn = CreateUIButton(bottomBar.transform, "MageBtn", "Mage\n30g", new Vector2(0.8f, 0.5f), new Vector2(180, 80), new Color(0.5f, 0.25f, 0.8f));

            SetPrivateField(uiMgr, "archerButton", archerBtn);
            SetPrivateField(uiMgr, "warriorButton", warriorBtn);
            SetPrivateField(uiMgr, "mageButton", mageBtn);

            // Controls Hint Box
            GameObject hintBox = CreateUIPanel(canvasObj.transform, "ControlsHint", new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 90), new Color(0.05f, 0.05f, 0.08f, 0.88f));
            CreateUIText(hintBox.transform, "HintLabel", "Left Click enemies to attack! | Click Tower to place | Right Click cancels\nDefeat enemies to earn coins for towers!", 22, new Vector2(0.5f, 0.5f), new Vector2(680, 80), Vector2.zero, TextAlignmentOptions.Center);
            SetPrivateField(uiMgr, "startControlsHintPanel", hintBox);

            // Tower Info Panel (Inspector on right side)
            GameObject infoPanel = CreateUIPanel(canvasObj.transform, "TowerInfoPanel", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(340, 260), new Color(0.12f, 0.12f, 0.18f, 0.95f));

            TextMeshProUGUI infoTitle = CreateUIText(infoPanel.transform, "InfoTitle", "Archer Tower (Lv 1)", 24, new Vector2(0.42f, 0.85f), new Vector2(240, 35), Vector2.zero, TextAlignmentOptions.Left);
            Button closeBtn = CreateUIButton(infoPanel.transform, "CloseBtn", "X", new Vector2(0.92f, 0.85f), new Vector2(36, 36), new Color(0.6f, 0.2f, 0.2f));

            TextMeshProUGUI statsText = CreateUIText(infoPanel.transform, "StatsText", "Dmg: 3 | Range: 4.0 | Rate: 1.2/s", 18, new Vector2(0.5f, 0.65f), new Vector2(310, 40), Vector2.zero, TextAlignmentOptions.Center);

            Button targetBtn = CreateUIButton(infoPanel.transform, "TargetBtn", "Target: First", new Vector2(0.5f, 0.44f), new Vector2(290, 42), new Color(0.25f, 0.45f, 0.7f));
            TextMeshProUGUI targetBtnText = targetBtn.GetComponentInChildren<TextMeshProUGUI>();

            Button upgradeBtn = CreateUIButton(infoPanel.transform, "UpgradeBtn", "Upgrade (11g)", new Vector2(0.32f, 0.18f), new Vector2(160, 48), new Color(0.2f, 0.65f, 0.3f));
            TextMeshProUGUI upgradeBtnText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();

            Button sellBtn = CreateUIButton(infoPanel.transform, "SellBtn", "Sell (+9g)", new Vector2(0.78f, 0.18f), new Vector2(120, 48), new Color(0.65f, 0.3f, 0.2f));
            TextMeshProUGUI sellBtnText = sellBtn.GetComponentInChildren<TextMeshProUGUI>();

            TowerInfoUI infoUI = canvasObj.AddComponent<TowerInfoUI>();
            SetPrivateField(infoUI, "panelRoot", infoPanel);
            SetPrivateField(infoUI, "titleText", infoTitle);
            SetPrivateField(infoUI, "statsText", statsText);
            SetPrivateField(infoUI, "targetButton", targetBtn);
            SetPrivateField(infoUI, "targetButtonText", targetBtnText);
            SetPrivateField(infoUI, "upgradeButton", upgradeBtn);
            SetPrivateField(infoUI, "upgradeButtonText", upgradeBtnText);
            SetPrivateField(infoUI, "sellButton", sellBtn);
            SetPrivateField(infoUI, "sellButtonText", sellBtnText);
            SetPrivateField(infoUI, "closeButton", closeBtn);

            infoPanel.SetActive(false);

            // Game Over Modal
            GameObject goPanel = CreateUIPanel(canvasObj.transform, "GameOverPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 320), new Color(0.2f, 0.05f, 0.05f, 0.96f));
            CreateUIText(goPanel.transform, "Title", "GAME OVER", 48, new Vector2(0.5f, 0.72f), new Vector2(400, 70), Vector2.zero, TextAlignmentOptions.Center);
            CreateUIText(goPanel.transform, "Desc", "Enemies breached your base defenses!", 24, new Vector2(0.5f, 0.48f), new Vector2(400, 50), Vector2.zero, TextAlignmentOptions.Center);
            Button goRestart = CreateUIButton(goPanel.transform, "RestartBtn", "Try Again", new Vector2(0.5f, 0.22f), new Vector2(240, 60), new Color(0.85f, 0.25f, 0.25f));
            goPanel.SetActive(false);
            SetPrivateField(uiMgr, "gameOverPanel", goPanel);
            SetPrivateField(uiMgr, "restartButtonGameOver", goRestart);

            // Victory Modal
            GameObject vicPanel = CreateUIPanel(canvasObj.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 320), new Color(0.08f, 0.2f, 0.1f, 0.96f));
            CreateUIText(vicPanel.transform, "Title", "VICTORY!", 48, new Vector2(0.5f, 0.72f), new Vector2(400, 70), Vector2.zero, TextAlignmentOptions.Center);
            CreateUIText(vicPanel.transform, "Desc", "You defended the base against all waves!", 24, new Vector2(0.5f, 0.48f), new Vector2(400, 50), Vector2.zero, TextAlignmentOptions.Center);
            Button vicRestart = CreateUIButton(vicPanel.transform, "PlayAgainBtn", "Play Again", new Vector2(0.5f, 0.22f), new Vector2(240, 60), new Color(0.25f, 0.75f, 0.35f));
            vicPanel.SetActive(false);
            SetPrivateField(uiMgr, "victoryPanel", vicPanel);
            SetPrivateField(uiMgr, "restartButtonVictory", vicRestart);
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string name, string text, float fontSize, Vector2 anchorPosRatio, Vector2 size, Vector2 posOffset, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorPosRatio;
            rt.anchorMax = anchorPosRatio;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = posOffset;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 anchorRatio, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorRatio;
            rt.anchorMax = anchorRatio;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.color = color;

            Button btn = obj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null) return;
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
#endif
