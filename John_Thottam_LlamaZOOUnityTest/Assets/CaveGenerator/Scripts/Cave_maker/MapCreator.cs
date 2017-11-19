//***************************************************************/
// 2D map and logic is implemented
// Form path between isolated rooms/regions
//**************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapCreator : MonoBehaviour {

	//obtains value from editor tool GUI
    [HideInInspector]
    public int mapLength;
    [HideInInspector]
    public int mapBreadth;

    [HideInInspector]
    public string m_strSeed;

    [HideInInspector]
    public bool m_bUseRandomSeed;

    [HideInInspector]
    [Range(0, 100)]
    public int m_iRandomFillPercent;

	//walkable area coord
	public int[,] safeSpace;

    int[,] m_arrMap;    

	// create map
    public void GenerateMap()
    {
        m_arrMap = new int[mapLength, mapBreadth];
		safeSpace = new int[mapLength, mapBreadth];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

		//remove minor density cell areas
        ProcessMap();

		//create border for the map
        int borderSize = 1;
        int[,] borderedMap = new int[mapLength + borderSize * 2, mapBreadth + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < mapLength + borderSize && y >= borderSize && y < mapBreadth + borderSize)
                {
                    borderedMap[x, y] = m_arrMap[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

		//copying array value
		for (int i = 0; i < mapLength; i++) 
		{
			for (int j = 0; j < mapBreadth; j++) 
			{
				safeSpace [i, j] = m_arrMap [i, j];
			}
		}

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

	//remove minor density cell areas
    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    m_arrMap[tile.m_iTileX, tile.m_iTileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    m_arrMap[tile.m_iTileX, tile.m_iTileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, m_arrMap));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].m_bIsMainRoom = true;
        survivingRooms[0].m_bIsAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> a_AllRooms, bool a_ForceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (a_ForceAccessibilityFromMainRoom)
        {
            foreach (Room room in a_AllRooms)
            {
                if (room.m_bIsAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = a_AllRooms;
            roomListB = a_AllRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!a_ForceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.m_lstOfConnectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.m_lstOfEdgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.m_lstOfEdgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.m_lstOfEdgeTiles[tileIndexA];
                        Coord tileB = roomB.m_lstOfEdgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.m_iTileX - tileB.m_iTileX, 2) + Mathf.Pow(tileA.m_iTileY - tileB.m_iTileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !a_ForceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && a_ForceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(a_AllRooms, true);
        }

        if (!a_ForceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(a_AllRooms, true);
        }
    }

	//connect isolated rooms
    void CreatePassage(Room a_RoomA, Room a_RoomB, Coord a_TileA, Coord a_TileB)
    {
        Room.ConnectRooms(a_RoomA, a_RoomB);

        List<Coord> line = GetLine(a_TileA, a_TileB);

		//set a path between rooms
		// pass higher int value to DrawCircle(c , <passage_radius>) for a bigger passage size.
        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }


    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.m_iTileX + x;
                    int drawY = c.m_iTileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        m_arrMap[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord a_From, Coord a_To)
    {
        List<Coord> line = new List<Coord>();

        int x = a_From.m_iTileX;
        int y = a_From.m_iTileY;

        int dx = a_To.m_iTileX - a_From.m_iTileX;
        int dy = a_To.m_iTileY - a_From.m_iTileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord a_Tile)
    {
        return new Vector3(-mapLength / 2 + .5f + a_Tile.m_iTileX, 2, -mapBreadth / 2 + .5f + a_Tile.m_iTileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[mapLength, mapBreadth];

        for (int x = 0; x < mapLength; x++)
        {
            for (int y = 0; y < mapBreadth; y++)
            {
                if (mapFlags[x, y] == 0 && m_arrMap[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.m_iTileX, tile.m_iTileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int a_StartX, int a_StartY)
    {
		//list of coordinates to store cells in
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[mapLength, mapBreadth];
        int tileType = m_arrMap[a_StartX, a_StartY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(a_StartX, a_StartY));

		// flag to show that cell is sucessfully looked
        mapFlags[a_StartX, a_StartY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.m_iTileX - 1; x <= tile.m_iTileX + 1; x++)
            {
                for (int y = tile.m_iTileY - 1; y <= tile.m_iTileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.m_iTileY || x == tile.m_iTileX))
                    {
                        if (mapFlags[x, y] == 0 && m_arrMap[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

	//check if passed value exceeps map dimensions
    bool IsInMapRange(int a_X, int a_Y)
    {
        return a_X >= 0 && a_X < mapLength && a_Y >= 0 && a_Y < mapBreadth;
    }

	//generate random pattern for map
    void RandomFillMap()
    {
        if (m_bUseRandomSeed)
        {
            m_strSeed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(m_strSeed.GetHashCode());

        for (int x = 0; x < mapLength; x++)
        {
            for (int y = 0; y < mapBreadth; y++)
            {
                if (x == 0 || x == mapLength - 1 || y == 0 || y == mapBreadth - 1)
                {
                    m_arrMap[x, y] = 1;
                }
                else
                {
                    m_arrMap[x, y] = (pseudoRandom.Next(0, 100) < m_iRandomFillPercent) ? 1 : 0;
                }
            }
        }
    }

	//smoothen by filling or emptying each cell based upon the wall count of the respective cell
	//done to avoid liear or too flat patters
    void SmoothMap()
    {
        for (int x = 0; x < mapLength; x++)
        {
            for (int y = 0; y < mapBreadth; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

				if (neighbourWallTiles > 4) 
				{
					m_arrMap [x,y] = 1;
				} 
				else if (neighbourWallTiles < 4) 
				{
					m_arrMap [x,y] = 0;
				}
            }
        }

    }

	//get the wall count of a particular cell
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += m_arrMap[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
		
}
