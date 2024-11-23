using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedPlaneMesh : MonoBehaviour
{
    // 板のサイズ
    public float width = 25f;
    public float height = 25f;

    // 板の曲がり具合
    public float curveAmount = 5f;

    // 頂点の分割数
    public int segments = 10;

    // パーリンノイズ関連のパラメータ
    public float noiseScale = 0.75f; // ノイズのスケール
    public float noiseStrength = 4f; // ノイズの強度

    [ContextMenu("生成")]

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        // 頂点と三角形リストを初期化
        Vector3[] vertices = new Vector3[(segments + 1) * (segments + 1)];
        int[] triangles = new int[segments * segments * 6];

        // UV座標リストを初期化
        Vector2[] uv = new Vector2[vertices.Length];

        int vertexIndex = 0;
        int triangleIndex = 0;

        // 頂点を配置
        for (int i = 0; i <= segments; i++)
        {
            for (int j = 0; j <= segments; j++)
            {
                // X座標とZ座標を計算
                float y = (i / (float)segments) * width;//もともとxだった
                //float z = (j / (float)segments) * height;
                float x = (j / (float)segments) * height;//付け足した行

                // 曲がりをY軸に適用（シンプルな二次曲線）
                //float y = Mathf.Sin((i / (float)segments) * Mathf.PI) * curveAmount;
                float z = Mathf.Sin((i / (float)segments) * Mathf.PI) * curveAmount;//付け足した行

                // パーリンノイズを適用してY軸にランダムな起伏を追加
                float noise = Mathf.PerlinNoise(i * noiseScale, j * noiseScale) * noiseStrength;
                //y += noise;
                z += noise;//付け足した行

                // 頂点を配置
                vertices[vertexIndex] = new Vector3(x, y, z);
                uv[vertexIndex] = new Vector2(i / (float)segments, j / (float)segments);

                if (i < segments && j < segments)
                {
                    // 三角形1つ目
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + segments + 1;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    // 三角形2つ目
                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + segments + 1;
                    triangles[triangleIndex + 5] = vertexIndex + segments + 2;

                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        // メッシュに頂点、三角形、UVを設定
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        // メッシュをコンポーネントに適用
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
