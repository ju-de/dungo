﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {

	public GameObject map;
	public int mapWidth, mapHeight;
	public bool debugMapGen = false;

	public GameObject player;

	private MapGen mapGen;
	private MeshGen meshGen;
	private NavMeshSurface navMeshSurface;

	private bool[,] tileMap;

	void Awake() {
		mapGen = map.GetComponent<MapGen>();
		meshGen = map.GetComponent<MeshGen>();
		navMeshSurface = map.GetComponent<NavMeshSurface>();
	}

	void Start () {
		tileMap = mapGen.GenerateMap(mapWidth, mapHeight);
		mapGen.debug = debugMapGen;
		if (!debugMapGen) {
			meshGen.GenerateMesh(tileMap, mapWidth, mapHeight);
		}

		navMeshSurface.BuildNavMesh();

		PlacePlayer();
		PlaceEnemies();
	}

	void Update () {

	}

	void PlacePlayer() {
		// scan diagonally for player spawn location
		for (int sum = 0; sum < mapWidth + mapHeight - 1; sum++) {
			int x = Math.Min(sum, mapWidth - 1), y = sum - x;
			while (x >= 0 && y < mapHeight) {
				if (x > 0 && y > 0 && x < mapWidth - 1 && y < mapHeight - 1
						&& MapUtils.GetWallNeighbours(tileMap, x, y) == 0) {
					player.transform.Translate(x, 0, y, null);
					return;
				}
				x--;
				y++;
			}
		}
	}

	public GameObject enemy;	// todo: relocate

	void PlaceEnemies() {
		int spawnCount = 10;
		int x, y;
		while(spawnCount > 0) {
			x = (int) Random.Range(1, mapWidth - 1);
			y = (int) Random.Range(1, mapWidth - 1);

			if (MapUtils.GetWallNeighbours(tileMap, x, y) != 0) {
				continue;
			}

			Instantiate(enemy, new Vector3(x, 2, y), Quaternion.Euler(0, Random.Range(0f, 360f), 0));
			spawnCount--;
		}
	}
}
