using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class MapGen : MonoBehaviour {

	public int seed;
	public bool useCustomSeed = false;
	public int width, height;
	[Range(0, 100)]
	public int fill;	// Percentage of map that starts as FILLED
	public int smoothIterations;
	public int wallThreshold, roomThreshold;

	public bool drawGizmos = false;

	bool[,] tileMap;		// true = WHITE = WALL; false = BLACK = ROOM
	List<List<Tile>> wallRegions;
	List<List<Tile>> roomRegions;

	void Start() {
		GenerateMap();
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			GenerateMap();
		}
	}

	void GenerateMap() {
		tileMap = new bool[width, height];

		wallRegions = new List<List<Tile>>();
		roomRegions = new List<List<Tile>>();

		FillMap();
		for (int i = 0; i < smoothIterations; i++) {
			SmoothMap();
		}


		ExpandRooms();
		RemoveThinWalls();
		SmoothMap();
		RemoveSmallRegions();

		GenerateMesh(tileMap, width, height);
	}

	void FillMap() {
		UnityEngine.Random.seed = useCustomSeed ? seed : (int) System.DateTime.Now.ToFileTime();
		Debug.Log(UnityEngine.Random.seed);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
					tileMap[x, y] = true;
				} else {
					tileMap[x, y] = UnityEngine.Random.Range(0, 100) < fill;
				}
			}
		}
	}

	void SmoothMap() {
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				// Get number of surrounding wall tiles
				int walls = 0;
				for (int dx = x - 1; dx <= x + 1; dx++) {
					for (int dy = y - 1; dy <= y + 1; dy++) {
						if (tileMap[dx, dy]) walls++;
					}
				}
				tileMap[x, y] = walls > 4;
			}
		}
	}

	void ExpandRooms() {
		bool[,] mapCopy = (bool[,]) tileMap.Clone();
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if (!(mapCopy[x-1, y] && mapCopy[x+1, y] && mapCopy[x, y-1] && mapCopy[x, y+1])) {
					tileMap[x, y] = false;
				}
			}
		}
	}

	void RemoveThinWalls() {
		// for (int x = 1; x < width - 1; x++) {
		// 	for (int y = 1; y < height - 1; y++) {
		// 		if ((!map[x-1, y-1] && !map[x+1, y+1]) || (!map[x+1, y-1] && !map[x-1, y+1])) {
		// 			map[x, y] = false;
		// 		}
		// 	}
		// }
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if ((!tileMap[x-1, y] && !tileMap[x+1, y]) || (!tileMap[x, y-1] && !tileMap[x, y+1])) {
					tileMap[x, y] = false;
				}
			}
		}
	}

	void RemoveSmallRegions() {
		// find all wall and room regions
		GetAllRegions();

		// remove wall regions that are too small
		foreach (List<Tile> region in wallRegions) {
			if (region.Count < wallThreshold) {
				foreach (Tile tile in region) {
					tileMap[tile.x,tile.y] = false;
				}
			}
		}
		// remove room regions that are too small
		foreach (List<Tile> region in roomRegions) {
			if (region.Count < roomThreshold) {
				foreach (Tile tile in region) {
					tileMap[tile.x,tile.y] = true;
				}
			}
		}

		// get final regions
		GetAllRegions();
		foreach (List<Tile> room in roomRegions) {
			Debug.Log(room.Count);
		}
	}

	void GetAllRegions() {
		bool[,] visited = new bool[width, height];
		wallRegions.Clear();
		roomRegions.Clear();

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (!visited[x, y]) {
					List<Tile> region = BFS(x, y, visited);
					if (tileMap[x, y]) {
						wallRegions.Add(region);
					} else {
						roomRegions.Add(region);
					}
				}
			}
		}
	}

	List<Tile> BFS(int startX, int startY, bool[,] visited) {
		List<Tile> tiles = new List<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		bool tileType = tileMap[startX, startY];

		queue.Enqueue(new Tile(startX, startY));
		visited[startX, startY] = true;
		while (queue.Count > 0) {
			Tile t = queue.Dequeue();
			tiles.Add(t);

			List<Tile> neighbours = GetNeighbourTiles(t);
			foreach (Tile n in neighbours) {
				if (!visited[n.x, n.y] && tileMap[n.x, n.y] == tileType) {
					queue.Enqueue(n);
					visited[n.x, n.y] = true;
				}
			}
		}
		return tiles;
	}

	List<Tile> GetNeighbourTiles(Tile t) {
		List<Tile> tiles = new List<Tile>();
		List<Tile> ret = new List<Tile>();
		tiles.Add(new Tile(t.x - 1, t.y));
		tiles.Add(new Tile(t.x + 1, t.y));
		tiles.Add(new Tile(t.x, t.y - 1));
		tiles.Add(new Tile(t.x, t.y + 1));

		foreach(Tile n in tiles) {
			if (IsInMapRange(n)) ret.Add(n);
		}
		return ret;
	}

	bool IsEdgeTile(int x, int y) {
		List<Tile> neighbours = new List<Tile>();
		neighbours.Add(new Tile(x, y+1));
		neighbours.Add(new Tile(x, y-1));
		neighbours.Add(new Tile(x-1, y));
		neighbours.Add(new Tile(x+1, y));
		foreach (Tile n in neighbours) {
			if (IsInMapRange(n) && !tileMap[n.x, n.y]) {
				return true;
			}
		}
		return false;
	}

	bool IsInMapRange(Tile t) {
		return t.x >= 0 && t.x < width && t.y >= 0 && t.y < height;
	}



	List<Vector3> vertices = new List<Vector3>();
	int[,] ceilingVertMap, floorVertMap;
	List<int> triangles = new List<int>();

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
					if (IsEdgeTile(x, y)) {
						AddCeilingAndFloorVertices(x, y);
					} else {
						AddCeilingVertex(x, y);
					}
				} else {
					// AddFloorVertex(x, y);
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
				if (ctl != -1 && ctr != -1 && cbl != -1 && cbr != -1) {
					AddRectangle(ctl, cbl, cbr, ctr);
				} else if (ctr != -1 && cbl != -1 && cbr != -1) {	// tr <-> bl
					AddTriangle(ctr, cbl, cbr);
					// AddTriangle(ftr, ftl, fbl);
					AddRectangle(cbl, ctr, ftr, fbl);
				} else if (ctl != -1 && cbl != -1 && cbr != -1) {	// tl <-> br
					AddTriangle(ctl, cbl, cbr);
					// AddTriangle(ftl, fbr, ftr);
					AddRectangle(ctl, cbr, fbr, ftl);
				} else if (ctl != -1 && ctr != -1 && cbr != -1) {	// tl <-> br
					AddTriangle(ctl, cbr, ctr);
					// AddTriangle(ftl, fbl, fbr);
					AddRectangle(ftl, fbr, cbr, ctl);
				} else if (ctl != -1 && ctr != -1 && cbl != -1) {	// tr <-> bl
					AddTriangle(ctl, cbl, ctr);
					// AddTriangle(fbl, fbr, ftr);
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
					// AddTriangle(ftl, fbl, fbr);
					// AddTriangle(ftl, fbr, ftr);
				}
			}
		}

		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
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

	void OnDrawGizmos() {
		if (drawGizmos) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Gizmos.color = tileMap[x, y] ? Color.white : Color.black;
					Vector3 pos = new Vector3(x, 0, y);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
		}
	}
}
