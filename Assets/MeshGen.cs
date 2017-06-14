using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	List<Vector3> vertices = new List<Vector3>();
	int[,] vertMap;
	List<int> triangles = new List<int>();

	public void GenerateMesh(bool[,] map, int width, int height) {
		vertMap = new int[width+2, height+2];

		for (int x = 0; x < width + 2; x++) {
			for (int y = 0; y < height + 2; y++) {
				vertices.Add(new Vector3(x - 0.5f - width/2, 0, y + 0.5f - height/2));
				vertMap[x, y] = vertices.Count - 1;
			}
		}

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (map[x, y]) {
					triangles.Add(vertMap[x, y]);
					triangles.Add(vertMap[x, y+1]);
					triangles.Add(vertMap[x+1, y+1]);
					triangles.Add(vertMap[x, y]);
					triangles.Add(vertMap[x+1, y+1]);
					triangles.Add(vertMap[x+1, y]);
				}
			}
		}
	
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
	}
}
