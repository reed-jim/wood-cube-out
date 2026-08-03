using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MyGame.Runtime
{
    [RequireComponent(typeof(ParticleField))]
    public sealed class ParticleRenderer : MonoBehaviour
    {
        [SerializeField] private ParticleField field;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private int renderQueue = 3500;
        [SerializeField] private float depthBiasTowardCamera = 0.1f;
        [SerializeField] private bool logDiagnostics;

        [Header("Bóng khối trên hạt (bake vào atlas lúc Awake — không tốn gì lúc chạy)")]
        [SerializeField] private Vector2 lightDir = new Vector2(-0.55f, 0.75f);
        [Range(0.1f, 1f)][SerializeField] private float shadeMin = 0.45f;
        [Range(0.5f, 1f)][SerializeField] private float shadeBase = 0.88f;
        [Range(0f, 1f)][SerializeField] private float rimDarkness = 0.35f;

        [Header("Chấm bóng specular (đi trắng độc lập, không phụ thuộc màu palette)")]
        [Range(15f, 120f)][SerializeField] private float highlightSharpness = 55f;
        [Range(0f, 1f)][SerializeField] private float highlightWhiteness = 0.85f;

        [Header("Vết xước / sờn cũ")]
        [Range(0f, 1f)][SerializeField] private float wearIntensity = 0.55f;
        [SerializeField] private uint wearSeed = 12345;
        [SerializeField] private FilterMode atlasFilter = FilterMode.Bilinear;

        private Mesh quadMesh;
        private Material[] materialsByColor;
        private Texture2D grainAtlas;
        private List<Matrix4x4>[] buckets;
        private const int MaxInstancesPerCall = 1023;

        private readonly Matrix4x4[] instanceBuffer = new Matrix4x4[MaxInstancesPerCall];
        private int lastDrawnCount;


        private const float ShadeEncodeScale = 2f;

        private void Reset() => field = GetComponent<ParticleField>();

        private void Awake()
        {
            quadMesh = BuildQuadMesh();
            if (logDiagnostics)
                Debug.Log($"[ParticleRenderer] SystemInfo.supportsInstancing = {SystemInfo.supportsInstancing}, " +
                    $"GfxAPI = {SystemInfo.graphicsDeviceType}");
        }

        private void LateUpdate()
        {
            lastDrawnCount = 0;
            var palette = field.Palette;
            if (palette == null || palette.Length == 0)
            {
                if (logDiagnostics && Time.frameCount % 60 == 0)
                    Debug.Log("[ParticleRenderer] palette null/rỗng — chưa vẽ được gì (kiểm tra GameManager.SetPalette).");
                return;
            }
            EnsureMaterials(palette);

            int n = field.ActiveCount;
            for (int c = 0; c < buckets.Length; c++) buckets[c].Clear();
            if (n == 0 && field.FlyingCount == 0)
            {
                return;
            }

            var positions = field.Positions;
            var prevPositions = field.PrevPositions;
            var colors = field.ParticleColors;
            var variants = field.Variants;
            float radius = field.ParticleRadius;
            float diameter = radius * 2f;

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            float snapThresholdSqr = (radius * 8f) * (radius * 8f);

            for (int i = 0; i < n; i++)
            {
                int c = colors[i];
                if (c < 0 || c >= buckets.Length) continue;
                float2 cur = positions[i];
                float2 prev = prevPositions[i];
                float2 p = math.distancesq(cur, prev) > snapThresholdSqr ? cur : math.lerp(prev, cur, alpha);
                Vector3 world = transform.TransformPoint(new Vector3(p.x, p.y, -depthBiasTowardCamera));

                buckets[c].Add(MakeInstance(world, diameter, variants[i]));
            }

            int fn = field.FlyingCount;
            for (int i = 0; i < fn; i++)
            {
                int c = field.FlyingColor(i);
                if (c < 0 || c >= buckets.Length) continue;
                float2 p = field.FlyingPos(i);
                Vector3 world = transform.TransformPoint(new Vector3(p.x, p.y, -depthBiasTowardCamera));
                buckets[c].Add(MakeInstance(world, diameter, field.FlyingVariant(i)));
            }

            for (int c = 0; c < buckets.Length; c++)
            {
                var list = buckets[c];
                if (list.Count == 0) continue;

                for (int offset = 0; offset < list.Count; offset += MaxInstancesPerCall)
                {
                    int chunk = Mathf.Min(MaxInstancesPerCall, list.Count - offset);
                    list.CopyTo(offset, instanceBuffer, 0, chunk);
                    Graphics.DrawMeshInstanced(quadMesh, 0, materialsByColor[c], instanceBuffer, chunk,
                        null, UnityEngine.Rendering.ShadowCastingMode.Off, false, gameObject.layer);
                    lastDrawnCount += chunk;
                }
            }
        }


        private Matrix4x4 MakeInstance(Vector3 world, float diameter, byte variant)
        {
            var m = Matrix4x4.TRS(world, transform.rotation, Vector3.one * diameter);
            m.m22 = 1f + variant;
            return m;
        }

        private void OnDestroy()
        {
            DestroyGeneratedAssets();
            if (quadMesh != null) Destroy(quadMesh);
        }

        private void DestroyGeneratedAssets()
        {
            if (materialsByColor != null)
            {
                for (int i = 0; i < materialsByColor.Length; i++)
                    if (materialsByColor[i] != null) Destroy(materialsByColor[i]);
                materialsByColor = null;
            }
            if (grainAtlas != null)
            {
                Destroy(grainAtlas);
                grainAtlas = null;
            }
        }

        private void EnsureMaterials(Color32[] palette)
        {
            if (materialsByColor != null && materialsByColor.Length == palette.Length) return;

            Material template = baseMaterial;
            if (template == null)
            {
                var shader = Shader.Find("MyGame/ParticleInstancedUnlit");
                if (shader == null)
                {
                    return;
                }
                template = new Material(shader);
            }

            DestroyGeneratedAssets();
            grainAtlas = BuildGrainAtlas();
            var tex = grainAtlas;

            materialsByColor = new Material[palette.Length];
            buckets = new List<Matrix4x4>[palette.Length];
            for (int c = 0; c < palette.Length; c++)
            {
                var mat = new Material(template) { enableInstancing = true, renderQueue = renderQueue };
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_Color", palette[c]);
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.5f);
                if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                materialsByColor[c] = mat;
                buckets[c] = new List<Matrix4x4>();
            }

            // Template dựng tạm khi baseMaterial chưa gán — đã clone xong thì huỷ, đừng để rò rỉ.
            if (template != baseMaterial) Destroy(template);
        }

        private static Mesh BuildQuadMesh()
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f)
                },
                uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.RecalculateBounds();
            return mesh;
        }
        // Tạo hiệu ứng Renderr
        private Texture2D BuildGrainAtlas()
        {
            const int cell = 32;
            const int grid = ParticleField.GrainGrid;
            const int size = cell * grid;
            // Bán kính 15 trong ô 32 => chừa 1px gutter, tránh ô này rỉ sang ô kia khi lọc bilinear.
            const float radius = cell * 0.5f - 1f;

            // Gom shading dạng float trước, chuẩn hoá ở lượt sau (xem cuối hàm).
            var shade = new float[size * size];
            var highlight = new float[size * size];
            var opaque = new bool[size * size];
            double shadeSum = 0;
            int shadeCount = 0;

            Vector2 lit = lightDir.sqrMagnitude > 1e-6f ? lightDir.normalized : new Vector2(-0.55f, 0.75f);

            for (int v = 0; v < ParticleField.GrainVariantCount; v++)
            {
                var rng = new Unity.Mathematics.Random(wearSeed + (uint)v * 7919u + 1u);
                var wear = BuildWearMask(cell, radius, ref rng);

                // Hiệu ứng sờn cũ 
                float h1p = rng.NextFloat(0f, 6.2832f), h1a = rng.NextFloat(0.010f, 0.045f) * wearIntensity;
                float h2p = rng.NextFloat(0f, 6.2832f), h2a = rng.NextFloat(0.008f, 0.035f) * wearIntensity;
                float h3p = rng.NextFloat(0f, 6.2832f), h3a = rng.NextFloat(0.005f, 0.025f) * wearIntensity;
                int nicks = rng.NextInt(0, 3);
                var nickAng = new float[3];
                var nickW = new float[3];
                var nickD = new float[3];
                for (int k = 0; k < nicks; k++)
                {
                    nickAng[k] = rng.NextFloat(0f, 6.2832f);
                    nickW[k] = rng.NextFloat(0.25f, 0.6f);
                    nickD[k] = rng.NextFloat(0.06f, 0.16f) * wearIntensity;
                }

                int ox = (v % grid) * cell;
                int oy = (v / grid) * cell;

                for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                    {
                        // Toạ độ chuẩn hoá [-1..1] so với tâm ô.
                        float nx = (x + 0.5f - cell * 0.5f) / radius;
                        float ny = (y + 0.5f - cell * 0.5f) / radius;
                        float r2 = nx * nx + ny * ny;

                        int dst = (oy + y) * size + (ox + x);

                        float ang = Mathf.Atan2(ny, nx);
                        float edge = 1f - (h1a + h2a + h3a)
                            + h1a * Mathf.Sin(2f * ang + h1p)
                            + h2a * Mathf.Sin(3f * ang + h2p)
                            + h3a * Mathf.Sin(5f * ang + h3p);
                        for (int k = 0; k < nicks; k++)
                        {
                            float d = Mathf.Abs(AngleDelta(ang, nickAng[k]));
                            if (d < nickW[k]) { float t = 1f - d / nickW[k]; edge -= nickD[k] * t * t; }
                        }

                        if (r2 > edge * edge) { opaque[dst] = false; continue; }

                        float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - r2));
                        float ndotl = Mathf.Clamp01(nx * lit.x + ny * lit.y + nz * 0.85f);

                        float s = Mathf.Lerp(shadeMin, shadeBase, ndotl);

                        float rim = Mathf.SmoothStep(0.5f, 1f, Mathf.Sqrt(r2));
                        s *= Mathf.Lerp(1f, 1f - rimDarkness, rim);

                        s *= wear[y * cell + x];

                        float hx = nx - lit.x * 0.5f;
                        float hy = ny - lit.y * 0.5f;
                        highlight[dst] = Mathf.Exp(-(hx * hx + hy * hy) * highlightSharpness) * highlightWhiteness;

                        shade[dst] = s;
                        opaque[dst] = true;
                        shadeSum += s;
                        shadeCount++;
                    }
            }

            // Chuẩn hoá để độ sáng TRUNG BÌNH trên toàn atlas đúng bằng 1.0, tức hạt trung bình ra
            // đúng màu palette chứ không tối đi. Chuẩn hoá trên cả atlas (không phải từng ô) để
            // chênh lệch tông giữa các biến thể vẫn còn.
            float mean = shadeCount > 0 ? (float)(shadeSum / shadeCount) : 1f;
            float norm = mean > 1e-4f ? 1f / mean : 1f;

            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!opaque[i]) { pixels[i] = new Color32(0, 0, 0, 0); continue; }
                // Chia cho ShadeEncodeScale để vùng SÁNG HƠN màu chuẩn (hệ số > 1) vẫn lưu được
                // trong RGBA32 vốn chỉ tới 1.0; shader nhân lại đúng hằng này.
                float enc = shade[i] * norm / ShadeEncodeScale;
                byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(enc * 255f), 0, 255);
                byte hl = (byte)Mathf.Clamp(Mathf.RoundToInt(highlight[i] * 255f), 0, 255);
                pixels[i] = new Color32(r, hl, r, 255);
            }

            // linear: true là BẮT BUỘC. Project chạy Linear color space; nếu để mặc định (sRGB)
            // thì Unity coi atlas là màu và giải mã gamma khi sample — hệ số 0.88 biến thành ~0.75
            // và hạt tối đi rõ rệt so với màu palette. Atlas này là dữ liệu, không phải màu.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "GrainAtlas",
                filterMode = atlasFilter,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        /// <summary>Chênh lệch góc (radian) đưa về khoảng [-PI, PI].</summary>
        private static float AngleDelta(float a, float b)
        {
            float d = a - b;
            while (d > Mathf.PI) d -= 2f * Mathf.PI;
            while (d < -Mathf.PI) d += 2f * Mathf.PI;
            return d;
        }

        /// <summary>
        /// Lớp sờn/xước của một ô, trả về hệ số nhân độ sáng [0..~1.1] cho từng pixel:
        /// mottling tần số thấp (sờn màu không đều) + vệt xước thẳng + lấm chấm rỗ.
        /// </summary>
        private float[] BuildWearMask(int cell, float radius, ref Unity.Mathematics.Random rng)
        {
            var mask = new float[cell * cell];
            float w = wearIntensity;

            // Mottling: vài "vết loang" tròn mềm làm sờn màu không đều.
            int blobs = 3 + rng.NextInt(0, 3);
            var blobX = new float[blobs];
            var blobY = new float[blobs];
            var blobR = new float[blobs];
            var blobA = new float[blobs];
            for (int b = 0; b < blobs; b++)
            {
                blobX[b] = rng.NextFloat(-0.8f, 0.8f);
                blobY[b] = rng.NextFloat(-0.8f, 0.8f);
                blobR[b] = rng.NextFloat(0.25f, 0.6f);
                blobA[b] = rng.NextFloat(-0.16f, 0.10f) * w;
            }

            for (int y = 0; y < cell; y++)
                for (int x = 0; x < cell; x++)
                {
                    float nx = (x + 0.5f - cell * 0.5f) / radius;
                    float ny = (y + 0.5f - cell * 0.5f) / radius;
                    float m = 1f;
                    for (int b = 0; b < blobs; b++)
                    {
                        float dx = nx - blobX[b];
                        float dy = ny - blobY[b];
                        float t = Mathf.Clamp01(1f - (dx * dx + dy * dy) / (blobR[b] * blobR[b]));
                        m += blobA[b] * t * t;
                    }
                    mask[y * cell + x] = m;
                }

            // Vệt xước thẳng: 2-4 vệt mảnh, ngẫu nhiên góc/vị trí, có vệt tối có vệt sáng.
            int scratches = 2 + rng.NextInt(0, 3);
            for (int s = 0; s < scratches; s++)
            {
                float ang = rng.NextFloat(0f, Mathf.PI);
                float dirX = Mathf.Cos(ang);
                float dirY = Mathf.Sin(ang);
                float offset = rng.NextFloat(-0.7f, 0.7f);
                float halfLen = rng.NextFloat(0.4f, 1.0f);
                float centerT = rng.NextFloat(-0.35f, 0.35f);
                // Vệt phải đủ dày và đủ đậm để sống sót khi hạt bị thu về ~19px trên màn;
                // vệt mảnh 1px trông rất đẹp trong atlas nhưng biến mất sạch lúc chạy thật.
                float width = rng.NextFloat(0.13f, 0.30f);
                float amp = rng.NextFloat(-0.55f, 0.30f) * w;

                for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                    {
                        float nx = (x + 0.5f - cell * 0.5f) / radius;
                        float ny = (y + 0.5f - cell * 0.5f) / radius;
                        // Khoảng cách vuông góc tới đường, và vị trí dọc theo đường.
                        float perp = -nx * dirY + ny * dirX - offset;
                        float along = nx * dirX + ny * dirY - centerT;
                        if (Mathf.Abs(along) > halfLen) continue;

                        float across = Mathf.Clamp01(1f - Mathf.Abs(perp) / width);
                        if (across <= 0f) continue;
                        // Nhạt dần về hai đầu vệt để không bị cắt cụt thô.
                        float fade = Mathf.Clamp01(1f - Mathf.Abs(along) / halfLen);
                        mask[y * cell + x] += amp * across * fade;
                    }
            }

            // Rỗ: chấm 2x2 chứ không phải 1 pixel lẻ, cùng lý do với độ dày vệt xước ở trên.
            int specks = Mathf.RoundToInt(rng.NextFloat(4f, 9f) * w);
            for (int s = 0; s < specks; s++)
            {
                int sx = rng.NextInt(3, cell - 4);
                int sy = rng.NextInt(3, cell - 4);
                float amp = rng.NextFloat(-0.5f, 0.3f) * w;
                for (int dy = 0; dy < 2; dy++)
                    for (int dx = 0; dx < 2; dx++)
                        mask[(sy + dy) * cell + (sx + dx)] += amp;
            }

            // Lệch tông tổng thể giữa các biến thể — hạt này xỉn hơn hạt kia. Đây là tín hiệu
            // "cũ" đọc rõ nhất ở kích thước thật, vì nó không phụ thuộc chi tiết nhỏ.
            float tone = 1f + rng.NextFloat(-0.13f, 0.07f) * w;
            for (int i = 0; i < mask.Length; i++) mask[i] *= tone;

            return mask;
        }
    }
}
