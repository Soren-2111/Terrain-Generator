using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedPlaneMesh : MonoBehaviour
{
    // �̃T�C�Y
    public float width = 25f;
    public float height = 25f;

    // �̋Ȃ���
    public float curveAmount = 5f;

    // ���_�̕�����
    public int segments = 10;

    // �p�[�����m�C�Y�֘A�̃p�����[�^
    public float noiseScale = 0.75f; // �m�C�Y�̃X�P�[��
    public float noiseStrength = 4f; // �m�C�Y�̋��x

    [ContextMenu("����")]

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        // ���_�ƎO�p�`���X�g��������
        Vector3[] vertices = new Vector3[(segments + 1) * (segments + 1)];
        int[] triangles = new int[segments * segments * 6];

        // UV���W���X�g��������
        Vector2[] uv = new Vector2[vertices.Length];

        int vertexIndex = 0;
        int triangleIndex = 0;

        // ���_��z�u
        for (int i = 0; i <= segments; i++)
        {
            for (int j = 0; j <= segments; j++)
            {
                // X���W��Z���W���v�Z
                float y = (i / (float)segments) * width;//���Ƃ���x������
                //float z = (j / (float)segments) * height;
                float x = (j / (float)segments) * height;//�t���������s

                // �Ȃ����Y���ɓK�p�i�V���v���ȓ񎟋Ȑ��j
                //float y = Mathf.Sin((i / (float)segments) * Mathf.PI) * curveAmount;
                float z = Mathf.Sin((i / (float)segments) * Mathf.PI) * curveAmount;//�t���������s

                // �p�[�����m�C�Y��K�p����Y���Ƀ����_���ȋN����ǉ�
                float noise = Mathf.PerlinNoise(i * noiseScale, j * noiseScale) * noiseStrength;
                //y += noise;
                z += noise;//�t���������s

                // ���_��z�u
                vertices[vertexIndex] = new Vector3(x, y, z);
                uv[vertexIndex] = new Vector2(i / (float)segments, j / (float)segments);

                if (i < segments && j < segments)
                {
                    // �O�p�`1��
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + segments + 1;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    // �O�p�`2��
                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + segments + 1;
                    triangles[triangleIndex + 5] = vertexIndex + segments + 2;

                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        // ���b�V���ɒ��_�A�O�p�`�AUV��ݒ�
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        // ���b�V�����R���|�[�l���g�ɓK�p
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
