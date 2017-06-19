using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {
	
	List<int> triangles = new List<int>();

	GameObject wallsObject, floorObject, ceilingObject;

	WallVertex[,] wallVertMap;
	List<Vector3> wallVertices;

	Vertex[,] ceilingVertMap, floorVertMap;
	List<Vector3> ceilingVertices, floorVertices;

	List<int> wallTriangles, ceilingTriangles;

	void Awake() {
		wallVertMap = new WallVertex[width, height];
		wallVertices = new List<Vector3>();

		ceilingVertices = new Vertex[width+2, height+2];
		ceilingVertices = new List<Vector3>();

		floorVertMap = new Vertex[width, height];
		floorVertices = new List<Vector3>();
	}

	public void GenerateMesh(bool[,] map, int width, int height) {
		GenerateVertices(map, width, height);
		GenerateTriangles(map, width, height);

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		wallsObject = this.transform.Find("Walls").gameObject;
		floorObject = this.transform.Find("Floor").gameObject;
		ceilingObject = this.transform.Find("Ceiling").gameObject;


		wallsObject.GetComponent<MeshFilter>().mesh = mesh;
		wallsObject.GetComponent<MeshCollider>().sharedMesh = mesh;

		// GetComponent<MeshFilter>().mesh = mesh;
		// GetComponent<MeshCollider>().sharedMesh = mesh;
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
					if (MapUtils.IsEdgeTile(x, y, width, height, map)) {
						AddWallVertices(x, y);
						// AddCeilingAndFloorVertices(x, y);
					} else {
						AddCeilingVertex(x, y);
					}
				} else {
					// AddFloorVertex(x, y);
				}
			}
		}
	}

	void GenerateTriangles(bool[,] map, int width, int height) {
		for (int x = 0; x < width - 1; x++) {
			for (int y = 0; y < height - 1; y++) {
				if (wallVertMap[x, y] != null) {
					
				}
			}
		}


		// Scan through every 4-vert square in the vert grid, draw mesh triangles accordingly
		for (int x = 0; x < width + 1; x++) {
			for (int y = 0; y < height + 1; y++) {
				Vertex ctl = ceilingVertMap[x, y];
				Vertex ctr = ceilingVertMap[x+1, y];
				Vertex cbl = ceilingVertMap[x, y+1];
				Vertex cbr = ceilingVertMap[x+1, y+1];
				// Vertex ftl = floorVertMap[x, y];
				// Vertex ftr = floorVertMap[x+1, y];
				// Vertex fbl = floorVertMap[x, y+1];
				// Vertex fbr = floorVertMap[x+1, y+1];
				if (ctl	!= null && ctr != null && cbl != null && cbr != null) {
					AddRectangle(false, ctl, cbl, cbr, ctr);
				} else if (ctr != null && cbl != null && cbr != null) {	// tr <-> bl
					AddTriangle(false, ctr, cbl, cbr);
					// AddTriangle(ftr, ftl, fbl);
					AddRectangle(true, cbl, ctr, ftr, fbl);
				} else if (ctl != null && cbl != null && cbr != null) {	// tl <-> br
					AddTriangle(false, ctl, cbl, cbr);
					// AddTriangle(ftl, fbr, ftr);
					AddRectangle(true, ctl, cbr, fbr, ftl);
				} else if (ctl != null && ctr != null && cbr != null) {	// tl <-> br
					// AddTriangle(ctl, cbr, ctr);
					AddTriangle(ftl, fbl, fbr);
					AddRectangle(ftl, fbr, cbr, ctl);
				} else if (ctl != null && ctr != null && cbl != null) {	// tr <-> bl
					// AddTriangle(ctl, cbl, ctr);
					AddTriangle(fbl, fbr, ftr);
					AddRectangle(fbl, ftr, ctr, cbl);
				} else {
					if (ctl != null && ctr != null) {
						AddRectangle(ctl, ftl, ftr, ctr);
					} else if (ctr != null && cbr != null) {
						AddRectangle(ctr, ftr, fbr, cbr);
					} else if (cbr != null && cbl != null) {
						AddRectangle(cbr, fbr, fbl, cbl);
					} else if (cbl != null && ctl != null ) {
						AddRectangle(cbl, fbl, ftl, ctl);
					}
					AddRectangle(ftl, fbl, fbr, ftr);
				}
			}
		}
	}

	void AddCeilingVertex(int x, int y) {
		ceilingVertices.Add(new Vector3(x, 2, y));
		ceilingVertMap[x+1, y+1] = new Vertex(vertices.Count - 1);
	}

	void AddFloorVertex(int x, int y) {
		vertices.Add(new Vector3(x, 0, y));
		floorVertMap[x+1, y+1] = vertices.Count - 1;
		ceilingVertMap[x+1, y+1] = -1;
	}

	void AddWallVertices(int x, int y) {
		wallVertices.Add(new Vector3(x, 2, y));
		wallVertices.Add(new Vector3(x, 0, y));
		wallVertMap[x, y] = new WallVertex(wallVertices.Count - 1, wallVertices.Count - 2);
	}

	void AddCeilingAndFloorVertices(int x, int y) {
		vertices.Add(new Vector3(x, 2, y));
		ceilingVertMap[x+1, y+1] = vertices.Count - 1;
		vertices.Add(new Vector3(x, 0, y));
		floorVertMap[x+1, y+1] = vertices.Count - 1;
	}

	void AddRectangle(bool isWall, Vertex v1, Vertex v2, Vertex v3, Vertex v4) {
		AddTriangle(isWall, v1, v2, v3);
		AddTriangle(isWall, v1, v3, v4);
	}

	void AddTriangle(bool isWall, Vertex v1, Vertex v2, Vertex v3) {
		if (isWall) {
			wallTriangles.Add(v1.index);
			wallTriangles.Add(v2.index);
			wallTriangles.Add(v3.index);
		} else {
			ceilingTriangles.Add(v1.index);
			ceilingTriangles.Add(v2.index);
			ceilingTriangles.Add(v3.index);
		}
	}

	struct Vertex {
		public int index;

		Vertex(int index) {
			this.index = index;
		}
	}

	struct WallVertex {
		public int ceilingIndex, floorIndex;

		WallVertex(int ceilingIndex, int floorIndex) {
			this.ceilingIndex = ceilingIndex;
			this.floorIndex = floorIndex;
		}
	}
}
