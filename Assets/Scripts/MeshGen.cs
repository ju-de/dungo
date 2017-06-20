using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	GameObject wallsObject, ceilingObject;

	int?[,] ceilingVertMap, wallVertMap;
	List<Vector3> ceilingVertices, wallVertices;
	List<int> ceilingTriangles, wallTriangles;

	public void GenerateMesh(bool[,] map, int width, int height) {
		wallVertMap = new int?[width+2, height+2];
		wallVertices = new List<Vector3>();
		wallTriangles = new List<int>();

		ceilingVertMap = new int?[width+2, height+2];
		ceilingVertices = new List<Vector3>();
		ceilingTriangles = new List<int>();

		GenerateVertices(map, width, height);
		GenerateTriangles(map, width, height);

		Mesh ceilingMesh = new Mesh();
		ceilingMesh.vertices = ceilingVertices.ToArray();
		ceilingMesh.triangles = ceilingTriangles.ToArray();
		ceilingMesh.RecalculateNormals();
		ceilingObject = this.transform.Find("Ceiling").gameObject;
		ceilingObject.GetComponent<MeshFilter>().mesh = ceilingMesh;

		Mesh wallMesh = new Mesh();
		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		wallMesh.RecalculateNormals();
		wallsObject = this.transform.Find("Walls").gameObject;
		wallsObject.GetComponent<MeshFilter>().mesh = wallMesh;
		wallsObject.GetComponent<MeshCollider>().sharedMesh = wallMesh;

	}

	void GenerateVertices(bool[,] map, int width, int height) {
		// Convert each wall tile to a point; add border points around the tilemap
		for (int x = -1; x < width + 1; x++) {
			for (int y = -1; y < height + 1; y++) {
				// Border tiles
				if (x < 0 || y < 0 || x >= width || y >= height) {
					AddCeilingVertex(x, y);
					continue;
				}
				if (map[x, y]) {
					AddCeilingVertex(x, y);
					if (MapUtils.IsEdgeTile(x, y, width, height, map)) {
						AddWallVertices(x, y);
					}
				}
			}
		}
	}

	void AddCeilingVertex(int x, int y) {
		ceilingVertices.Add(new Vector3(x, 2, y));
		ceilingVertMap[x+1, y+1] = ceilingVertices.Count - 1;
	}

	void AddWallVertices(int x, int y) {
		wallVertices.Add(new Vector3(x, 2, y));
		wallVertices.Add(new Vector3(x, 0, y));
		wallVertMap[x+1, y+1] = wallVertices.Count - 1;	// stored index is for floor; -1 to get ceiling
	}

	void GenerateTriangles(bool[,] map, int width, int height) {
		// Scan through every 4-vert square in the vert grid, draw mesh triangles accordingly
		for (int x = 0; x < width + 1; x++) {
			for (int y = 0; y < height + 1; y++) {
				int? ctl = ceilingVertMap[x, y];
				int? ctr = ceilingVertMap[x+1, y];
				int? cbl = ceilingVertMap[x, y+1];
				int? cbr = ceilingVertMap[x+1, y+1];
				int? wtl = wallVertMap[x, y];
				int? wtr = wallVertMap[x+1, y];
				int? wbl = wallVertMap[x, y+1];
				int? wbr = wallVertMap[x+1, y+1];
				if (ctl.HasValue && ctr.HasValue && cbl.HasValue && cbr.HasValue) {	// full ceiling
					AddRectangle(false, ctl.Value, cbl.Value, cbr.Value, ctr.Value);
				} else if (ctr.HasValue && cbl.HasValue && cbr.HasValue) {	// tr <-> bl diagonal
					AddTriangle(false, ctr.Value, cbl.Value, cbr.Value);
					AddRectangle(true, wbl.Value-1, wtr.Value-1, wtr.Value, wbl.Value);
				} else if (ctl.HasValue && cbl.HasValue && cbr.HasValue) {	// tl <-> br diagonal
					AddTriangle(false, ctl.Value, cbl.Value, cbr.Value);
					AddRectangle(true, wtl.Value-1, wbr.Value-1, wbr.Value, wtl.Value);
				} else if (ctl.HasValue && ctr.HasValue && cbr.HasValue) {	// tl <-> br diagonal
					AddTriangle(false, ctl.Value, cbr.Value, ctr.Value);
					AddRectangle(true, wtl.Value, wbr.Value, wbr.Value-1, wtl.Value-1);
				} else if (ctl.HasValue && ctr.HasValue && cbl.HasValue) {	// tr <-> bl diagonal
					AddTriangle(false, ctl.Value, cbl.Value, ctr.Value);
					AddRectangle(true, wbl.Value, wtr.Value, wtr.Value-1, wbl.Value-1);
				} else {
					if (ctl.HasValue && ctr.HasValue) {
						AddRectangle(true, wtl.Value-1, wtl.Value, wtr.Value, wtr.Value-1);
					} else if (ctr.HasValue && cbr.HasValue) {
						AddRectangle(true, wtr.Value-1, wtr.Value, wbr.Value, wbr.Value-1);
					} else if (cbr.HasValue && cbl.HasValue) {
						AddRectangle(true, wbr.Value-1, wbr.Value, wbl.Value, wbl.Value-1);
					} else if (cbl.HasValue && ctl.HasValue ) {
						AddRectangle(true, wbl.Value-1, wbl.Value, wtl.Value, wtl.Value-1);
					}
				}
			}
		}
	}

	void AddRectangle(bool isWall, int v1, int v2, int v3, int v4) {
		AddTriangle(isWall, v1, v2, v3);
		AddTriangle(isWall, v1, v3, v4);
	}

	void AddTriangle(bool isWall, int v1, int v2, int v3) {
		if (isWall) {
			wallTriangles.Add(v1);
			wallTriangles.Add(v2);
			wallTriangles.Add(v3);
		} else {
			ceilingTriangles.Add(v1);
			ceilingTriangles.Add(v2);
			ceilingTriangles.Add(v3);
		}
	}
}
