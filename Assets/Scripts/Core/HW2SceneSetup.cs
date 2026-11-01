#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using HW2.Core;
using HW2.Player;
using HW2.Road;
using HW2.Interactables;
using HW2.Pooling;
using HW2.Difficulty;
using HW2.VFX;
using HW2.UI;

namespace HW2.EditorUtils
{
    public static class HW2SceneSetup
    {
        private static Shader litShader;
        private static Shader unlitShader;
        private static TMP_FontAsset primaryFont;
        private static TMP_FontAsset titleFont;
        private static InputActionAsset inputActionsAsset;

        [MenuItem("HW2/Build Everything (Prefabs, VFX, Scenes)")]
        public static void BuildEverything()
        {
            litShader = Shader.Find("Universal Render Pipeline/Lit");
            unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

            LoadFontsAndInput();
            EnsureDirectories();

            var mats = CreateMaterials();
            var prefabs = CreatePrefabs(mats);

            SetupGameScene(prefabs);
            SetupMainMenuScene(prefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("🎉 HW2 Full Build with all fixes completed successfully!");
        }

        private static void LoadFontsAndInput()
        {
            primaryFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
            if (primaryFont == null)
            {
                primaryFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            }

            titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Electronic Highway Sign SDF.asset");
            if (titleFont == null) titleFont = primaryFont;

            inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = new string[]
            {
                "Assets/Materials",
                "Assets/Prefabs/Road",
                "Assets/Prefabs/Player",
                "Assets/Prefabs/Obstacles",
                "Assets/Prefabs/Collectibles",
                "Assets/Prefabs/VFX",
                "Assets/Scenes"
            };
            foreach (var d in dirs)
            {
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
        }

        public class MaterialsCollection
        {
            public Material roadMat;
            public Material roadStripeMat;
            public Material barrierMat;
            public Material playerBodyMat;
            public Material playerGlassMat;
            public Material playerWheelMat;
            public Material playerNeonMat;
            public Material headlightMat;
            public Material taillightMat;
            public Material staticObstacleMat;
            public Material mobileObstacleMat;
            public Material coinMat;
            public Material fuelCanMat;
            public Material streetLampMat;
            public Material lampLightMat;
            public Material exhaustParticleMat;
        }

        private static MaterialsCollection CreateMaterials()
        {
            var m = new MaterialsCollection();

            m.roadMat = GetOrCreateMaterial("Assets/Materials/RoadMat.mat", new Color(0.10f, 0.10f, 0.12f), 0.1f, 0.85f);
            m.roadStripeMat = GetOrCreateMaterial("Assets/Materials/RoadStripeMat.mat", new Color(1f, 0.95f, 0.3f), 0.2f, 0.4f, new Color(1f, 0.85f, 0.2f) * 2.0f);
            m.barrierMat = GetOrCreateMaterial("Assets/Materials/BarrierMat.mat", new Color(0.2f, 0.25f, 0.35f), 0.5f, 0.4f, new Color(0f, 0.5f, 1f) * 0.8f);

            // Player car: Deep glossy midnight blue with cyan accents
            m.playerBodyMat = GetOrCreateMaterial("Assets/Materials/PlayerBodyMat.mat", new Color(0.01f, 0.12f, 0.32f), 0.85f, 0.15f);
            m.playerGlassMat = GetOrCreateMaterial("Assets/Materials/PlayerGlassMat.mat", new Color(0.04f, 0.04f, 0.07f), 0.95f, 0.05f);
            m.playerWheelMat = GetOrCreateMaterial("Assets/Materials/PlayerWheelMat.mat", new Color(0.07f, 0.07f, 0.07f), 0.2f, 0.7f);

            // Emissive materials for Bloom
            m.playerNeonMat = GetOrCreateMaterial("Assets/Materials/PlayerNeonMat.mat", new Color(0f, 0.95f, 1f), 0.1f, 0.2f, new Color(0f, 0.95f, 1f) * 4.0f);
            m.headlightMat = GetOrCreateMaterial("Assets/Materials/HeadlightMat.mat", Color.white, 0.1f, 0.1f, Color.white * 4.5f);
            m.taillightMat = GetOrCreateMaterial("Assets/Materials/TaillightMat.mat", new Color(1f, 0.05f, 0.15f), 0.1f, 0.2f, new Color(1f, 0.05f, 0.15f) * 4.0f);

            // Obstacles
            m.staticObstacleMat = GetOrCreateMaterial("Assets/Materials/StaticObstacleMat.mat", new Color(0.85f, 0.08f, 0.15f), 0.5f, 0.4f, new Color(0.85f, 0.08f, 0.15f) * 0.5f);
            m.mobileObstacleMat = GetOrCreateMaterial("Assets/Materials/MobileObstacleMat.mat", new Color(1.0f, 0.55f, 0.02f), 0.5f, 0.4f, new Color(1.0f, 0.55f, 0.02f) * 0.5f);

            // Collectibles
            m.coinMat = GetOrCreateMaterial("Assets/Materials/CoinMat.mat", new Color(1f, 0.85f, 0.1f), 0.9f, 0.2f, new Color(1f, 0.85f, 0.1f) * 2.5f);
            m.fuelCanMat = GetOrCreateMaterial("Assets/Materials/FuelCanMat.mat", new Color(0.05f, 0.95f, 0.4f), 0.3f, 0.3f, new Color(0.05f, 0.95f, 0.4f) * 3.0f);

            // Street Lamps
            m.streetLampMat = GetOrCreateMaterial("Assets/Materials/StreetLampMat.mat", new Color(0.15f, 0.16f, 0.2f), 0.6f, 0.4f);
            m.lampLightMat = GetOrCreateMaterial("Assets/Materials/LampLightMat.mat", new Color(1f, 0.78f, 0.4f), 0.1f, 0.1f, new Color(1f, 0.75f, 0.35f) * 3.5f);

            // Exhaust particles
            m.exhaustParticleMat = GetOrCreateMaterial("Assets/Materials/ExhaustParticleMat.mat", new Color(0f, 0.85f, 1f, 0.85f), 0.1f, 0.1f, new Color(0f, 0.85f, 1f) * 2.5f);

            return m;
        }

        private static Material GetOrCreateMaterial(string path, Color baseColor, float metallic, float smoothness, Color? emission = null)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(litShader != null ? litShader : Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else if (mat.HasProperty("_Color")) mat.color = baseColor;

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public class PrefabsCollection
        {
            public GameObject roadSegmentPrefab;
            public GameObject sparkBurstPrefab;
            public GameObject fireworksBurstPrefab;
            public GameObject coinPrefab;
            public GameObject fuelCanPrefab;
            public GameObject staticObstaclePrefab;
            public GameObject mobileObstaclePrefab;
            public GameObject playerCarPrefab;
        }

        private static PrefabsCollection CreatePrefabs(MaterialsCollection mats)
        {
            var p = new PrefabsCollection();

            p.sparkBurstPrefab = CreateSparkBurstPrefab(mats);
            p.fireworksBurstPrefab = CreateFireworksBurstPrefab();
            p.coinPrefab = CreateCoinPrefab(mats, p.sparkBurstPrefab);
            p.fuelCanPrefab = CreateFuelCanPrefab(mats, p.sparkBurstPrefab);
            p.staticObstaclePrefab = CreateObstaclePrefab("ObstacleStatic", mats.staticObstacleMat, mats, p.sparkBurstPrefab, isMobile: false);
            p.mobileObstaclePrefab = CreateObstaclePrefab("ObstacleMobile", mats.mobileObstacleMat, mats, p.sparkBurstPrefab, isMobile: true);
            p.roadSegmentPrefab = CreateRoadSegmentPrefab(mats);
            p.playerCarPrefab = CreatePlayerCarPrefab(mats);

            return p;
        }

        private static GameObject CreateSparkBurstPrefab(MaterialsCollection mats)
        {
            string path = "Assets/Prefabs/VFX/SparkBurst.prefab";
            GameObject go = new GameObject("SparkBurst");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 16f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            main.startColor = new Color(1f, 0.65f, 0.1f);
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (mats.exhaustParticleMat != null) renderer.material = mats.exhaustParticleMat;

            go.AddComponent<ParticleAutoReturn>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateFireworksBurstPrefab()
        {
            string path = "Assets/Prefabs/VFX/FireworksBurst.prefab";
            GameObject go = new GameObject("FireworksBurst");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1.0f;
            main.loop = false;
            main.startLifetime = 1.8f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(14f, 32f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.gravityModifier = 0.35f;

            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.1f, 0.4f), 0f), new GradientColorKey(new Color(0f, 0.95f, 1f), 0.5f), new GradientColorKey(new Color(1f, 0.95f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(grad);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 140) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var lightObj = new GameObject("BurstLight");
            lightObj.transform.SetParent(go.transform);
            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.85f, 0.65f, 1f);
            l.range = 30f;
            l.intensity = 10f;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateCoinPrefab(MaterialsCollection mats, GameObject sparkPrefab)
        {
            string path = "Assets/Prefabs/Collectibles/Coin.prefab";
            GameObject go = new GameObject("Coin");

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "CoinMesh";
            visual.transform.SetParent(go.transform);
            visual.transform.localScale = new Vector3(0.9f, 0.12f, 0.9f);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.GetComponent<Renderer>().material = mats.coinMat;
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.9f;

            var coin = go.AddComponent<Coin>();
            SetSerializedField(coin, "pickupEffectPrefab", sparkPrefab);
            SetSerializedField(coin, "rotationSpeed", 180f);

            // Auto recycle safely when 35m behind player
            go.AddComponent<AutoDespawnBehindPlayer>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateFuelCanPrefab(MaterialsCollection mats, GameObject sparkPrefab)
        {
            string path = "Assets/Prefabs/Collectibles/FuelCan.prefab";
            GameObject go = new GameObject("FuelCan");

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CanisterBody";
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(0.65f, 0.95f, 0.45f);
            body.GetComponent<Renderer>().material = mats.fuelCanMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(go.transform);
            handle.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            handle.transform.localScale = new Vector3(0.35f, 0.15f, 0.15f);
            handle.GetComponent<Renderer>().material = mats.streetLampMat;
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            GameObject spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spout.name = "Spout";
            spout.transform.SetParent(go.transform);
            spout.transform.localPosition = new Vector3(0.22f, 0.58f, 0f);
            spout.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
            spout.GetComponent<Renderer>().material = mats.streetLampMat;
            Object.DestroyImmediate(spout.GetComponent<Collider>());

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.1f, 1.5f, 1.1f);

            var fuel = go.AddComponent<FuelCan>();
            SetSerializedField(fuel, "pickupEffectPrefab", sparkPrefab);

            // Auto recycle safely when 35m behind player
            go.AddComponent<AutoDespawnBehindPlayer>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateObstaclePrefab(string name, Material carMat, MaterialsCollection mats, GameObject sparkPrefab, bool isMobile)
        {
            string path = $"Assets/Prefabs/Obstacles/{name}.prefab";
            GameObject go = new GameObject(name);

            // Car body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Chassis";
            body.transform.SetParent(go.transform);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(1.85f, 0.65f, 3.8f);
            body.GetComponent<Renderer>().material = carMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Cabin
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(go.transform);
            cabin.transform.localPosition = new Vector3(0f, 0.85f, -0.2f);
            cabin.transform.localScale = new Vector3(1.5f, 0.6f, 2.0f);
            cabin.GetComponent<Renderer>().material = mats.playerGlassMat;
            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            // Taillights
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tl.name = "Taillight";
                tl.transform.SetParent(go.transform);
                tl.transform.localPosition = new Vector3(i * 0.7f, 0.45f, -1.92f);
                tl.transform.localScale = new Vector3(0.3f, 0.15f, 0.08f);
                tl.GetComponent<Renderer>().material = mats.taillightMat;
                Object.DestroyImmediate(tl.GetComponent<Collider>());
            }

            // Headlights
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hl.name = "Headlight";
                hl.transform.SetParent(go.transform);
                hl.transform.localPosition = new Vector3(i * 0.7f, 0.45f, 1.92f);
                hl.transform.localScale = new Vector3(0.3f, 0.15f, 0.08f);
                hl.GetComponent<Renderer>().material = mats.headlightMat;
                Object.DestroyImmediate(hl.GetComponent<Collider>());
            }

            // 4 Wheels
            Vector3[] wheelPositions = new Vector3[]
            {
                new Vector3(-0.95f, 0.25f, 1.2f),
                new Vector3(0.95f, 0.25f, 1.2f),
                new Vector3(-0.95f, 0.25f, -1.2f),
                new Vector3(0.95f, 0.25f, -1.2f)
            };
            foreach (var wp in wheelPositions)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = "Wheel";
                w.transform.SetParent(go.transform);
                w.transform.localPosition = wp;
                w.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                w.GetComponent<Renderer>().material = mats.playerWheelMat;
                Object.DestroyImmediate(w.GetComponent<Collider>());
            }

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.6f, 0f);
            col.size = new Vector3(2.0f, 1.2f, 4.0f);

            if (isMobile)
            {
                var mob = go.AddComponent<ObstacleMobile>();
                SetSerializedField(mob, "pickupEffectPrefab", sparkPrefab);
                SetSerializedField(mob, "speed", 14f); // Cruising speed
            }
            else
            {
                var stat = go.AddComponent<ObstacleStatic>();
                SetSerializedField(stat, "pickupEffectPrefab", sparkPrefab);
            }

            // Auto recycle safely when 35m behind player (prevents sudden disappearing)
            go.AddComponent<AutoDespawnBehindPlayer>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateRoadSegmentPrefab(MaterialsCollection mats)
        {
            string path = "Assets/Prefabs/Road/RoadSegment.prefab";
            GameObject go = new GameObject("RoadSegment");

            // Road surface: 12.5m wide, 50m long
            GameObject roadSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadSurface.name = "RoadSurface";
            roadSurface.transform.SetParent(go.transform);
            roadSurface.transform.localPosition = new Vector3(0f, -0.1f, 25f);
            roadSurface.transform.localScale = new Vector3(12.5f, 0.2f, 50f);
            roadSurface.GetComponent<Renderer>().material = mats.roadMat;

            // Lane Divider Markings (2 lines at X = -1.75 and X = 1.75)
            float[] stripeX = new float[] { -1.75f, 1.75f };
            for (int lx = 0; lx < stripeX.Length; lx++)
            {
                for (int z = 2; z < 50; z += 6)
                {
                    GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stripe.name = "LaneMarking";
                    stripe.transform.SetParent(go.transform);
                    stripe.transform.localPosition = new Vector3(stripeX[lx], 0.01f, z);
                    stripe.transform.localScale = new Vector3(0.18f, 0.03f, 3.2f);
                    stripe.GetComponent<Renderer>().material = mats.roadStripeMat;
                    Object.DestroyImmediate(stripe.GetComponent<Collider>());
                }
            }

            // Guardrails on Left & Right
            float[] railX = new float[] { -6.1f, 6.1f };
            for (int r = 0; r < railX.Length; r++)
            {
                GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = "Guardrail";
                rail.transform.SetParent(go.transform);
                rail.transform.localPosition = new Vector3(railX[r], 0.4f, 25f);
                rail.transform.localScale = new Vector3(0.35f, 0.8f, 50f);
                rail.GetComponent<Renderer>().material = mats.barrierMat;
            }

            // Street Lamps (Left & Right at z=12.5 and z=37.5)
            float[] lampZ = new float[] { 12.5f, 37.5f };
            for (int i = 0; i < lampZ.Length; i++)
            {
                CreateStreetLamp(go.transform, new Vector3(-6.5f, 0f, lampZ[i]), 1f, mats);
                CreateStreetLamp(go.transform, new Vector3(6.5f, 0f, lampZ[i]), -1f, mats);
            }

            var seg = go.AddComponent<RoadSegment>();
            SetSerializedField(seg, "segmentLength", 50f);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateStreetLamp(Transform parent, Vector3 localPos, float sideSign, MaterialsCollection mats)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StreetLampPole";
            pole.transform.SetParent(parent);
            pole.transform.localPosition = localPos + new Vector3(0f, 3.2f, 0f);
            pole.transform.localScale = new Vector3(0.2f, 3.2f, 0.2f);
            pole.GetComponent<Renderer>().material = mats.streetLampMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "LampHead";
            head.transform.SetParent(parent);
            head.transform.localPosition = localPos + new Vector3(sideSign * 0.8f, 6.4f, 0f);
            head.transform.localScale = new Vector3(1.2f, 0.25f, 0.5f);
            head.GetComponent<Renderer>().material = mats.lampLightMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            GameObject lightObj = new GameObject("StreetLight");
            lightObj.transform.SetParent(parent);
            lightObj.transform.localPosition = localPos + new Vector3(sideSign * 1.0f, 6.2f, 0f);
            lightObj.transform.localRotation = Quaternion.Euler(75f, sideSign * 45f, 0f);

            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = new Color(1.0f, 0.82f, 0.5f);
            l.range = 16f;
            l.spotAngle = 65f;
            l.intensity = 5.0f;
        }

        private static GameObject CreatePlayerCarPrefab(MaterialsCollection mats)
        {
            string path = "Assets/Prefabs/Player/PlayerCar.prefab";
            GameObject go = new GameObject("PlayerCar");

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.3f;
            cc.radius = 0.95f;
            cc.center = new Vector3(0f, 0.65f, 0f);

            GameObject visuals = new GameObject("Visuals");
            visuals.transform.SetParent(go.transform);

            // Sleek Car Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Chassis";
            body.transform.SetParent(visuals.transform);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(1.85f, 0.6f, 4.0f);
            body.GetComponent<Renderer>().material = mats.playerBodyMat;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Cabin / Windshield
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(visuals.transform);
            cabin.transform.localPosition = new Vector3(0f, 0.82f, -0.25f);
            cabin.transform.localScale = new Vector3(1.5f, 0.55f, 2.1f);
            cabin.GetComponent<Renderer>().material = mats.playerGlassMat;
            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            // Rear Spoiler
            GameObject spoiler = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spoiler.name = "Spoiler";
            spoiler.transform.SetParent(visuals.transform);
            spoiler.transform.localPosition = new Vector3(0f, 0.9f, -1.95f);
            spoiler.transform.localScale = new Vector3(1.7f, 0.1f, 0.4f);
            spoiler.GetComponent<Renderer>().material = mats.playerBodyMat;
            Object.DestroyImmediate(spoiler.GetComponent<Collider>());

            // 4 Wheels
            Vector3[] wheelPositions = new Vector3[]
            {
                new Vector3(-0.95f, 0.28f, 1.25f),
                new Vector3(0.95f, 0.28f, 1.25f),
                new Vector3(-0.95f, 0.28f, -1.25f),
                new Vector3(0.95f, 0.28f, -1.25f)
            };
            foreach (var wp in wheelPositions)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                w.name = "Wheel";
                w.transform.SetParent(visuals.transform);
                w.transform.localPosition = wp;
                w.transform.localScale = new Vector3(0.65f, 0.22f, 0.65f);
                w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                w.GetComponent<Renderer>().material = mats.playerWheelMat;
                Object.DestroyImmediate(w.GetComponent<Collider>());
            }

            // Headlights
            Light[] headLightsArray = new Light[2];
            int hlIdx = 0;
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject hlMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hlMesh.name = "HeadlightMesh";
                hlMesh.transform.SetParent(visuals.transform);
                hlMesh.transform.localPosition = new Vector3(i * 0.72f, 0.45f, 2.01f);
                hlMesh.transform.localScale = new Vector3(0.32f, 0.16f, 0.05f);
                hlMesh.GetComponent<Renderer>().material = mats.headlightMat;
                Object.DestroyImmediate(hlMesh.GetComponent<Collider>());

                GameObject spotObj = new GameObject("HeadlightSpot");
                spotObj.transform.SetParent(visuals.transform);
                spotObj.transform.localPosition = new Vector3(i * 0.72f, 0.5f, 2.1f);
                spotObj.transform.localRotation = Quaternion.Euler(10f, i * 4f, 0f);
                var spot = spotObj.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(0.95f, 0.98f, 1.0f);
                spot.range = 50f;
                spot.spotAngle = 55f;
                spot.intensity = 5.5f;
                headLightsArray[hlIdx++] = spot;
            }

            // Taillights
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject tlMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tlMesh.name = "TaillightMesh";
                tlMesh.transform.SetParent(visuals.transform);
                tlMesh.transform.localPosition = new Vector3(i * 0.72f, 0.45f, -2.01f);
                tlMesh.transform.localScale = new Vector3(0.32f, 0.16f, 0.05f);
                tlMesh.GetComponent<Renderer>().material = mats.taillightMat;
                Object.DestroyImmediate(tlMesh.GetComponent<Collider>());
            }

            // Underglow Neon strip underneath chassis
            GameObject neonStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonStrip.name = "NeonUnderglowStrip";
            neonStrip.transform.SetParent(visuals.transform);
            neonStrip.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            neonStrip.transform.localScale = new Vector3(1.7f, 0.05f, 3.6f);
            neonStrip.GetComponent<Renderer>().material = mats.playerNeonMat;
            Object.DestroyImmediate(neonStrip.GetComponent<Collider>());

            // Underglow point lights
            Light[] underglowArray = new Light[2];
            for (int i = -1; i <= 1; i += 2)
            {
                int uIdx = i == -1 ? 0 : 1;
                GameObject ugObj = new GameObject("UnderglowLight");
                ugObj.transform.SetParent(visuals.transform);
                ugObj.transform.localPosition = new Vector3(0f, 0.1f, i * 0.9f);
                var ugl = ugObj.AddComponent<Light>();
                ugl.type = LightType.Point;
                ugl.color = new Color(0f, 0.9f, 1f);
                ugl.range = 5.5f;
                ugl.intensity = 3.5f;
                underglowArray[uIdx] = ugl;
            }

            // Exhaust Particle Systems
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject exhaustObj = new GameObject("ExhaustPipe");
                exhaustObj.transform.SetParent(visuals.transform);
                exhaustObj.transform.localPosition = new Vector3(i * 0.5f, 0.2f, -2.05f);
                exhaustObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                var ps = exhaustObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 0.4f;
                main.startSpeed = 10f;
                main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.4f);
                main.startColor = new Color(0f, 0.85f, 1f, 0.9f);

                var emission = ps.emission;
                emission.rateOverTime = 35f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 12f;
                shape.radius = 0.08f;

                var rend = exhaustObj.GetComponent<ParticleSystemRenderer>();
                if (mats.exhaustParticleMat != null) rend.material = mats.exhaustParticleMat;
            }

            var pc = go.AddComponent<PlayerController>();
            var ph = go.AddComponent<PlayerHealth>();
            var fs = go.AddComponent<FuelSystem>();
            var neonCtrl = go.AddComponent<NeonLightController>();

            SetSerializedField(pc, "visualsContainer", visuals.transform);
            SetSerializedField(pc, "health", ph);
            SetSerializedField(pc, "fuel", fs);
            SetSerializedField(pc, "laneChangeSpeed", 22f);

            SetSerializedField(neonCtrl, "underglowLights", underglowArray);
            SetSerializedField(neonCtrl, "headlights", headLightsArray);
            SetSerializedField(neonCtrl, "neonStripRenderer", neonStrip.GetComponent<Renderer>());

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void SetupGameScene(PrefabsCollection prefabs)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);

            var roots = scene.GetRootGameObjects();
            foreach (var r in roots) Object.DestroyImmediate(r);

            // Night Lighting
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.06f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.11f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.012f;

            GameObject dirLightObj = new GameObject("NightDirectionalLight");
            dirLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var dirLight = dirLightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(0.35f, 0.5f, 0.75f);
            dirLight.intensity = 0.4f;

            // Global Post-Processing Volume (Bloom, Color, Vignette)
            GameObject volumeObj = new GameObject("GlobalPostProcessVolume");
            var volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "GameSceneVolumeProfile";
            AssetDatabase.CreateAsset(profile, "Assets/Settings/GameSceneVolumeProfile.asset");
            volume.profile = profile;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 1.8f;
            bloom.threshold.value = 0.85f;
            bloom.scatter.value = 0.7f;

            ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.postExposure.value = 0.3f;
            colorAdj.contrast.value = 18f;
            colorAdj.saturation.value = 15f;

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.32f;
            vignette.smoothness.value = 0.35f;

            // Core Managers
            GameObject managersObj = new GameObject("CoreManagers");
            managersObj.AddComponent<GameManager>();
            managersObj.AddComponent<ScoreManager>();
            managersObj.AddComponent<AudioManager>();
            var dm = managersObj.AddComponent<DifficultyManager>();
            SetSerializedField(dm, "initialSpeed", 28f);
            SetSerializedField(dm, "maxSpeed", 56f);
            SetSerializedField(dm, "speedAccelerationPerSecond", 0.35f);
            managersObj.AddComponent<PoolManager>();

            // Fireworks Controller
            GameObject fireworksObj = new GameObject("FireworksController");
            var fc = fireworksObj.AddComponent<FireworksController>();
            SetSerializedField(fc, "fireworksPrefabs", new ParticleSystem[] { prefabs.fireworksBurstPrefab.GetComponent<ParticleSystem>() });

            // Spawn Player
            GameObject player = PrefabUtility.InstantiatePrefab(prefabs.playerCarPrefab) as GameObject;
            player.name = "PlayerCar";
            player.transform.position = new Vector3(0f, 0.1f, 0f);

            // Camera Setup
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.04f, 0.1f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            var follow = camObj.AddComponent<CameraFollow>();
            SetSerializedField(follow, "target", player.transform);
            SetSerializedField(follow, "offset", new Vector3(0f, 4.4f, -7.8f));

            // Road Generator
            GameObject roadGenObj = new GameObject("RoadGenerator");
            var rg = roadGenObj.AddComponent<RoadGenerator>();
            SetSerializedField(rg, "roadSegmentPrefab", prefabs.roadSegmentPrefab);
            SetSerializedField(rg, "staticObstaclePrefab", prefabs.staticObstaclePrefab);
            SetSerializedField(rg, "mobileObstaclePrefab", prefabs.mobileObstaclePrefab);
            SetSerializedField(rg, "coinPrefab", prefabs.coinPrefab);
            SetSerializedField(rg, "fuelCanPrefab", prefabs.fuelCanPrefab);
            SetSerializedField(rg, "playerTransform", player.transform);

            // UI Canvas Setup
            SetupGameUI(player);

            EditorSceneManager.SaveScene(scene);
        }

        private static void SetupGameUI(GameObject player)
        {
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Event System with New Input System actions asset wired!
            GameObject eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModule = eventSys.AddComponent<InputSystemUIInputModule>();
            if (inputActionsAsset != null)
            {
                inputModule.actionsAsset = inputActionsAsset;
                inputModule.AssignDefaultActions();
            }

            // HUD Panel
            GameObject hudPanel = new GameObject("HUDPanel");
            hudPanel.transform.SetParent(canvasObj.transform, false);
            var hudRect = hudPanel.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.sizeDelta = Vector2.zero;

            // --- Top Left: Speed & Fuel ---
            GameObject speedObj = CreateTMPText(hudPanel.transform, "SpeedText", "0 KM/H", 46, TextAlignmentOptions.TopLeft);
            var speedRect = speedObj.GetComponent<RectTransform>();
            speedRect.anchorMin = new Vector2(0f, 1f);
            speedRect.anchorMax = new Vector2(0f, 1f);
            speedRect.pivot = new Vector2(0f, 1f);
            speedRect.anchoredPosition = new Vector2(40f, -35f);
            speedRect.sizeDelta = new Vector2(340f, 65f);

            // Fuel Bar Container
            GameObject fuelObj = new GameObject("FuelSlider");
            fuelObj.transform.SetParent(hudPanel.transform, false);
            var fuelSlider = fuelObj.AddComponent<Slider>();
            var fuelRect = fuelObj.GetComponent<RectTransform>();
            fuelRect.anchorMin = new Vector2(0f, 1f);
            fuelRect.anchorMax = new Vector2(0f, 1f);
            fuelRect.pivot = new Vector2(0f, 1f);
            fuelRect.anchoredPosition = new Vector2(40f, -105f);
            fuelRect.sizeDelta = new Vector2(300f, 28f);

            GameObject fuelBg = new GameObject("Background");
            fuelBg.transform.SetParent(fuelObj.transform, false);
            var fuelBgImg = fuelBg.AddComponent<Image>();
            fuelBgImg.color = new Color(0.1f, 0.1f, 0.18f, 0.95f);
            var fuelBgRect = fuelBg.GetComponent<RectTransform>();
            fuelBgRect.anchorMin = Vector2.zero;
            fuelBgRect.anchorMax = Vector2.one;
            fuelBgRect.sizeDelta = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(fuelObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-8f, -8f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.1f, 0.95f, 0.35f);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;

            fuelSlider.targetGraphic = fillImg;
            fuelSlider.fillRect = fillRect;
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = 1f;
            fuelSlider.value = 1f;

            GameObject fuelPercentObj = CreateTMPText(hudPanel.transform, "FuelPercent", "FUEL: 100%", 20, TextAlignmentOptions.Left);
            var fuelPercentRect = fuelPercentObj.GetComponent<RectTransform>();
            fuelPercentRect.anchorMin = new Vector2(0f, 1f);
            fuelPercentRect.anchorMax = new Vector2(0f, 1f);
            fuelPercentRect.pivot = new Vector2(0f, 1f);
            fuelPercentRect.anchoredPosition = new Vector2(40f, -138f);
            fuelPercentRect.sizeDelta = new Vector2(200f, 26f);

            // --- LIVES DISPLAY (CLEAN: TITLE + 3 SQUARES ONLY) ---
            GameObject livesBox = new GameObject("LivesBox");
            livesBox.transform.SetParent(hudPanel.transform, false);
            var livesBoxRect = livesBox.AddComponent<RectTransform>();
            livesBoxRect.anchorMin = new Vector2(0f, 1f);
            livesBoxRect.anchorMax = new Vector2(0f, 1f);
            livesBoxRect.pivot = new Vector2(0f, 1f);
            livesBoxRect.anchoredPosition = new Vector2(40f, -175f);
            livesBoxRect.sizeDelta = new Vector2(300f, 50f);

            var livesBoxBg = livesBox.AddComponent<Image>();
            livesBoxBg.color = new Color(0.04f, 0.08f, 0.18f, 0.85f);

            GameObject livesTitleObj = CreateTMPText(livesBox.transform, "LivesTitle", "LIVES: 3/3", 26, TextAlignmentOptions.Left);
            var ltr = livesTitleObj.GetComponent<RectTransform>();
            ltr.anchoredPosition = new Vector2(16f, 0f);
            ltr.sizeDelta = new Vector2(150f, 36f);

            // 3 Life Badges (Square Boxes: 3 green, 2 yellow, 1 red)
            GameObject[] lifeBadges = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject badge = new GameObject($"Badge_{i}");
                badge.transform.SetParent(livesBox.transform, false);
                var br = badge.AddComponent<RectTransform>();
                br.anchoredPosition = new Vector2(175f + (i * 38f), 0f);
                br.sizeDelta = new Vector2(28f, 28f);
                var bImg = badge.AddComponent<Image>();
                bImg.color = new Color(0.05f, 0.95f, 0.35f, 1f); // Green default
                lifeBadges[i] = badge;
            }

            // --- Top Right: Score & Best ---
            GameObject scoreObj = CreateTMPText(hudPanel.transform, "ScoreText", "SCORE: 0", 38, TextAlignmentOptions.TopRight);
            var scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(1f, 1f);
            scoreRect.anchorMax = new Vector2(1f, 1f);
            scoreRect.pivot = new Vector2(1f, 1f);
            scoreRect.anchoredPosition = new Vector2(-40f, -35f);
            scoreRect.sizeDelta = new Vector2(400f, 55f);

            GameObject highScoreObj = CreateTMPText(hudPanel.transform, "HighScoreText", "BEST: 0", 26, TextAlignmentOptions.TopRight);
            var highScoreRect = highScoreObj.GetComponent<RectTransform>();
            highScoreRect.anchorMin = new Vector2(1f, 1f);
            highScoreRect.anchorMax = new Vector2(1f, 1f);
            highScoreRect.pivot = new Vector2(1f, 1f);
            highScoreRect.anchoredPosition = new Vector2(-40f, -95f);
            highScoreRect.sizeDelta = new Vector2(400f, 40f);

            // Connect HUDController
            var hudCtrl = hudPanel.AddComponent<HUDController>();
            SetSerializedField(hudCtrl, "speedText", speedObj.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hudCtrl, "fuelSlider", fuelSlider);
            SetSerializedField(hudCtrl, "fuelFillImage", fillImg);
            SetSerializedField(hudCtrl, "fuelPercentText", fuelPercentObj.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hudCtrl, "lifeIcons", lifeBadges);
            SetSerializedField(hudCtrl, "livesText", livesTitleObj.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hudCtrl, "scoreText", scoreObj.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hudCtrl, "highScoreText", highScoreObj.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hudCtrl, "playerHealth", player.GetComponent<PlayerHealth>());
            SetSerializedField(hudCtrl, "fuelSystem", player.GetComponent<FuelSystem>());

            // --- Pause Menu Panel & GameOver Panel on Canvas ---
            // Attaching UI scripts to Canvas guarantees they are ALWAYS active and listening!
            GameObject pauseObj = CreatePausePanel(canvasObj.transform);
            GameObject goPanel = CreateGameOverPanel(canvasObj.transform);

            var pauseUI = canvasObj.AddComponent<PauseMenuUI>();
            SetSerializedField(pauseUI, "pausePanel", pauseObj);
            SetSerializedField(pauseUI, "resumeButton", pauseObj.transform.Find("ResumeButton").GetComponent<Button>());
            SetSerializedField(pauseUI, "restartButton", pauseObj.transform.Find("RestartButton").GetComponent<Button>());
            SetSerializedField(pauseUI, "mainMenuButton", pauseObj.transform.Find("MainMenuButton").GetComponent<Button>());

            var gameOverUI = canvasObj.AddComponent<GameOverUI>();
            SetSerializedField(gameOverUI, "gameOverPanel", goPanel);
            SetSerializedField(gameOverUI, "currentScoreText", goPanel.transform.Find("FinalScoreText").GetComponent<TextMeshProUGUI>());
            SetSerializedField(gameOverUI, "highScoreText", goPanel.transform.Find("BestScoreText").GetComponent<TextMeshProUGUI>());
            SetSerializedField(gameOverUI, "newRecordBadge", goPanel.transform.Find("NewRecordBadge").gameObject);
            SetSerializedField(gameOverUI, "restartButton", goPanel.transform.Find("RestartButton").GetComponent<Button>());
            SetSerializedField(gameOverUI, "mainMenuButton", goPanel.transform.Find("MainMenuButton").GetComponent<Button>());

            pauseObj.SetActive(false);
            goPanel.SetActive(false);
        }

        private static GameObject CreatePausePanel(Transform canvasTransform)
        {
            GameObject pauseObj = new GameObject("PausePanel");
            pauseObj.transform.SetParent(canvasTransform, false);
            var pr = pauseObj.AddComponent<RectTransform>();
            pr.anchorMin = Vector2.zero;
            pr.anchorMax = Vector2.one;
            pr.sizeDelta = Vector2.zero;

            var bg = pauseObj.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.09f, 0.92f);

            GameObject title = CreateTMPText(pauseObj.transform, "PauseTitle", "GAME PAUSED", 56, TextAlignmentOptions.Center);
            var tr = title.GetComponent<RectTransform>();
            tr.anchoredPosition = new Vector2(0f, 150f);
            tr.sizeDelta = new Vector2(600f, 80f);

            CreateButton(pauseObj.transform, "ResumeButton", "RESUME [ESC / P]", new Vector2(0f, 30f), new Vector2(300f, 65f));
            CreateButton(pauseObj.transform, "RestartButton", "RESTART", new Vector2(0f, -50f), new Vector2(300f, 65f));
            CreateButton(pauseObj.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -130f), new Vector2(300f, 65f));

            return pauseObj;
        }

        private static GameObject CreateGameOverPanel(Transform canvasTransform)
        {
            GameObject goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(canvasTransform, false);
            var gr = goPanel.AddComponent<RectTransform>();
            gr.anchorMin = Vector2.zero;
            gr.anchorMax = Vector2.one;
            gr.sizeDelta = Vector2.zero;

            var bg = goPanel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.01f, 0.03f, 0.94f);

            GameObject title = CreateTMPText(goPanel.transform, "GameOverTitle", "GAME OVER", 68, TextAlignmentOptions.Center);
            var titleText = title.GetComponent<TextMeshProUGUI>();
            titleText.color = new Color(1f, 0.2f, 0.28f);
            var tr = title.GetComponent<RectTransform>();
            tr.anchoredPosition = new Vector2(0f, 190f);
            tr.sizeDelta = new Vector2(600f, 95f);

            GameObject finalScore = CreateTMPText(goPanel.transform, "FinalScoreText", "FINAL SCORE: 0", 40, TextAlignmentOptions.Center);
            var fsr = finalScore.GetComponent<RectTransform>();
            fsr.anchoredPosition = new Vector2(0f, 85f);
            fsr.sizeDelta = new Vector2(500f, 60f);

            GameObject bestScore = CreateTMPText(goPanel.transform, "BestScoreText", "BEST RECORD: 0", 30, TextAlignmentOptions.Center);
            var bsr = bestScore.GetComponent<RectTransform>();
            bsr.anchoredPosition = new Vector2(0f, 25f);
            bsr.sizeDelta = new Vector2(500f, 50f);

            GameObject newRecord = CreateTMPText(goPanel.transform, "NewRecordBadge", "★ NEW HIGH SCORE! ★", 28, TextAlignmentOptions.Center);
            newRecord.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.88f, 0.1f);
            var nrr = newRecord.GetComponent<RectTransform>();
            nrr.anchoredPosition = new Vector2(0f, -30f);
            nrr.sizeDelta = new Vector2(400f, 40f);

            CreateButton(goPanel.transform, "RestartButton", "PLAY AGAIN [SPACE / R]", new Vector2(0f, -100f), new Vector2(340f, 70f));
            CreateButton(goPanel.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -185f), new Vector2(340f, 65f));

            return goPanel;
        }

        private static void SetupMainMenuScene(PrefabsCollection prefabs)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            foreach (var r in roots) Object.DestroyImmediate(r);

            // Night Ambiance & Camera
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.08f, 0.18f);

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 2.0f, -5.5f);
            camObj.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.08f);
            camObj.AddComponent<AudioListener>();

            // Lights
            GameObject dirLightObj = new GameObject("MenuLight");
            var dl = dirLightObj.AddComponent<Light>();
            dl.type = LightType.Directional;
            dl.color = new Color(0.35f, 0.55f, 0.9f);
            dl.intensity = 0.6f;

            // Showcase Player Car in menu
            if (prefabs.playerCarPrefab != null)
            {
                GameObject car = PrefabUtility.InstantiatePrefab(prefabs.playerCarPrefab) as GameObject;
                car.name = "ShowcaseCar";
                car.transform.position = new Vector3(0f, 0.2f, 0f);
                car.transform.rotation = Quaternion.Euler(0f, -25f, 0f); // Sleek angle

                // Disable player controller in menu
                var pc = car.GetComponent<PlayerController>();
                if (pc != null) Object.DestroyImmediate(pc);
                var cc = car.GetComponent<CharacterController>();
                if (cc != null) Object.DestroyImmediate(cc);
            }

            // Road Podium underneath car
            GameObject roadPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadPlat.name = "ShowcaseRoadPlatform";
            roadPlat.transform.position = new Vector3(0f, -0.1f, 0f);
            roadPlat.transform.localScale = new Vector3(8f, 0.2f, 10f);
            roadPlat.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RoadMat.mat");

            // Event System with wired New Input System actions!
            GameObject eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<UnityEngine.EventSystems.EventSystem>();
            var inputModule = eventSys.AddComponent<InputSystemUIInputModule>();
            if (inputActionsAsset != null)
            {
                inputModule.actionsAsset = inputActionsAsset;
                inputModule.AssignDefaultActions();
            }

            // Canvas
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Title
            GameObject title = CreateTMPText(canvasObj.transform, "GameTitle", "NEON HIGHWAY", 88, TextAlignmentOptions.Center, true);
            var titleText = title.GetComponent<TextMeshProUGUI>();
            titleText.color = new Color(0f, 0.95f, 1f);
            var tr = title.GetComponent<RectTransform>();
            tr.anchoredPosition = new Vector2(0f, 260f);
            tr.sizeDelta = new Vector2(900f, 110f);

            GameObject subtitle = CreateTMPText(canvasObj.transform, "GameSubtitle", "HIGH-SPEED ENDLESS NIGHT DRIVE", 30, TextAlignmentOptions.Center);
            subtitle.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.35f, 0.85f);
            var str = subtitle.GetComponent<RectTransform>();
            str.anchoredPosition = new Vector2(0f, 185f);
            str.sizeDelta = new Vector2(700f, 50f);

            // High Score Badge
            int currentHigh = PlayerPrefs.GetInt("HW2_HighScore", 0);
            GameObject highScore = CreateTMPText(canvasObj.transform, "HighScoreText", $"★ BEST RECORD: {currentHigh:N0} ★", 28, TextAlignmentOptions.Center);
            highScore.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.9f, 0.2f);
            var hsr = highScore.GetComponent<RectTransform>();
            hsr.anchoredPosition = new Vector2(0f, 100f);
            hsr.sizeDelta = new Vector2(500f, 50f);

            // Play & Quit Buttons
            var playBtn = CreateButton(canvasObj.transform, "PlayButton", "START ENGINE [SPACE / CLICK]", new Vector2(0f, -140f), new Vector2(400f, 75f));
            var quitBtn = CreateButton(canvasObj.transform, "QuitButton", "EXIT GAME", new Vector2(0f, -230f), new Vector2(300f, 60f));

            var menuUI = canvasObj.AddComponent<MainMenuUI>();
            SetSerializedField(menuUI, "playButton", playBtn);
            SetSerializedField(menuUI, "quitButton", quitBtn);
            SetSerializedField(menuUI, "highScoreText", highScore.GetComponent<TextMeshProUGUI>());

            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, bool useTitleFont = false)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();

            if (useTitleFont && titleFont != null)
            {
                tmp.font = titleFont;
            }
            else if (primaryFont != null)
            {
                tmp.font = primaryFont;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return obj;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.06f, 0.16f, 0.36f, 0.95f);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.06f, 0.16f, 0.36f, 0.95f);
            colors.highlightedColor = new Color(0f, 0.8f, 1f, 1f);
            colors.pressedColor = new Color(0f, 0.5f, 0.9f, 1f);
            btn.colors = colors;

            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            GameObject textObj = CreateTMPText(btnObj.transform, "Label", label, 24, TextAlignmentOptions.Center);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private static void SetSerializedField(Component target, string fieldName, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is Object unityObj)
                {
                    prop.objectReferenceValue = unityObj;
                }
                else if (value is float f)
                {
                    prop.floatValue = f;
                }
                else if (value is int i)
                {
                    prop.intValue = i;
                }
                else if (value is bool b)
                {
                    prop.boolValue = b;
                }
                else if (value is System.Array arr)
                {
                    prop.arraySize = arr.Length;
                    for (int idx = 0; idx < arr.Length; idx++)
                    {
                        var elem = prop.GetArrayElementAtIndex(idx);
                        object val = arr.GetValue(idx);
                        if (val is Object elemObj) elem.objectReferenceValue = elemObj;
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
