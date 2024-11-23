using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PyramidGenerator_2 : MonoBehaviour
{
    public int numberOfPoints = 8;  // ��ʂ̒��_�̐�
    public float radius = 3f;        // ��ʂ̔��a
    public float height = 12f;       // �s���~�b�h�̍���

    public bool isf;

    public Vector3[] basePoints;

    float centerX, centerZ;

    [ContextMenu("生成")]
    void Start()
    {
        radius = Random.Range(1.5f, 3);
        if (isf == false)
        {
            height = -height;
        }
        StartPyla();
        GenerateRandomPoints();
        ArrangePointsInCircle();
        CreatePyramidMesh();
    }

    public void StartPyla()
    {
        centerX = Random.Range(-10, 10);
        centerZ = Random.Range(-10, 10);
    }
    public void GetbasePoints(Vector3[] array)
    {
        basePoints=array;
    }

    #region 
        // �����_���ȓ_�𐶐�
    void GenerateRandomPoints()
    {
        basePoints = new Vector3[numberOfPoints];
        //全体的にfloat yを定義してその分全体を下げたり上げたり
        for (int i = 0; i < numberOfPoints; i++)
        {
            float x = Random.Range(-radius, radius);
            float z = Random.Range(-radius, radius);
            x += centerX;
            z += centerZ;
            //x,zに中心点分の座標をずらす(更にランダムで値足してもいい)
            basePoints[i] = new Vector3(x, 0, z);
        }
    }

    // �_���~����ɕ��בւ�
    void ArrangePointsInCircle()
    {
        Vector3 centroid = Vector3.zero;
        foreach (var point in basePoints)
        {
            centroid += point;
        }
        centroid /= numberOfPoints;

        System.Array.Sort(basePoints, (a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - centroid.y, a.x - centroid.x);
            float angleB = Mathf.Atan2(b.y - centroid.y, b.x - centroid.x);
            return angleA.CompareTo(angleB);
        });
    }
    #endregion
    

    // ���b�V���𐶐�
    void CreatePyramidMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        //Vector3 topVertex = new Vector3(0, 0, height); // �s���~�b�h�̒��_
        //���_��xz���W��ݒ�
        Vector3 topVertex = new Vector3(centerX, height, centerZ);
        int verticesCount = numberOfPoints + 1; // ��ʂ̒��_��+���_

        Vector3[] vertices = new Vector3[verticesCount];
        //int[] triangles = new int[numberOfPoints * 3 * 2]; // ��ʂƑ��ʂ̎O�p�`�̐�
        int[] triangles = new int[numberOfPoints * 3 ];

        // ���_�ݒ�
        for (int i = 0; i < numberOfPoints; i++)
        {
            vertices[i] = basePoints[i];
        }
        vertices[numberOfPoints] = topVertex;
        /*
        // ��ʂ̎O�p�`�ݒ�
        for (int i = 0; i < numberOfPoints; i++)
        {
            int nextIndex = (i + 1) % numberOfPoints;
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = nextIndex;
            triangles[i * 3 + 2] = numberOfPoints;
        }
        */

        // ���ʂ̎O�p�`�ݒ�
        /*
        for (int i = 0; i < numberOfPoints; i++)
        {
            int nextIndex = (i + 1) % numberOfPoints;
            triangles[numberOfPoints * 3 + i * 3] = i;
            triangles[numberOfPoints * 3 + i * 3 + 1] = nextIndex;
            triangles[numberOfPoints * 3 + i * 3 + 2] = numberOfPoints;
        }
        */
        for (int i = 0; i < numberOfPoints; i++)
        {
            int nextIndex = (i + 1) % numberOfPoints;
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = nextIndex;
            triangles[i * 3 + 2] = numberOfPoints;
        }

        // ���b�V���ݒ�
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    // �f�o�b�O�p�ɓ_��`��
    void OnDrawGizmos()
    {
        if (basePoints == null) return;
        /*
        if (isf == true)
        {
            Gizmos.color = Color.blue;
            foreach (var point in basePoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(0, height, 0), 0.1f);
        }
        else
        {
            Gizmos.color = Color.blue;
            foreach (var point in basePoints)
            {
                Gizmos.DrawSphere(point, 0.1f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(0, -height, 0), 0.1f);
        }
        */
        
        Gizmos.color = Color.blue;
        foreach (var point in basePoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(0, height, 0), 0.1f);
    }
}

