using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	public int wallHeight = 2;

	GameObject wallsObject, ceilingObject;

	int?[,] ceilingVertMap;
	WallVert[,] wallVertMap;
	List<Vector3> ceilingVertices, wallVertices;
	List<int> ceilingTriangles, wallTriangles;

	public void GenerateMesh(bool[,] map, int width, int height) {
		wallVertMap = new WallVert[width+2, height+2];
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
		ceilingVertices.Add(new Vector3(x, wallHeight, y));
		ceilingVertMap[x+1, y+1] = ceilingVertices.Count - 1;
	}

	void AddWallVertices(int x, int y) {
		wallVertices.Add(new Vector3(x, wallHeight, y));
		wallVertices.Add(new Vector3(x, 0, y));
		wallVertices.Add(new Vector3(x, wallHeight, y));
		wallVertices.Add(new Vector3(x, 0, y));
		wallVertMap[x+1, y+1] = new WallVert(wallVertices.Count-4, wallVertices.Count-3, wallVertices.Count-2, wallVertices.Count-1);
	}

	void GenerateTriangles(bool[,] map, int width, int height) {
		// Scan through every 4-vert square in the vert grid, draw mesh triangles accordingly
		for (int x = 0; x < width + 1; x++) {
			for (int y = 0; y < height + 1; y++) {
				int? cbl = ceilingVertMap[x, y];
				int? cbr = ceilingVertMap[x+1, y];
				int? ctl = ceilingVertMap[x, y+1];
				int? ctr = ceilingVertMap[x+1, y+1];
				WallVert wbl = wallVertMap[x, y];
				WallVert wbr = wallVertMap[x+1, y];
				WallVert wtl = wallVertMap[x, y+1];
				WallVert wtr = wallVertMap[x+1, y+1];
				if (cbl.HasValue && cbr.HasValue && ctl.HasValue && ctr.HasValue) {	// full ceiling
					AddRectangle(false, cbl.Value, ctl.Value, ctr.Value, cbr.Value);
				} else if (cbr.HasValue && ctl.HasValue && ctr.HasValue) {	// tr <-> bl diagonal facing down
					AddTriangle(false, cbr.Value, ctl.Value, ctr.Value);
					AddRectangle(true, wtl.GetCeilVert(), wbr.GetCeilVert(), wbr.GetFloorVert(), wtl.GetFloorVert());
					wtl.SetUsed();
					wbr.SetUsed();
				} else if (cbl.HasValue && ctl.HasValue && ctr.HasValue) {	// tl <-> br diagonal facing down
					AddTriangle(false, cbl.Value, ctl.Value, ctr.Value);
					AddRectangle(true, wbl.GetCeilVert(), wtr.GetCeilVert(), wtr.GetFloorVert(), wbl.GetFloorVert());
					wbl.SetUsed();
					wtr.SetUsed();
				} else if (cbl.HasValue && cbr.HasValue && ctr.HasValue) {	// tl <-> br diagonal facing up
					AddTriangle(false, cbl.Value, ctr.Value, cbr.Value);
					AddRectangle(true, wbl.GetFloorVert(), wtr.GetFloorVert(), wtr.GetCeilVert(), wbl.GetCeilVert());
					wbl.SetUsed();
					wtr.SetUsed();
				} else if (cbl.HasValue && cbr.HasValue && ctl.HasValue) {	// tr <-> bl diagonal facing up
					AddTriangle(false, cbl.Value, ctl.Value, cbr.Value);
					AddRectangle(true, wtl.GetFloorVert(), wbr.GetFloorVert(), wbr.GetCeilVert(), wtl.GetCeilVert());
					wtl.SetUsed();
					wbr.SetUsed();
				} else {
					if (cbl.HasValue && cbr.HasValue) {						// bottom
						int tl = wbl.GetCeilVert(), bl = wbl.GetFloorVert(), br = wbr.GetFloorVert(), tr = wbr.GetCeilVert();
						AddRectangle(true, tl, bl, br, tr);
						wbl.SetUsed();
						wbr.SetUsed();
					} else if (cbr.HasValue && ctr.HasValue) {				// right
						int tl = wbr.GetCeilVert(), bl = wbr.GetFloorVert(), br = wtr.GetFloorVert(), tr = wtr.GetCeilVert();
						AddRectangle(true, tl, bl, br, tr);
						wbr.SetUsed();
						wtr.SetUsed();
					} else if (ctr.HasValue && ctl.HasValue) {				// top
						int tl = wtr.GetCeilVert(), bl = wtr.GetFloorVert(), br = wtl.GetFloorVert(), tr = wtl.GetCeilVert();
						AddRectangle(true, tl, bl, br, tr);
						wtr.SetUsed();
						wtl.SetUsed();
					} else if (ctl.HasValue && cbl.HasValue ) {				// left
						int tl = wtl.GetCeilVert(), bl = wtl.GetFloorVert(), br = wbl.GetFloorVert(), tr = wbl.GetCeilVert();
						AddRectangle(true, tl, bl, br, tr);
						wtl.SetUsed();
						wbl.SetUsed();
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

	private class WallVert {
		public int c1, f1, c2, f2;
		public bool isUsed = false;

		public WallVert(int c1, int f1, int c2, int f2) {
			this.c1 = c1;
			this.f1 = f1;
			this.c2 = c2;
			this.f2 = f2;
		}

		public int GetCeilVert() {
			return this.isUsed ? this.c2 : this.c1;
		}

		public int GetFloorVert() {
			return this.isUsed ? this.f2 : this.f1;
		}

		public void SetUsed() {
			this.isUsed = true;
		}
	}
}
