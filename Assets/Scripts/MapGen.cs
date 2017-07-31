using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class MapGen : MonoBehaviour {

	public int seed;
	public bool useCustomSeed = false;
	[Range(0, 100)]
	public int fill;
	public int smoothAndFillIterations, smoothIterations;
	public int wallThreshold;
	public int minMapSize, maxMapSize;

	public bool debug = false;

	bool[,] tileMap;		// true = WHITE = WALL; false = BLACK = ROOM
	int width, height;
	List<List<Tile>> wallRegions;
	List<List<Tile>> roomRegions;
	
	void Update() {
		if (debug && Input.GetMouseButtonDown(0)) {
            GenerateMap(width, height);
        }
	}

	public bool[,] GenerateMap(int width, int height) {
		this.width = width;
		this.height = height;

		Random.seed = useCustomSeed ? seed : (int) System.DateTime.Now.ToFileTime();
		Debug.Log(Random.seed);

		for (int i = 0; i < 5; i++) {
			if (TryGeneratingMap()) break;
		}

		return tileMap;
	}

	bool TryGeneratingMap() {
		tileMap = new bool[width, height];

		wallRegions = new List<List<Tile>>();
		roomRegions = new List<List<Tile>>();

		FillMap();
		for (int i = 0; i < smoothAndFillIterations; i++) {
			SmoothAndAddWalls();
		}
		for (int i = 0; i < smoothIterations; i++) {
			SmoothMap();
		}


		// ExpandRooms();
		RemoveSmallRooms();
		RemoveSmallWalls();
		ClearPassages();

		int filled = 0 ;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (tileMap[x, y]) filled++;
			}
		}

		int unfilledPercentage = (int) ((1f - 1f * filled / (width * height)) * 100f);
		Debug.Log("Map size percentage: " + unfilledPercentage);
		return unfilledPercentage >= minMapSize && unfilledPercentage <= maxMapSize;
	}

	void FillMap() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				// pre-fill outer regions so open space does not "clip" at edges of the map and create long, flat walls
				if (x < 10 || x >= width - 10 || y < 10 || y >= height - 10) {
					tileMap[x, y] = true;
				} else {
					tileMap[x, y] = Random.Range(0, 100) < fill;
				}
			}
		}
	}

	void SmoothMap() {
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if (tileMap[x, y]) {
					tileMap[x, y] = MapUtils.GetWallNeighbours(tileMap, x, y) >= 4;
				} else {
					tileMap[x, y] = MapUtils.GetWallNeighbours(tileMap, x, y) >= 5;
				}
			}
		}
	}

	void SmoothAndAddWalls() {
		for (int x = 2; x < width - 2; x++) {
			for (int y = 2; y < height - 2; y++) {
				tileMap[x, y] = tileMap[x, y] ||
						MapUtils.GetWallNeighbours(tileMap, x, y) >= 5 ||
						MapUtils.GetTwoTileWallNeighbours(tileMap, x, y) <= 1;
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

	void ClearPassages() {
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				if (!tileMap[x, y]) {
					if (tileMap[x-1, y] && tileMap[x+1, y] && !tileMap[x, y-1] && !tileMap[x, y+1]) {
						tileMap[x-1, y] = true;
						tileMap[x+1, y] = true;
					} else if (!tileMap[x-1, y] && !tileMap[x+1, y] && tileMap[x, y-1] && tileMap[x, y+1]) {
						tileMap[x, y-1] = true;
						tileMap[x, y+1] = true;
					}
				}
			}
		}
	}

	void RemoveSmallRooms() {
		GetAllRegions();

		// only keep the largest room
		List<Tile> largestRoom = roomRegions[0];
		foreach (List<Tile> region in roomRegions) {
			if (region.Count > largestRoom.Count) {
				largestRoom = region;
			}
		}
		foreach (List<Tile> region in roomRegions) {
			if (region != largestRoom) {
				foreach (Tile tile in region) {
					tileMap[tile.x, tile.y] = true;
				}
			}
		}
	}

	void RemoveSmallWalls() {
		GetAllRegions();

		// remove wall regions that are too small
		foreach (List<Tile> region in wallRegions) {
			if (region.Count < wallThreshold) {
				foreach (Tile tile in region) {
					tileMap[tile.x, tile.y] = false;
				}
			}
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

			List<Tile> neighbours = MapUtils.GetCardinalNeighbours(t, width, height);
			foreach (Tile n in neighbours) {
				if (!visited[n.x, n.y] && tileMap[n.x, n.y] == tileType) {
					queue.Enqueue(n);
					visited[n.x, n.y] = true;
				}
			}
		}
		return tiles;
	}

	void OnDrawGizmos() {
		if (debug) {
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
