using UnityEngine;

public class UVPaletteBaker : MonoBehaviour
{
    [SerializeField] private Vector2Int expectedUV;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;

        var scale = transform.localScale;

        int random = transform.GetSiblingIndex();

        for (int i = 0; i < uvs.Length; i++)
        {
            int range = (int)((float)i / uvs.Length * 21f);

            range = 0;

            uvs[i] = new Vector2((expectedUV.x + range) / 129f, expectedUV.y / 129f);
        }

        mesh.uv = uvs;
    }
}
