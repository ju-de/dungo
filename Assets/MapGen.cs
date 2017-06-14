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

	bool[,] map;		// true = WHITE = WALL; false = BLACK = ROOM

	void Start() {
		GenerateMap();
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			GenerateMap();
		}
	}

	void GenerateMap() {
		map = new bool[width,height];

		FillMap();
		for (int i = 0; i < smoothIterations; i++) {
			SmoothMap();
		}


		ExpandRooms();
//		RemoveThinWalls();
		SmoothMap();
		RemoveSmallRegions();

		MeshGen meshGen = GetComponent<MeshGen>();
		meshGen.GenerateMesh(map, width, height);
	}

	void FillMap() {
		if (useCustomSeed) {
			UnityEngine.Random.seed = seed;
		} else {
			UnityEngine.Random.seed = (int) System.DateTime.Now.ToFileTime();
		}
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
					map[x, y] = true;
				} else {
					map[x, y] = UnityEngine.Random.Range(0, 100) < fill;
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
						if (map[dx, dy]) walls++;
					}
				}
				map[x, y] = walls > 4;
			}
		}
	}

	void ExpandRooms() {
		bool[,] mapCopy = (bool[,]) map.Clone();
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if (!(mapCopy[x-1, y] && mapCopy[x+1, y] && mapCopy[x, y-1] && mapCopy[x, y+1])) {
					map[x, y] = false;
				}
			}
		}
	}

	void RemoveThinWalls() {
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if ((!map[x-1, y-1] && !map[x+1, y+1]) || (!map[x+1, y-1] && !map[x-1, y+1])) {
					map[x, y] = false;
				}
			}
		}
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if ((!map[x-1, y] && !map[x+1, y]) || (!map[x, y-1] && !map[x, y+1])) {
					map[x, y] = false;
				}
			}
		}
	}

	void RemoveSmallRegions() {
		bool[,] visited = new bool[width, height];

		List<List<Tile>> wallRegions = new List<List<Tile>>();
		List<List<Tile>> roomRegions = new List<List<Tile>>();

		// find all wall and room regions
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (!visited[x, y]) {
					List<Tile> region = BFS(x, y, visited);
					if (map[x, y]) {
						wallRegions.Add(region);
					} else {
						roomRegions.Add(region);
					}
				}
			}
		}

		// remove wall regions that are too small
		foreach (List<Tile> region in wallRegions) {
			if (region.Count < wallThreshold) {
				foreach (Tile tile in region) {
					map[tile.x,tile.y] = false;
				}
			}
		}
		// remove room regions that are too small
		foreach (List<Tile> region in roomRegions) {
			if (region.Count < roomThreshold) {
				foreach (Tile tile in region) {
					map[tile.x,tile.y] = true;
				}
			}
		}
	}

	List<Tile> BFS(int startX, int startY, bool[,] visited) {
		List<Tile> tiles = new List<Tile>();
		Queue<Tile> queue = new Queue<Tile>();
		bool tileType = map[startX, startY];

		queue.Enqueue(new Tile(startX, startY));
		visited[startX, startY] = true;
		while (queue.Count > 0) {
			Tile t = queue.Dequeue();
			tiles.Add(t);

			List<Tile> neighbours = GetNeighbourTiles(t);
			foreach (Tile n in neighbours) {
				if (!visited[n.x, n.y] && map[n.x, n.y] == tileType) {
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

	bool IsInMapRange(Tile t) {
		return t.x >= 0 && t.x < width && t.y >= 0 && t.y < height;
	}

	void OnDrawGizmos() {
//		for (int x = 0; x < width; x++) {
//			for (int y = 0; y < height; y++) {
//				Gizmos.color = map[x, y] ? Color.white : Color.black;
//				Vector3 pos = new Vector3(-width/2 + x + .5f, 0, -height/2 + y + .5f);
//				Gizmos.DrawCube(pos,Vector3.one);
//			}
//		}
	}
}
