using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	List<Vector3> vertices = new List<Vector3>();
	int[,] ceilingVertMap, floorVertMap;
	
	List<int> triangles = new List<int>();

	GameObject wallsObject, floorObject, ceilingObject;

	int[,] wallVertMap;
	List<Vector3> wallVertices;

	void Awake() {
		wallVertMap = new int[width, height];
		wallVertices = new List<Vector3>();
	}

	public void GenerateMesh(bool[,] map, int width, int height) {
		ceilingVertMap = new int[width+2, height+2];
		floorVertMap = new int[width+2, height+2];


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
						// AddCeilingVertex(x, y);
					}
				} else {
					AddFloorVertex(x, y);
					ceilingVertMap[x+1, y+1] = -1;
				}
			}
		}

		// Scan through every 4-vert square in the vert grid, draw mesh triangles accordingly
		for (int x = 0; x < width + 1; x++) {
			for (int y = 0; y < height + 1; y++) {
				int ctl = ceilingVertMap[x, y];
				int ctr = ceilingVertMap[x+1, y];
				int cbl = ceilingVertMap[x, y+1];
				int cbr = ceilingVertMap[x+1, y+1];
				int ftl = floorVertMap[x, y];
				int ftr = floorVertMap[x+1, y];
				int fbl = floorVertMap[x, y+1];
				int fbr = floorVertMap[x+1, y+1];
				if (ctl	 != -1 && ctr != -1 && cbl != -1 && cbr != -1) {
					// AddRectangle(ctl, cbl, cbr, ctr);
				} else if (ctr != -1 && cbl != -1 && cbr != -1) {	// tr <-> bl
					// AddTriangle(ctr, cbl, cbr);
					AddTriangle(ftr, ftl, fbl);
					AddRectangle(cbl, ctr, ftr, fbl);
				} else if (ctl != -1 && cbl != -1 && cbr != -1) {	// tl <-> br
					// AddTriangle(ctl, cbl, cbr);
					AddTriangle(ftl, fbr, ftr);
					AddRectangle(ctl, cbr, fbr, ftl);
				} else if (ctl != -1 && ctr != -1 && cbr != -1) {	// tl <-> br
					// AddTriangle(ctl, cbr, ctr);
					AddTriangle(ftl, fbl, fbr);
					AddRectangle(ftl, fbr, cbr, ctl);
				} else if (ctl != -1 && ctr != -1 && cbl != -1) {	// tr <-> bl
					// AddTriangle(ctl, cbl, ctr);
					AddTriangle(fbl, fbr, ftr);
					AddRectangle(fbl, ftr, ctr, cbl);
				} else {
					if (ctl != -1 && ctr != -1) {
						AddRectangle(ctl, ftl, ftr, ctr);
					} else if (ctr != -1 && cbr != -1) {
						AddRectangle(ctr, ftr, fbr, cbr);
					} else if (cbr != -1 && cbl != -1) {
						AddRectangle(cbr, fbr, fbl, cbl);
					} else if (cbl != -1 && ctl != -1 ) {
						AddRectangle(cbl, fbl, ftl, ctl);
					}
					AddRectangle(ftl, fbl, fbr, ftr);
				}
			}
		}

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

	void AddCeilingVertex(int x, int y) {
		vertices.Add(new Vector3(x, 2, y));
		ceilingVertMap[x+1, y+1] = vertices.Count - 1;
		floorVertMap[x+1, y+1] = -1;
	}

	void AddFloorVertex(int x, int y) {
		vertices.Add(new Vector3(x, 0, y));
		floorVertMap[x+1, y+1] = vertices.Count - 1;
		ceilingVertMap[x+1, y+1] = -1;
	}

	void AddWallVertices(int x, int y) {
		wallVertices.Add(new Vector3(x, 2, y));
		wallVertices.Add(new Vector3(x, 0, y));
		wallVertMap[x, y] = vertices.Count();
	}

	void AddCeilingAndFloorVertices(int x, int y) {
		vertices.Add(new Vector3(x, 2, y));
		ceilingVertMap[x+1, y+1] = vertices.Count - 1;
		vertices.Add(new Vector3(x, 0, y));
		floorVertMap[x+1, y+1] = vertices.Count - 1;
	}

	void AddRectangle(int v1, int v2, int v3, int v4) {
		AddTriangle(v1, v2, v3);
		AddTriangle(v1, v3, v4);
	}

	void AddTriangle(int v1, int v2, int v3) {
		triangles.Add(v1);
		triangles.Add(v2);
		triangles.Add(v3);
	}
}
