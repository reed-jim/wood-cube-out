#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Spine.Unity;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SpineSpriteSheetExporter : EditorWindow
{
    // ===================== UI enums & config =====================
    public enum FrameResolutionPreset
    {
        AutoTightToBounds,  // fits the animation's max world bounds into the render area (keeps aspect)
        Square256,
        Square512,
        Square1024,
        Square2048,
        Custom              // use customWidth x customHeight
    }

    [Serializable]
    public class ExportSettings
    {
        public float fps = 20f;
        public string exportRoot = "Assets/SpriteSheets";
        public FrameResolutionPreset framePreset = FrameResolutionPreset.Square512;
        public int customWidth = 512;
        public int customHeight = 512;
        public int columns = 0;            // 0 = auto
        public int paddingPixels = 8;      // transparent padding around the max bounds inside each cell
        public bool includeLastFrame = true; // sample at t = duration - epsilon to avoid wrap
        public bool overwrite = true;
        public bool generateAnimationEnum = false;
        public string enumNamespace = "SpineExports";
        public string enumName = "SpineAnimations";
        public bool openFolderWhenDone = false;
    }

    // ===================== State =====================
    private GameObject targetGO;
    private SkeletonAnimation skeletonAnim;
    private string[] animationNames = Array.Empty<string>();
    private int selectedAnimIndex = 0;
    private ExportSettings settings = new ExportSettings();

    // cached temp camera/go
    private Camera captureCam;
    private GameObject captureRoot;   // holds a cloned skeleton for isolated rendering
    private GameObject clonedSkeleton;
    private int[] originalLayers;     // to restore on the clone (we'll force to one layer)
    private int tempLayer = 31;       // try using highest user layer; we restore after

    // prepass bounds
    private Bounds combinedWorldBounds;

    // progress
    private int totalFrames = 0;
    private int processed = 0;
    private bool busy = false;
    private string busyPhase = "";
    private Action step;

    [MenuItem("Tools/Spine Sprite Sheet Exporter (Rebuilt)")]
    public static void Open() => GetWindow<SpineSpriteSheetExporter>("Spine ➜ SpriteSheet");

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spine ➜ Sprite Sheet Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // target selection
        EditorGUI.BeginChangeCheck();
        targetGO = (GameObject)EditorGUILayout.ObjectField("Spine GameObject", targetGO, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck()) RefreshSkeletonData();

        using (new EditorGUI.DisabledScope(skeletonAnim == null))
        {
            if (skeletonAnim != null && animationNames.Length > 0)
                selectedAnimIndex = EditorGUILayout.Popup("Animation", selectedAnimIndex, animationNames);
        }

        // settings
        EditorGUILayout.Space();
        settings.fps = EditorGUILayout.FloatField("Frames Per Second", Mathf.Max(1f, settings.fps));
        settings.exportRoot = EditorGUILayout.TextField("Export Folder", settings.exportRoot);

        EditorGUILayout.Space();
        settings.framePreset = (FrameResolutionPreset)EditorGUILayout.EnumPopup("Frame Size Preset", settings.framePreset);
        if (settings.framePreset == FrameResolutionPreset.Custom)
        {
            settings.customWidth = Mathf.Max(8, EditorGUILayout.IntField("Custom Width (px)", settings.customWidth));
            settings.customHeight = Mathf.Max(8, EditorGUILayout.IntField("Custom Height (px)", settings.customHeight));
        }
        settings.paddingPixels = Mathf.Clamp(EditorGUILayout.IntField("Inner Padding (px)", settings.paddingPixels), 0, 256);
        settings.columns = Mathf.Max(0, EditorGUILayout.IntField("Columns (0 = auto)", settings.columns));

        EditorGUILayout.Space();
        settings.includeLastFrame = EditorGUILayout.Toggle(new GUIContent("Include Last Frame",
            "Adds a final sample at duration - epsilon so you actually get the visual last pose."), settings.includeLastFrame);
        settings.overwrite = EditorGUILayout.Toggle("Overwrite Existing", settings.overwrite);
        settings.openFolderWhenDone = EditorGUILayout.Toggle("Open Folder When Done", settings.openFolderWhenDone);

        EditorGUILayout.Space();
        settings.generateAnimationEnum = EditorGUILayout.Toggle("Generate Animations Enum (.cs)", settings.generateAnimationEnum);
        using (new EditorGUI.DisabledScope(!settings.generateAnimationEnum))
        {
            settings.enumNamespace = EditorGUILayout.TextField("Enum Namespace", settings.enumNamespace);
            settings.enumName = EditorGUILayout.TextField("Enum Name", settings.enumName);
        }

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(skeletonAnim == null || animationNames.Length == 0 || busy))
        {
            if (GUILayout.Button("Export Sprite Sheet"))
            {
                BeginExport();
            }
        }

        if (busy)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"{busyPhase}  •  {processed}/{totalFrames}", MessageType.Info);
        }
    }

    private void RefreshSkeletonData()
    {
        skeletonAnim = null;
        animationNames = Array.Empty<string>();
        selectedAnimIndex = 0;

        if (targetGO == null) return;

        skeletonAnim = targetGO.GetComponent<SkeletonAnimation>();
        if (skeletonAnim == null)
        {
            EditorGUILayout.HelpBox("Selected object must have a Spine SkeletonAnimation.", MessageType.Warning);
            return;
        }

        var data = skeletonAnim.SkeletonDataAsset?.GetSkeletonData(true);
        if (data == null)
        {
            EditorGUILayout.HelpBox("Could not read SkeletonDataAsset.", MessageType.Warning);
            return;
        }
        animationNames = data.Animations.Select(a => a.Name).ToArray();
        if (animationNames.Length == 0)
            EditorGUILayout.HelpBox("No animations found on this skeleton.", MessageType.Warning);
    }

    private void BeginExport()
    {
        if (skeletonAnim == null || animationNames.Length == 0) return;

        string animName = animationNames[Mathf.Clamp(selectedAnimIndex, 0, animationNames.Length - 1)];

        // ✅ Single folder per character (no per-animation directories)
        string characterDir = Path.Combine(settings.exportRoot, targetGO.name);
        Directory.CreateDirectory(characterDir);

        // temp frames live here (deleted after merge)
        string framesDir = Path.Combine(characterDir, "~tmp_" + animName);
        Directory.CreateDirectory(framesDir);

        // Optional: generate enum of animations once per character
        if (settings.generateAnimationEnum)
        {
            GenerateAnimationEnumFile(Path.Combine(settings.exportRoot, $"{targetGO.name}_Animations.cs"));
        }

        // Set up an isolated clone and camera (no Play Mode)
        SetupIsolatedStage();

        // timeline sampling
        var data = skeletonAnim.SkeletonDataAsset.GetSkeletonData(true);
        var anim = data.FindAnimation(animName);
        float duration = anim.Duration;
        int frameCount = Mathf.CeilToInt(duration * settings.fps);
        List<float> times = new List<float>(frameCount + 1);
        for (int i = 0; i < frameCount; i++) times.Add(i / settings.fps);
        if (settings.includeLastFrame && duration > 0f)
            times.Add(Mathf.Max(0f, duration - 1e-5f));

        // 1) Prepass: compute combined world bounds
        busy = true;
        busyPhase = "Prepass: computing animation bounds";
        processed = 0;
        totalFrames = times.Count;

        Bounds preBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        var clonedSkelAnim = clonedSkeleton.GetComponent<SkeletonAnimation>();
        var renderer = clonedSkeleton.GetComponent<Renderer>();

        for (int i = 0; i < times.Count; i++)
        {
            PoseAtTime(clonedSkelAnim, animName, times[i]);
            clonedSkelAnim.LateUpdate();
            if (renderer != null)
            {
                if (!hasBounds) { preBounds = renderer.bounds; hasBounds = true; }
                else preBounds.Encapsulate(renderer.bounds);
            }
            processed = i + 1;
            if (EditorUtility.DisplayCancelableProgressBar("Spine Sprite Export", $"{busyPhase} ({processed}/{totalFrames})", (float)processed / totalFrames))
            {
                Cleanup();
                EditorUtility.ClearProgressBar();
                busy = false;
                return;
            }
        }
        if (!hasBounds) preBounds = new Bounds(clonedSkeleton.transform.position, Vector3.one);
        combinedWorldBounds = preBounds;

        // 2) Camera once for the whole anim
        SetupCameraForBounds(combinedWorldBounds);

        // 3) Decide cell size in pixels
        Vector2Int cell = ResolveFramePixelSize();

        // 4) Render frames
        busyPhase = "Rendering frames";
        processed = 0;
        totalFrames = times.Count;

        var rt = new RenderTexture(cell.x, cell.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
        {
            antiAliasing = 1,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };

        Color clear = new Color(0, 0, 0, 0);

        for (int i = 0; i < times.Count; i++)
        {
            PoseAtTime(clonedSkelAnim, animName, times[i]);
            clonedSkelAnim.LateUpdate();

            captureCam.targetTexture = rt;
            RenderTexture.active = rt;
            GL.Clear(true, true, clear, 1.0f); // fully transparent clear
            captureCam.Render();
            RenderTexture.active = null;

            Texture2D tex = new Texture2D(cell.x, cell.y, TextureFormat.RGBA32, false, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, cell.x, cell.y), 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = null;

            string framePath = Path.Combine(framesDir, $"frame_{i:0000}.png");
            if (settings.overwrite || !File.Exists(framePath))
                File.WriteAllBytes(framePath, tex.EncodeToPNG());

            DestroyImmediate(tex);
            processed = i + 1;

            if (EditorUtility.DisplayCancelableProgressBar("Spine Sprite Export", $"{busyPhase} ({processed}/{totalFrames})", (float)processed / totalFrames))
            {
                Cleanup();
                EditorUtility.ClearProgressBar();
                busy = false;
                return;
            }
        }

        // 5) Build sheet directly into character folder
        EditorUtility.DisplayProgressBar("Spine Sprite Export", "Building sprite sheet", 0.95f);
        string[] frames = Directory.GetFiles(framesDir, "frame_*.png").OrderBy(p => p).ToArray();
        if (frames.Length == 0)
        {
            Cleanup();
            EditorUtility.ClearProgressBar();
            busy = false;
            Debug.LogWarning("No frames were written. Aborting spritesheet merge.");
            return;
        }

        int cols = settings.columns > 0 ? settings.columns : Mathf.CeilToInt(Mathf.Sqrt(frames.Length));
        cols = Mathf.Max(1, cols);
        int rows = Mathf.CeilToInt(frames.Length / (float)cols);

        int sheetW = cols * cell.x;
        int sheetH = rows * cell.y;

        Texture2D sheet = new Texture2D(sheetW, sheetH, TextureFormat.RGBA32, false, false);
        var blank = Enumerable.Repeat(new Color(0, 0, 0, 0), sheetW * sheetH).ToArray();
        sheet.SetPixels(blank);

        for (int i = 0; i < frames.Length; i++)
        {
            int cx = i % cols;
            int cy = i / cols;
            int x = cx * cell.x;
            int y = (rows - 1 - cy) * cell.y;

            byte[] bytes = File.ReadAllBytes(frames[i]);
            Texture2D ft = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            ft.LoadImage(bytes, false);
            sheet.SetPixels(x, y, cell.x, cell.y, ft.GetPixels());
            DestroyImmediate(ft);
        }
        sheet.Apply(false, false);

        // ✅ Final path: <exportRoot>/<Character>/<AnimationName>_sheet.png
        string outPng = Path.Combine(characterDir, $"{animName}_sheet.png");
        File.WriteAllBytes(outPng, sheet.EncodeToPNG());
        DestroyImmediate(sheet);

        // Cleanup temp frames
        try
        {
            Directory.Delete(framesDir, true);
            string meta = framesDir + ".meta";
            if (File.Exists(meta)) File.Delete(meta);
        }
        catch { /* ignore */ }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        Cleanup();
        busy = false;

        Debug.Log($"✅ Sprite sheet exported: {outPng}");
        if (settings.openFolderWhenDone) EditorUtility.RevealInFinder(outPng);
    }


    private void SetupIsolatedStage()
    {
        // Clone a hidden copy to avoid touching the original
        captureRoot = new GameObject("[SpineExportStage]");
        captureRoot.hideFlags = HideFlags.HideAndDontSave;

        clonedSkeleton = Instantiate(targetGO, captureRoot.transform);
        clonedSkeleton.name = targetGO.name + "_CloneForExport";
        clonedSkeleton.hideFlags = HideFlags.HideAndDontSave;

        // ensure single layer on clone so camera can isolate it
        originalLayers = SaveAndForceLayerRecursive(clonedSkeleton, tempLayer);

        // Reset any scaling/rotation drift on the clone root (keep local props on child as-is)
        clonedSkeleton.transform.position = Vector3.zero;
        // Keep rotation/scale from original in case rig depends on it:
        clonedSkeleton.transform.rotation = targetGO.transform.rotation;
        clonedSkeleton.transform.localScale = targetGO.transform.localScale;

        // Camera
        var camGO = new GameObject("CaptureCamera");
        camGO.transform.SetParent(captureRoot.transform, false);
        captureCam = camGO.AddComponent<Camera>();
        captureCam.orthographic = true;
        captureCam.clearFlags = CameraClearFlags.SolidColor;
        captureCam.backgroundColor = new Color(0, 0, 0, 0);
        captureCam.allowHDR = false;           // disable HDR to avoid bloom/glow bleeding into alpha
        captureCam.useOcclusionCulling = false;
        captureCam.depthTextureMode = DepthTextureMode.None;
        captureCam.cullingMask = 1 << tempLayer;   // only our clone

        // Try to disable URP post processing if present (reflection to avoid hard dep)
        var uacd = captureCam.GetComponent("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
        if (uacd != null)
        {
            var ppProp = uacd.GetType().GetProperty("renderPostProcessing");
            if (ppProp != null && ppProp.CanWrite) ppProp.SetValue(uacd, false);
            var clrProp = uacd.GetType().GetProperty("clearDepth");
            if (clrProp != null && clrProp.CanWrite) clrProp.SetValue(uacd, true);
        }
    }

    private void Cleanup()
    {
        if (captureCam != null) captureCam.targetTexture = null;
        if (clonedSkeleton != null && originalLayers != null) RestoreLayerRecursive(clonedSkeleton, originalLayers);
        if (captureRoot != null) DestroyImmediate(captureRoot);
        captureRoot = null;
        captureCam = null;
        clonedSkeleton = null;
        originalLayers = null;
    }

    private Vector2Int ResolveFramePixelSize()
    {
        switch (settings.framePreset)
        {
            case FrameResolutionPreset.AutoTightToBounds:
                {
                    // auto: longest side ~512px (similar to Square512 but keeps aspect)
                    float worldW = Mathf.Max(0.001f, combinedWorldBounds.size.x);
                    float worldH = Mathf.Max(0.001f, combinedWorldBounds.size.y);
                    int longPx = 512;
                    bool wider = worldW >= worldH;
                    float aspect = worldW / worldH;
                    int w = wider ? longPx : Mathf.RoundToInt(longPx * aspect);
                    int h = wider ? Mathf.RoundToInt(longPx / Mathf.Max(0.001f, aspect)) : longPx;
                    return new Vector2Int(Mathf.Max(8, w + 2 * settings.paddingPixels),
                                          Mathf.Max(8, h + 2 * settings.paddingPixels));
                }
            case FrameResolutionPreset.Square256: return new Vector2Int(256, 256);
            case FrameResolutionPreset.Square512: return new Vector2Int(512, 512);
            case FrameResolutionPreset.Square1024: return new Vector2Int(1024, 1024);
            case FrameResolutionPreset.Square2048: return new Vector2Int(2048, 2048);
            case FrameResolutionPreset.Custom: return new Vector2Int(settings.customWidth, settings.customHeight);
        }
        return new Vector2Int(512, 512);
    }

    private void SetupCameraForBounds(Bounds worldBounds)
    {
        // Pad in world space according to pixel padding converted later; simplest: add 5% margin
        float pad = 0.05f;
        float halfH = worldBounds.extents.y * (1f + pad);
        captureCam.orthographicSize = Mathf.Max(0.001f, halfH);

        // center camera on bounds center
        Vector3 center = worldBounds.center;
        captureCam.transform.position = new Vector3(center.x, center.y, center.z - 10f);
        captureCam.nearClipPlane = 0.01f;
        captureCam.farClipPlane = 100f;
    }

    private void PoseAtTime(SkeletonAnimation skelAnim, string animName, float time)
    {
        // Deterministic pose without accumulating delta
        var state = skelAnim.AnimationState;
        state.ClearTracks();
        state.SetAnimation(0, animName, false);
        var track = state.GetCurrent(0);
        if (track != null) track.TrackTime = time;
        state.Update(0f); // do not advance time, just apply current track time
        state.Apply(skelAnim.Skeleton);
        skelAnim.Skeleton.UpdateWorldTransform();
    }

    private int[] SaveAndForceLayerRecursive(GameObject go, int layer)
    {
        var trs = go.GetComponentsInChildren<Transform>(true);
        int[] layers = new int[trs.Length];
        for (int i = 0; i < trs.Length; i++)
        {
            layers[i] = trs[i].gameObject.layer;
            trs[i].gameObject.layer = layer;
        }
        return layers;
    }
    private void RestoreLayerRecursive(GameObject go, int[] layers)
    {
        var trs = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trs.Length && i < layers.Length; i++)
            trs[i].gameObject.layer = layers[i];
    }

    private void GenerateAnimationEnumFile(string path)
    {
        try
        {
            var data = skeletonAnim.SkeletonDataAsset.GetSkeletonData(true);
            var names = data.Animations.Select(a => a.Name).ToArray();

            string Sanitize(string raw)
            {
                // Letters/nums/_ only, and must not start with digit
                string s = new string(raw.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
                if (string.IsNullOrEmpty(s)) s = "Anim";
                if (char.IsDigit(s[0])) s = "_" + s;
                // collapse multiple underscores
                while (s.Contains("__")) s = s.Replace("__", "_");
                return s;
            }

            var uniq = new HashSet<string>();
            List<string> members = new List<string>();
            foreach (var n in names)
            {
                string m = Sanitize(n);
                int k = 1;
                string mk = m;
                while (!uniq.Add(mk)) { k++; mk = m + "_" + k; }
                members.Add($"        {mk}, // \"{n}\"");
            }

            string code =
$@"// Auto-generated by SpineSpriteSheetExporter on {DateTime.Now:yyyy-MM-dd HH:mm}
namespace {settings.enumNamespace}
{{
    public enum {settings.enumName}
    {{
{string.Join(Environment.NewLine, members)}
    }}
}}";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, code);
            AssetDatabase.Refresh();
            Debug.Log($"Generated enum file: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Enum generation failed: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        if (busy) EditorUtility.ClearProgressBar();
        Cleanup();
    }
}
#endif