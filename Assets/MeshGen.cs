using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	List<Vector3> vertices = new List<Vector3>();
	int[,] vertMap;
	List<int> triangles = new List<int>();

	public void GenerateMesh(bool[,] map, int width, int height) {
		vertMap = new int[width+2, height+2];

		// Convert each wall tile to a point; add border points around the tilemap
		for (int x = -1; x < width + 1; x++) {
			for (int y = -1; y < height + 1; y++) {
				if (x >= 0 && y >= 0 && x < width && y < height && !map[x, y]) {
					vertMap[x+1, y+1] = -1;					// Space tiles = invalid vert index
				} else {
					vertices.Add(new Vector3(x + 0.5f - width/2, 0, y + 0.5f - height/2));
					vertMap[x+1, y+1] = vertices.Count - 1;	// Store vert index in the map at its coordinates
				}
			}
		}

		// Scan through every 4-vert square in the vert grid, draw mesh triangles accordingly
		for (int x = 0; x < width + 1; x++) {
			for (int y = 0; y < height + 1; y++) {
				int tl = vertMap[x, y];
				int tr = vertMap[x+1, y];
				int bl = vertMap[x, y+1];
				int br = vertMap[x+1, y+1];
				if (tl != -1 && tr != -1 && bl != -1 && br != -1) {
					AddTriangle(tl, bl, br);
					AddTriangle(tl, br, tr);
				} else if (tr != -1 && bl != -1 && br != -1) {
					AddTriangle(tr, bl, br);
				} else if (tl != -1 && bl != -1 && br != -1) {
					AddTriangle(tl, bl, br);
				} else if (tl != -1 && tr != -1 && br != -1) {
					AddTriangle(tl, br, tr);
				} else if (tl != -1 && tr != -1 && bl != -1) {
					AddTriangle(tl, bl, tr);
				}
			}
		}
	
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
	}

	void AddTriangle(int v1, int v2, int v3) {
		triangles.Add(v1);
		triangles.Add(v2);
		triangles.Add(v3);
	}
}
