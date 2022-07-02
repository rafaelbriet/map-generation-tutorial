using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int height = 10;
    [SerializeField]
    private int numberOfRooms = 10;
    [SerializeField]
    private GameObject roomPrefab;

    private Room[,] map;

    private void Awake()
    {
        CreateNewMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateNewMap();
        }
    }

    public void CreateNewMap()
    {
        RemoveRooms();
        CreateMapLayout();
        PlaceRooms();
    }

    private void RemoveRooms()
    {
        List<Transform> rooms = transform.Cast<Transform>().ToList();

        foreach (var child in rooms)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateMapLayout()
    {
        int roomsCount = 0;

        while (roomsCount < numberOfRooms)
        {
            map = InitializeMap();

            Room startingRoom = map[width / 2, height / 2];
            startingRoom.IsSelected = true;

            Queue<Room> roomsToVisit = new Queue<Room>();
            roomsToVisit.Enqueue(startingRoom);

            roomsCount = 1;

            while (roomsToVisit.Count > 0)
            {
                Room currentRoom = roomsToVisit.Dequeue();
                List<Room> currentRoomNeighbors = GetNeighbors(currentRoom);

                foreach (Room neighbor in currentRoomNeighbors)
                {
                    if (neighbor.IsSelected)
                    {
                        continue;
                    }

                    if (HasMoreThanOneNeighbor(neighbor))
                    {
                        continue;
                    }

                    bool hasEnoughRoom = roomsCount >= numberOfRooms;

                    if (hasEnoughRoom)
                    {
                        continue;
                    }

                    bool shouldGiveUp = UnityEngine.Random.Range(0, 1f) < 0.5f;

                    if (shouldGiveUp)
                    {
                        continue;
                    }

                    neighbor.IsSelected = true;
                    roomsToVisit.Enqueue(neighbor);
                    roomsCount++;
                }
            }
        }
    }

    private void PlaceRooms()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Room room = map[x, y];

                if (room.IsSelected)
                {
                    Vector3 roomPosition = new Vector3(room.Position.x * 10, room.Position.y * 10);
                    Instantiate(roomPrefab, roomPosition, Quaternion.identity, transform);
                }
            }
        }
    }

    private bool HasMoreThanOneNeighbor(Room room)
    {
        List<Room> neighbors = GetNeighbors(room);
        int selectedNeighborsCount = 0;

        foreach (Room neighbor in neighbors)
        {
            if (neighbor.IsSelected)
            {
                selectedNeighborsCount++;
            }
        }

        return selectedNeighborsCount > 1;
    }

    private bool IsRoomPositionValid(Vector3Int position)
    {
        if (position.x < 0 || position.x >= width)
        {
            return false;
        }
        
        if (position.y < 0 || position.y >= height)
        {
            return false;
        }

        return true;
    }

    private List<Room> GetNeighbors(Room room)
    {
        List<Room> output = new List<Room>();

        int up = room.Position.y + 1;

        if (IsRoomPositionValid(new Vector3Int(room.Position.x, up)))
        {
            output.Add(map[room.Position.x, up]);
        }

        int down = room.Position.y - 1;

        if (IsRoomPositionValid(new Vector3Int(room.Position.x, down)))
        {
            output.Add(map[room.Position.x, down]);
        }

        int right = room.Position.x + 1;

        if (IsRoomPositionValid(new Vector3Int(right, room.Position.y)))
        {
            output.Add(map[right, room.Position.y]);
        }

        int left = room.Position.x - 1;

        if (IsRoomPositionValid(new Vector3Int(left, room.Position.y)))
        {
            output.Add(map[left, room.Position.y]);
        }

        return output;
    }

    private Room[,] InitializeMap()
    {
        Room[,] output = new Room[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                output[x, y] = new Room(new Vector3Int(x, y));
            }
        }

        return output;
    }
}
