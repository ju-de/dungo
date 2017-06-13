using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapGen : MonoBehaviour {

	public int width, height;

	[Range(0, 100)]
	public int fill;	// Percentage of map that starts as FILLED

	public int wallThreshold = 20, roomThreshold = 20;

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
		for (int i = 0; i < 5; i++) {
			SmoothMap();
		}
		RefineMap();
	}

	void FillMap() {
		UnityEngine.Random.seed = (int) System.DateTime.Now.ToFileTime();
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

	void RefineMap() {
		bool[,] visited = new bool[width, height];

		List<List<Tile>> wallRegions = new List<List<Tile>>();
		List<List<Tile>> roomRegions = new List<List<Tile>>();

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

		foreach (List<Tile> region in wallRegions) {
			if (region.Count < wallThreshold) {
				foreach (Tile tile in region) {
					map[tile.x,tile.y] = false;
				}
			}
		}
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
		bool isWall = map[startX, startY];

		queue.Enqueue(new Tile(startX, startY));
		visited[startX, startY] = true;
		while (queue.Count > 0) {
			Tile t = queue.Dequeue();
			tiles.Add(t);

			List<Tile> neighbours = GetNeighbourTiles(t);
			foreach (Tile n in neighbours) {
				if (!visited[n.x, n.y] && map[n.x, n.y] == isWall) {
					queue.Enqueue(n);
					visited[n.x, n.y] = true;
				}
			}
		}
		return tiles;
	}

	List<Tile> GetNeighbourTiles(Tile t) {
		List<Tile> ret = new List<Tile>();
		for (int x = t.x - 1; x <= t.x + 1; x++) {
			for (int y = t.y - 1; y <= t.y + 1; y++) {
				if (IsInMapRange(x, y) && !(t.x == x && t.y == y)) {
					ret.Add(new Tile(x, y));
				}
			}
		}
		return ret;
	}

	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	struct Tile {
		public int x, y;

		public Tile (int x, int y) {
			this.x = x;
			this.y = y;
		}
	}

	void OnDrawGizmos() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Gizmos.color = map[x, y] ? Color.white : Color.black;
				Vector3 pos = new Vector3(-width/2 + x + .5f, 0, -height/2 + y + .5f);
				Gizmos.DrawCube(pos,Vector3.one);
			}
		}
	}
}
