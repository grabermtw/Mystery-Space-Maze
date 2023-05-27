using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

struct Wall {
    public Vector3 coord;
    public bool isVertical;
    public List<Cell> cells;

    public Wall(Vector3 coord, bool isVertical, List<Cell> cells) {
        this.coord = coord;
        this.isVertical = isVertical;
        this.cells = cells;
    }
}

struct Cell {
    public Vector3 coord;

    public Cell(Vector3 coord) {
        this.coord = coord;
    }
}

public class MazeMaker : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject cellPrefab;
    public GameObject cornerMarkerPrefab;
    public float cornerDropHeight = 2;
    public Transform bottomLeftTarget;
    public Transform topRightTarget;
    public Transform topLeftTarget;
    public Transform bottomRightTarget;
    public Vector3 mazeOrigin;
    public List<Vector2> levelBorders;
    public Slider progressBar;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI giveUpHeader;
    public TextMeshProUGUI giveUpSubheader;
    public GameObject giveUpPanel;


    private List<Wall> walls;
    private List<Wall> borderWalls;
    private List<HashSet<Cell>> cellSets;
    private List<GameObject> corners;
    private int currentLevel = 0;
    private List<Vector3> doorCellCoords;
    private int cellsExplored = 0;
    private GameplayAudio gameAudio;

    private string[] subheaderPercentSentences = new string[] {
        "It seems you only explored a really small portion of the maze!",
        "That's a decent amount of the maze you've explored!",
        "Wow! You explored a lot of the known maze!",
        "Looks like you've got nearly perfect knowledge of the maze!"
    };
    private string[] subheaderLevelSentences = new string[] {
        "It seems to be a small maze anyway, so it wouldn't have taken you too long to finish exploring the whole thing, right?",
        "The maze turned out to be bigger than expected, but a valiant effort was made to explore it nonetheless!",
        "Can't blame you though, who could've known this maze would be so big?",
        "Dang this is a big maze."
    };

    void Start()
    {
        walls = new List<Wall>();
        borderWalls = new List<Wall>();
        cellSets = new List<HashSet<Cell>>();
        corners = new List<GameObject>();
        doorCellCoords = new List<Vector3>();
        PlanMaze(levelBorders);
        InstantiateMaze(new Vector3(0,0,0));
        progressBar.value = 0;
        percentageText.text = "0%";
        giveUpPanel.SetActive(false);
        gameAudio = GetComponent<GameplayAudio>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Use randomized Kruskal's Algorithm to plan the maze
    public void PlanMaze(List<Vector2> levelBorders)
    {
        int xDim = (int)levelBorders[levelBorders.Count - 1].x;
        int yDim = (int)levelBorders[levelBorders.Count - 1].y;
        // Initialize the lists
        for (int x = 0; x < xDim + 1; x++)
        {
            for (int y = 0; y < yDim + 1; y++)
            {
                if (y < yDim && x < xDim)
                {
                    cellSets.Add(new HashSet<Cell>() { new Cell(new Vector3(x, 0 ,y)) });
                }
                // Vertical wall
                if (y < yDim)
                {
                    List<Cell> wallCells = new List<Cell>();
                    if (x > 0) {
                        wallCells.Add(new Cell(new Vector3(x - 1, 0, y)));
                    }
                    if (x < xDim) {
                        wallCells.Add(new Cell(new Vector3(x, 0, y)));
                    }
                    if (wallCells.Count > 1)
                    {
                        walls.Add(new Wall(new Vector3(x, 0, y), true, wallCells));
                    }
                    else
                    {
                        borderWalls.Add(new Wall(new Vector3(x, 0, y), true, wallCells));
                    }
                }
                // Horizontal wall
                if (x < xDim)
                {
                    List<Cell> wallCells = new List<Cell>();
                    if (y > 0) {
                        wallCells.Add(new Cell(new Vector3(x, 0, y - 1)));
                    }
                    if (y < yDim) {
                        wallCells.Add(new Cell(new Vector3(x, 0, y)));
                    }
                    if (wallCells.Count > 1)
                    {
                        walls.Add(new Wall(new Vector3(x, 0, y), false, wallCells));
                    }
                    else
                    {
                        borderWalls.Add(new Wall(new Vector3(x, 0, y), false, wallCells));
                    }
                } 
            }
        }

        // Perform random Kruskal
        // Begin with shuffle
        for (int i = 0; i < walls.Count; i++) {
            Wall temp = walls[i];
            int randomIndex = Random.Range(i, walls.Count);
            walls[i] = walls[randomIndex];
            walls[randomIndex] = temp;
        }

        Vector3[] levelDoors = new Vector3[levelBorders.Count];
        
        List<Wall> unvisitedWalls = new List<Wall>(walls);

        while (cellSets.Count > 1 && unvisitedWalls.Count > 0)
        {
            Wall victimWall = unvisitedWalls[Random.Range(0, unvisitedWalls.Count)];
            // First we want to check if the victim wall is on a level border
            // We only want one door in a level border, so we skip if there's already one.
            bool skip = false;
            for (int j = 0; j < levelBorders.Count; j++)
            {
                if ((victimWall.isVertical && victimWall.coord.x == levelBorders[j].x && victimWall.coord.z < levelBorders[j].y) ||
                    (!victimWall.isVertical && victimWall.coord.x < levelBorders[j].x && victimWall.coord.z == levelBorders[j].y))
                {
                    if (levelDoors[j] == new Vector3(0,0,0))
                    {
                        levelDoors[j] = victimWall.coord;
                        // record coordinates of the door that will trigger the next level
                        if (victimWall.isVertical)
                        {
                            doorCellCoords.Add(victimWall.coord - new Vector3(1, 0, 0));
                        } else
                        {
                            doorCellCoords.Add(victimWall.coord - new Vector3(0, 0, 1));
                        }
                    }
                    else
                    {
                        skip = true;
                    }
                }
            }
            if (!skip)
            {
                // find the sets that contain each of the wall's cells
                int cellSetIdx1 = -1;
                int cellSetIdx2 = -1;
                for (int i = 0; i < cellSets.Count; i++)
                {
                    if (cellSets[i].Contains(victimWall.cells[0]))
                    {
                        cellSetIdx1 = i;
                    }
                    if (cellSets[i].Contains(victimWall.cells[1]))
                    {
                        cellSetIdx2 = i;
                    }
                }
                // remove the wall if they're different sets and combine the sets
                if (cellSetIdx1 != cellSetIdx2)
                {
                    cellSets[cellSetIdx1].UnionWith(cellSets[cellSetIdx2]);
                    cellSets.RemoveAt(cellSetIdx2);
                    walls.Remove(victimWall);
                } 
            }
            unvisitedWalls.Remove(victimWall);
        }
    }

    // Perform the instantiation of the maze using the walls list
    private void InstantiateMaze(Vector3 origin)
    {
        // Create the walls
        List<Wall> allWalls = Enumerable.Concat(walls, borderWalls).ToList();
        for (int i = 0; i < allWalls.Count; i++)
        {
            if (!(allWalls[i].isVertical && allWalls[i].coord == new Vector3(0,0,1)))
            {
                Quaternion rotation = Quaternion.identity;
                if (!allWalls[i].isVertical)
                {
                    rotation.eulerAngles = new Vector3(0, 90, 0);
                }
                Instantiate(wallPrefab, allWalls[i].coord + origin, rotation, transform);
            }
        }

        // Create the cells
        for (int i = 0; i < cellSets.Count; i++)
        {
            List<Cell> cellList = cellSets[i].ToList();
            for (int j = 0; j < cellList.Count; j++)
            {
                Instantiate(cellPrefab, cellList[j].coord + origin, Quaternion.identity, transform);
            }
        }
    }

    public void SetMazeLevel(int level)
    {
        topRightTarget.position = new Vector3(levelBorders[level].x, 1, levelBorders[level].y) + mazeOrigin;
        topLeftTarget.position = new Vector3(0, 1, levelBorders[level].y) + mazeOrigin;
        bottomRightTarget.position = new Vector3(levelBorders[level].x, 1, 0) + mazeOrigin;

        for (int i = 0; i < corners.Count; i++)
        {
            Destroy(corners[i]);
        }

        corners = new List<GameObject>();
        corners.Add(Instantiate(cornerMarkerPrefab, new Vector3(0, cornerDropHeight, 0) + mazeOrigin, Quaternion.identity));
        corners.Add(Instantiate(cornerMarkerPrefab, new Vector3(levelBorders[level].x, cornerDropHeight, levelBorders[level].y) + mazeOrigin, Quaternion.identity));
        corners.Add(Instantiate(cornerMarkerPrefab, new Vector3(0, cornerDropHeight, levelBorders[level].y) + mazeOrigin, Quaternion.identity));
        corners.Add(Instantiate(cornerMarkerPrefab, new Vector3(levelBorders[level].x, cornerDropHeight, 0) + mazeOrigin, Quaternion.identity));
    }

    private void IncrementMazeLevel()
    {
        currentLevel += 1;
        SetMazeLevel(currentLevel);
        gameAudio.PlayRandomChord(4);
    }

    public void CellIlluminated(Vector3 pos)
    {
        // Check if it's a door
        if (doorCellCoords.Contains(pos))
        {
            doorCellCoords.Remove(pos);
            IncrementMazeLevel();
        }

        // Keep track of percentage of maze that's been illuminated
        cellsExplored++;
        float progress = cellsExplored / (levelBorders[currentLevel].x * levelBorders[currentLevel].y);
        progressBar.value = progress;
        float progressRounded = Mathf.Round(progress * 10000f) / 10000f;
        percentageText.text = progressRounded * 100f + "%";
        if (progress == 1)
        {
            Win();
        }
    }

    public void GiveUp()
    {
        giveUpHeader.text = "You explored " + percentageText.text + " of the known maze!";
        float progress = cellsExplored / (levelBorders[currentLevel].x * levelBorders[currentLevel].y);
        string subheader = "";
        if (progress < 0.33f)
        {
            subheader += subheaderPercentSentences[0];
        }
        else if (progress >= 0.33f && progress < 0.66f)
        {
            subheader += subheaderPercentSentences[1];
        }
        else if (progress >= 0.66f && progress < 0.9f)
        {
            subheader += subheaderPercentSentences[2];
        } else
        {
            subheader += subheaderPercentSentences[3];
        }

        subheader += " " + subheaderLevelSentences[currentLevel];
        giveUpSubheader.text = subheader;

        giveUpPanel.SetActive(true);
    }

    public void Win()
    {
        giveUpHeader.text = "You explored 100% of the maze! Wow, you're a legend!";
        giveUpSubheader.text = "Now get outside and touch some grass!";
        giveUpPanel.SetActive(true);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
