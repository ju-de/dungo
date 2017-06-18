using System.Collections;
using System.Collections.Generic;

public class MapUtils {

	public static bool IsInMapRange(Tile t, int width, int height) {
		return t.x >= 0 && t.x < width && t.y >= 0 && t.y < height;
	}

	public static List<Tile> GetCardinalNeighbours(Tile t, int width, int height) {
		List<Tile> tiles = new List<Tile>();
		List<Tile> ret = new List<Tile>();
		tiles.Add(new Tile(t.x - 1, t.y));
		tiles.Add(new Tile(t.x + 1, t.y));
		tiles.Add(new Tile(t.x, t.y - 1));
		tiles.Add(new Tile(t.x, t.y + 1));

		foreach(Tile n in tiles) {
			if (IsInMapRange(n, width, height)) ret.Add(n);
		}
		return ret;
	}

	public static bool IsEdgeTile(int x, int y, int width, int height, bool[,] map) {
		List<Tile> neighbours = new List<Tile>();
		neighbours.Add(new Tile(x, y+1));
		neighbours.Add(new Tile(x, y-1));
		neighbours.Add(new Tile(x-1, y));
		neighbours.Add(new Tile(x+1, y));
		foreach (Tile n in neighbours) {
			if (IsInMapRange(n, width, height) && !map[n.x, n.y]) {
				return true;
			}
		}
		return false;
	}

	public static int getWallNeighbours(bool[,] map, int x, int y) {
		int walls = 0;
		for (int dx = x - 1; dx <= x + 1; dx++) {
			for (int dy = y - 1; dy <= y + 1; dy++) {
				if (map[dx, dy] && !(dx == x && dy == y)) walls++;
			}
		}
		return walls;
	}

	public static int getTwoTileWallNeighbours(bool[,] map, int x, int y) {
		int walls = 0;
		for (int dx = x - 2; dx <= x + 2; dx++) {
			for (int dy = y - 2; dy <= y + 2; dy++) {
				if (map[dx, dy]) walls++;
			}
		}
		return walls;
	}
}
