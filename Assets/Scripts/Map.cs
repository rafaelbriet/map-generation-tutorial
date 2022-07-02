using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField]
    private int numberOfRooms = 10;
    [SerializeField]
    private bool ignoreRoomWithNeighbor = true;
    [SerializeField]
    private bool complyWithNumberOfRooms = true;
    [SerializeField]
    [Range(0f, 0.9f)]
    private float roomGiveUpChance = 0.5f;
    [Header("Map")]
    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int height = 10;
    [Header("Rooms")]
    [SerializeField]
    private float roomWidth = 10;
    [SerializeField]
    private float roomHeight = 10;
    [SerializeField]
    private GameObject roomPrefab;
    [SerializeField]
    private GameObject roomBossPrefab;
    [SerializeField]
    private GameObject roomTreasurePrefab;
    [Header("Doors")]
    [SerializeField]
    private GameObject doorUpPrefab;
    [SerializeField]
    private GameObject doorDownPrefab;
    [SerializeField]
    private GameObject doorRightPrefab;
    [SerializeField]
    private GameObject doorLeftPrefab;


    private Room[,] map;
    private List<Room> deadEndRooms = new List<Room>();

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
            deadEndRooms.Clear();

            Room startingRoom = map[width / 2, height / 2];
            startingRoom.IsSelected = true;

            Queue<Room> roomsToVisit = new Queue<Room>();
            roomsToVisit.Enqueue(startingRoom);

            roomsCount = 1;

            while (roomsToVisit.Count > 0)
            {
                Room currentRoom = roomsToVisit.Dequeue();
                List<Room> currentRoomNeighbors = GetNeighbors(currentRoom);
                bool hasAddedNewRoom = false;

                foreach (Room neighbor in currentRoomNeighbors)
                {
                    if (neighbor.IsSelected)
                    {
                        continue;
                    }

                    if (ignoreRoomWithNeighbor && HasMoreThanOneNeighbor(neighbor))
                    {
                        continue;
                    }

                    bool hasEnoughRoom = roomsCount >= numberOfRooms;

                    if (complyWithNumberOfRooms && hasEnoughRoom)
                    {
                        continue;
                    }

                    bool shouldGiveUp = UnityEngine.Random.Range(0, 1f) < roomGiveUpChance;

                    if (shouldGiveUp)
                    {
                        continue;
                    }

                    neighbor.IsSelected = true;
                    roomsToVisit.Enqueue(neighbor);
                    roomsCount++;
                    hasAddedNewRoom = true;
                }

                if (!hasAddedNewRoom)
                {
                    deadEndRooms.Add(currentRoom);
                }
            }
        }
    }

    private void PlaceRooms()
    {
        PlaceNormalRooms();
        PlaceBossRoom();
        PlaceTreasureRooms();
    }

    private void PlaceNormalRooms()
    {
        foreach (Room room in map)
        {
            if (room.IsSelected && !deadEndRooms.Contains(room))
            {
                InstantiateRoom(roomPrefab, room);
            }
        }
    }

    private void PlaceBossRoom()
    {
        Room room = deadEndRooms.Last();
        InstantiateRoom(roomBossPrefab, room);
    }

    private void PlaceTreasureRooms()
    {
        List<Room> rooms = deadEndRooms.Where(room => room != deadEndRooms.Last()).ToList();

        foreach (Room room in rooms)
        {
            float treasureChance = UnityEngine.Random.Range(0f, 1f);
            bool hasTreasure = treasureChance > 0.7f;

            if (hasTreasure)
            {
                InstantiateRoom(roomTreasurePrefab, room);
            }
            else
            {
                InstantiateRoom(roomPrefab, room);
            }
        }
    }

    private GameObject InstantiateRoom(GameObject roomPrefab, Room room)
    {
        Vector3 roomPosition = new Vector3(room.Position.x * roomWidth, room.Position.y * roomHeight);
        GameObject roomGameObject = Instantiate(roomPrefab, roomPosition, Quaternion.identity, transform);
        AddDoorsToRoom(roomGameObject, room);
        
        return roomGameObject;
    }

    private void AddDoorsToRoom(GameObject roomGameObject, Room room)
    {
        int up = room.Position.y + 1;

        if (IsRoomValid(new Vector3Int(room.Position.x, up)))
        {
            ReplaceWallWithDoor(roomGameObject, "Up", doorUpPrefab);
        }

        int down = room.Position.y - 1;

        if (IsRoomValid(new Vector3Int(room.Position.x, down)))
        {
            ReplaceWallWithDoor(roomGameObject, "Down", doorDownPrefab);
        }

        int right = room.Position.x + 1;

        if (IsRoomValid(new Vector3Int(right, room.Position.y)))
        {
            ReplaceWallWithDoor(roomGameObject, "Right", doorRightPrefab);
        }

        int left = room.Position.x - 1;

        if (IsRoomValid(new Vector3Int(left, room.Position.y)))
        {
            ReplaceWallWithDoor(roomGameObject, "Left", doorLeftPrefab);
        }
    }

    private void ReplaceWallWithDoor(GameObject roomGameObject, string wallName, GameObject doorPrefab)
    {
        Transform wall = roomGameObject.transform.Find("Walls");
        Transform wallPiece = wall.Find(wallName);
        Instantiate(doorPrefab, wall, false);
        Destroy(wallPiece.gameObject);
    }

    private bool IsRoomValid(Vector3Int position)
    {
        if (!IsRoomPositionValid(position))
        {
            return false;
        }

        Room room = map[position.x, position.y];

        return room.IsSelected;
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
