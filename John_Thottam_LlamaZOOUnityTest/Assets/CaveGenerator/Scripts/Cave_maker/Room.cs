//***************************************************************/
// Get isolated region details and connect
//**************************************************************/

using System;
using System.Collections.Generic;

public class Room : IComparable<Room>
{
    public List<Coord> m_lstOfTiles;
    public List<Coord> m_lstOfEdgeTiles;
    public List<Room> m_lstOfConnectedRooms;
    public int m_iRoomSize;
    public bool m_bIsAccessibleFromMainRoom;
    public bool m_bIsMainRoom;

    public Room()
    {
    }

    public Room(List<Coord> a_roomTiles, int[,] a_map)
    {
        m_lstOfTiles = a_roomTiles;
        m_iRoomSize = m_lstOfTiles.Count;
        m_lstOfConnectedRooms = new List<Room>();

        m_lstOfEdgeTiles = new List<Coord>();
        foreach (Coord tile in m_lstOfTiles)
        {
            for (int x = tile.m_iTileX - 1; x <= tile.m_iTileX + 1; x++)
            {
                for (int y = tile.m_iTileY - 1; y <= tile.m_iTileY + 1; y++)
                {
                    if (x == tile.m_iTileX || y == tile.m_iTileY)
                    {
                        if (a_map[x, y] == 1)
                        {
                            m_lstOfEdgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!m_bIsAccessibleFromMainRoom)
        {
            m_bIsAccessibleFromMainRoom = true;
            foreach (Room connectedRoom in m_lstOfConnectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRooms(Room a_RoomA, Room a_RoomB)
    {
        if (a_RoomA.m_bIsAccessibleFromMainRoom)
        {
            a_RoomB.SetAccessibleFromMainRoom();
        }
        else if (a_RoomB.m_bIsAccessibleFromMainRoom)
        {
            a_RoomA.SetAccessibleFromMainRoom();
        }
        a_RoomA.m_lstOfConnectedRooms.Add(a_RoomB);
        a_RoomB.m_lstOfConnectedRooms.Add(a_RoomA);
    }

    public bool IsConnected(Room a_OtherRoom)
    {
        return m_lstOfConnectedRooms.Contains(a_OtherRoom);
    }

    public int CompareTo(Room a_OtherRoom)
    {
        return a_OtherRoom.m_iRoomSize.CompareTo(m_iRoomSize);
    }
}
