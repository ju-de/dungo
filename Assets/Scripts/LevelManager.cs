using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	public GameObject map;
	public bool debugMapGen = false;

	public GameObject player;

	private MapGen mapGen;
	private MeshGen meshGen;

	void Awake() {
		mapGen = map.GetComponent<MapGen>();
		meshGen = map.GetComponent<MeshGen>();
	}

	void Start () {
		bool[,] tileMap = mapGen.GenerateMap();
		PlacePlayer(tileMap);
		mapGen.debug = debugMapGen;
		if (!debugMapGen) {
			meshGen.GenerateMesh(tileMap, mapGen.width, mapGen.height);
		}
	}

	void Update () {

	}

	void PlacePlayer(bool[,] tileMap) {
		int width = mapGen.width;
		int height = mapGen.height;

		// scan diagonally for player spawn location
		for (int sum = 0; sum < width + height - 1; sum++) {
			int x = Math.Min(sum, width - 1), y = sum - x;
			while (x >= 0 && y < height) {
				if (x > 0 && y > 0 && x < width - 1 && y < height - 1
						&& MapUtils.GetWallNeighbours(tileMap, x, y) == 0) {
					player.GetComponent<Rigidbody>().MovePosition(new Vector3(x, 0, y));
					return;
				}
				x--;
				y++;
			}
		}
	}
}
