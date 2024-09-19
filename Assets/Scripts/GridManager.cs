using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static GridElement;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.Rendering.DebugUI.Table;

public class GridManager : MonoBehaviour
{
    public GameObject levelStatus;
    public GameObject celebrationPanel;
    public GameObject failPanel;
    public GameObject gridObject;
    public GameObject gridMaskObject;

    public TextAsset[] levelStructuresText;
    public GameObject[] cubePrefabs;
    public GameObject[] obstaclePrefabs;
    public GameObject tntPrefab;


    private LevelSerialize levelSerialize = new LevelSerialize();
    public int gridWidth;
    public int gridHeight;
    private int boxCounter = 0;
    private int stoneCounter = 0;
    private int vaseCounter = 0;
    private int moveCounter = 0;

    private GameObject[,] grid;
    private Vector2[,] GridPositions;

    private bool canUserClick = true;

    private void Start()
    {
        this.levelSerialize = JsonUtility.FromJson<LevelSerialize>(this.levelStructuresText[PlayerPrefs.GetInt("level")].text);
        this.gridWidth = this.levelSerialize.grid_width;
        this.gridHeight = this.levelSerialize.grid_height;
        this.moveCounter = this.levelSerialize.move_count;
        celebrationPanel.SetActive(false);
        InitializeGrid();
        levelStatus.GetComponent<LevelStatus>().StartUp(boxCounter, stoneCounter, vaseCounter, this.moveCounter);
    }

    /*FOR INITIALIZE*/
    private void InitializeGrid()
    {
        grid = new GameObject[gridWidth, gridHeight*2];
        GridPositions = new Vector2[gridWidth, gridHeight*2];
        arrangeGridSize(new Vector2(gridWidth, gridHeight));
        calculatePositions(new Vector2(gridWidth, gridHeight));
        createElementDependsOnStructure();
        tntCreatableIinitalize();
    }

    private void arrangeGridSize(Vector2 gridSize)
    {
        float gridWidth = gridObject.GetComponent<SpriteRenderer>().bounds.size.x;
        float desiredGridHeight = (gridWidth / gridSize.x) * gridSize.y;

        if(desiredGridHeight > 8)
        {
            float gridHeight = gridObject.GetComponent<SpriteRenderer>().bounds.size.y;
            float desiredGridWidth = (gridHeight / gridSize.y) * gridSize.x;
        }
        else
        {
            gridObject.GetComponent<SpriteRenderer>().size = new Vector2(gridWidth, desiredGridHeight);
            gridMaskObject.transform.localScale = new Vector2(gridWidth, desiredGridHeight);
        }
    }

    private void calculatePositions(Vector2 gridSize)
    {
        for (int y = 0; y < gridSize.y*2; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                float parentWidth = gridObject.GetComponent<SpriteRenderer>().bounds.size.x * 0.97f;
                float parentHeight = gridObject.GetComponent<SpriteRenderer>().bounds.size.y * 0.95f;

                float cellWidth = parentWidth / gridSize.x;
                float cellHeight = parentHeight / gridSize.y;

                float parentLeftBorder = (-parentWidth / 2.0f) + (cellWidth / 2.0f);
                float parentBottomBorder = (-parentHeight / 2.0f) + (cellHeight / 2.0f);

                float absolutePosX = (x * cellWidth) + parentLeftBorder;
                float absolutePosY = (y * cellHeight) + parentBottomBorder;
                if(y >= gridHeight) absolutePosY += 0.3f;
                GridPositions[x, y] = new Vector2(absolutePosX, absolutePosY);
            }
        }
    }

    private void createElementDependsOnStructure()
    {
        for (int y = gridHeight - 1; y > -1; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {//t: TNT, bo: Box, s: Stone, v: Vase
                Vector2 gridPosition = new Vector2(x, y);
                switch (this.levelSerialize.grid[(y * (gridWidth - 1)) + x])
                {
                    case "b":
                        createCubeWithColor(gridPosition, Cube.CubeColor.Blue);
                        break;

                    case "g":
                        createCubeWithColor(gridPosition, Cube.CubeColor.Green);
                        break;

                    case "r":
                        createCubeWithColor(gridPosition, Cube.CubeColor.Red);
                        break;

                    case "y":
                        createCubeWithColor(gridPosition, Cube.CubeColor.Yellow);
                        break;

                    case "rand":
                        CreateRandomCube(gridPosition);
                        break;

                    case "t":
                        CreateRandomCube(gridPosition);
                        break;

                    case "bo":
                        boxCounter++;
                        createObstacleWithType(gridPosition, Obstacle.ObstacleType.Box);
                        break;

                    case "s":
                        stoneCounter++;
                        createObstacleWithType(gridPosition, Obstacle.ObstacleType.Stone);
                        break;

                    case "v":
                        vaseCounter++;
                        createObstacleWithType(gridPosition, Obstacle.ObstacleType.Vase);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private void tntCreatableIinitalize()
    {
        for (int y = 0; y < this.gridHeight; y++)
        {
            for (int x = 0; x < this.gridWidth; x++)
            {
                ElementType isCube = this.grid[x, y].GetComponent<GridElement>().elementType;
                if (isCube == GridElement.ElementType.Cube)
                {
                    if ((grid[x, y].GetComponent<Cube>().isTntCreatable != true))
                    {
                        List<(GameObject cube, int distance)> adjacentCubes = getAdjacentCubesWithDistances(new Vector2(x, y));
                        if (adjacentCubes.Count > 4)
                        {
                            for (int i = 0; i < adjacentCubes.Count; i++)
                            {
                                adjacentCubes[i].cube.GetComponent<Cube>().setTntCreatable(true);
                            }
                        }
                    }
                }
            }
        }
    }

    private Vector3 GetElementScale(Vector2 gridPosition, GameObject currentCube)
    {
        float parentWidth = gridObject.GetComponent<SpriteRenderer>().bounds.size.x * 0.97f;
        float cellWidth = parentWidth / gridWidth;
        float childWidth = currentCube.GetComponent<SpriteRenderer>().bounds.size.x;
        float desiredXScale = (cellWidth / childWidth);

        float parentHeight = gridObject.GetComponent<SpriteRenderer>().bounds.size.y * 0.95f;
        float cellHeight = parentHeight / gridHeight;
        float childHeight = currentCube.GetComponent<SpriteRenderer>().bounds.size.y;
        float desiredYScale = cellHeight / childHeight;

        Vector3 desiredWorldScale = new Vector3(desiredXScale, desiredXScale, 1f);
        return desiredWorldScale;
    }



    /*FOR CREATIONS*/
    private void createCubeWithColor(Vector2 gridPosition, Cube.CubeColor color)
    {
        int randomIndex = Random.Range(0, cubePrefabs.Length);

        foreach (var prefab in cubePrefabs)
        {
            if(prefab.GetComponent<Cube>().cubeColor == color)
            {
                GameObject newCubeObj = Instantiate(prefab, gridObject.transform);
                Cube newCube = newCubeObj.GetComponent<Cube>();
                newCube.gridPosition = new Vector2(gridPosition.x, gridPosition.y);
                newCube.setGridManager(this);

                newCubeObj.transform.localPosition = this.GridPositions[(int)gridPosition.x, (int)gridPosition.y];
                newCubeObj.transform.localScale = GetElementScale(gridPosition, newCubeObj);
                newCubeObj.GetComponent<SpriteRenderer>().sortingOrder = (int)(gridPosition.y) + 1;
                grid[(int)gridPosition.x, (int)gridPosition.y] = newCubeObj;
            }
        }
    }

    private void CreateRandomCube(Vector2 gridPosition)
    {
        int randomIndex = Random.Range(0, cubePrefabs.Length);
        GameObject selectedPrefab = cubePrefabs[randomIndex];
        GameObject newCubeObj = Instantiate(selectedPrefab, gridObject.transform);

        Cube newCube = newCubeObj.GetComponent<Cube>();
        newCube.gridPosition = new Vector2(gridPosition.x, gridPosition.y);
        newCube.setGridManager(this);

        newCubeObj.transform.localPosition = this.GridPositions[(int)gridPosition.x, (int)gridPosition.y];
        newCubeObj.transform.localScale = GetElementScale(gridPosition, newCubeObj);
        newCubeObj.GetComponent<SpriteRenderer>().sortingOrder = (int)(gridPosition.y) + 1;
        grid[(int)gridPosition.x, (int)gridPosition.y] = newCubeObj;
    }

    private void createTnt(Vector2 gridPosition)
    {
        GameObject newTntObj = Instantiate(this.tntPrefab, gridObject.transform);

        TNT newTnt = newTntObj.GetComponent<TNT>();
        newTnt.gridPosition = new Vector2(gridPosition.x, gridPosition.y);
        newTnt.setGridManager(this);

        newTntObj.transform.localPosition = this.GridPositions[(int)gridPosition.x, (int)gridPosition.y];
        newTntObj.transform.localScale = GetElementScale(gridPosition, newTntObj);
        newTntObj.GetComponent<SpriteRenderer>().sortingOrder = (int)(gridPosition.y) + 1;
        grid[(int)gridPosition.x, (int)gridPosition.y] = newTntObj;
    }

    private void createObstacleWithType(Vector2 gridPosition, Obstacle.ObstacleType type)
    {

        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab.GetComponent<Obstacle>().obstacleType == type)
            {
                GameObject newObstacleObj = Instantiate(prefab, gridObject.transform);
                Obstacle newObstacle = newObstacleObj.GetComponent<Obstacle>();
                newObstacle.gridPosition = new Vector2(gridPosition.x, gridPosition.y);
                newObstacle.setGridManager(this);

                newObstacleObj.transform.localPosition = this.GridPositions[(int)gridPosition.x, (int)gridPosition.y];
                newObstacleObj.transform.localScale = GetElementScale(gridPosition, newObstacleObj);
                newObstacleObj.GetComponent<SpriteRenderer>().sortingOrder = (int)(gridPosition.y) + 1;
                grid[(int)gridPosition.x, (int)gridPosition.y] = newObstacleObj;
            }
        }
    }



    /*FOR SIGNAL->SLOTS, SIGNAL FROM OTHER SCRIPTS TO THIS*/
    public void removeElementCallback(Vector2 gridPosition)
    {
        this.grid[(int)gridPosition.x, (int)gridPosition.y] = null;
        SpawnNewCube((int)gridPosition.x);
        CollapseGrid((int)gridPosition.x);
    }

    public void removeCubeAddTntCallback(Vector2 gridPosition)
    {
        this.grid[(int)gridPosition.x, (int)gridPosition.y] = null;
        createTnt(gridPosition);
        CollapseGrid((int)gridPosition.x);
    }

    public void explodeTntCallback(Vector2 gridPosition)
    {
        this.grid[(int)gridPosition.x, (int)gridPosition.y] = null;
        createTnt(gridPosition);
        CollapseGrid((int)gridPosition.x);
    }

    public void obstacleDestroyedCallback(Obstacle.ObstacleType type)
    {
        if(type == Obstacle.ObstacleType.Box)
        {
            boxCounter--;
            levelStatus.GetComponent<LevelStatus>().decreaseBoxCounter();
        }

        if (type == Obstacle.ObstacleType.Stone)
        {
            stoneCounter--;
            levelStatus.GetComponent<LevelStatus>().decreaseStoneCounter();
        }

        if (type == Obstacle.ObstacleType.Vase)
        {
            vaseCounter--;
            levelStatus.GetComponent<LevelStatus>().decreaseVaseCounter();
        }

        if (boxCounter == 0 && stoneCounter == 0 && vaseCounter == 0)
        {
            celebrationPanel.transform.localScale = Vector3.zero;
            celebrationPanel.SetActive(true);
            celebrationPanel.transform.DOScale(new Vector3(0.1f, 0.1f, 1), 0.5f)
            .SetEase(Ease.InOutBounce);

            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1);
            PlayerPrefs.Save();
            DOTween.Sequence()
                .AppendInterval(3f)
                .AppendCallback(() => SceneManager.LoadScene("MainScene", LoadSceneMode.Single));
            
        }
    }

    public void clickedElement(Vector2 gridPosition)
    {
        if (canUserClick == true)
        {
            float maxDelayForUserCanClicked = 0;
            if (grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<GridElement>().isExploding == false)
            {
                if (grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<GridElement>().elementType == GridElement.ElementType.Cube)
                {
                    List<(GameObject cube, int distance)> adjacentCubes = getAdjacentCubesWithDistances(gridPosition);

                    if (adjacentCubes.Count > 1)
                    {
                        setCanUserClick(false);
                        adjacentCubes[0].cube.GetComponent<Cube>().isClicked = true;
                        List<(GameObject cube, int distance)> adjacentObstacles = getAdjacentObstacles(adjacentCubes);

                        for (int i = 1; i < adjacentCubes.Count; i++)
                        {
                            Cube cubeComponent = adjacentCubes[i].cube.GetComponent<Cube>();
                            float delay = adjacentCubes[i].distance * 0.2f;
                            if (delay > maxDelayForUserCanClicked) maxDelayForUserCanClicked = delay;
                            cubeComponent.setDestinationPositionForJoining(GridPositions[(int)gridPosition.x, (int)gridPosition.y]);
                            cubeComponent.isExploding = true;
                            cubeComponent.Invoke("OnExplode", delay);
                        }

                        maxDelayForUserCanClicked += 0.2f;
                        if (grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<Cube>().isTntCreatable == true)
                        {
                            adjacentCubes[0].cube.GetComponent<Cube>().onExplodeDuration += maxDelayForUserCanClicked;
                            maxDelayForUserCanClicked = adjacentCubes[0].cube.GetComponent<Cube>().onExplodeDuration;
                        }
                        adjacentCubes[0].cube.GetComponent<Cube>().isExploding = true;
                        adjacentCubes[0].cube.GetComponent<Cube>().Invoke("OnExplode", adjacentCubes[0].distance * 0.2f);

                        foreach ((GameObject obstacleObj, int distance) in adjacentObstacles)
                        {

                            Obstacle obstacleComponent = obstacleObj.GetComponent<Obstacle>();
                            if (obstacleComponent.isDamageFromCube == true)
                            {
                                float delay = distance * 0.2f;
                                if ((delay + obstacleComponent.onExplodeDuration) > maxDelayForUserCanClicked)
                                {
                                    maxDelayForUserCanClicked = delay + obstacleComponent.onExplodeDuration;
                                }
                                obstacleComponent.isExploding = true;
                                obstacleComponent.Invoke("takeDamage", delay);
                            }
                        }

                        DOTween.Sequence()
                            .AppendInterval(maxDelayForUserCanClicked + 0.40f)
                            .AppendCallback(() => setCanUserClick(true));
                    }
                }

                if (grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<GridElement>().elementType == GridElement.ElementType.TNT)
                {
                    setCanUserClick(false);
                    List<(GameObject Tnt, int distance)> adjacentTnts = getAdjacentTntWithDistances(gridPosition);

                    if (adjacentTnts.Count > 1)
                    {
                        for (int i = 1; i < adjacentTnts.Count; i++)
                        {
                            TNT tntComponent = adjacentTnts[i].Tnt.GetComponent<TNT>();
                            float delay = adjacentTnts[i].distance * 0.2f;
                            if (delay > maxDelayForUserCanClicked) maxDelayForUserCanClicked = delay;
                            tntComponent.setDestinationPositionForJoining(GridPositions[(int)gridPosition.x, (int)gridPosition.y]);
                            adjacentTnts[i].Tnt.GetComponent<TNT>().isExploding = true;
                            tntComponent.Invoke("comboJoining", delay);
                        }
                    }
                    int explodeRange = ((adjacentTnts.Count > 1) ? 7 : 5);
                    maxDelayForUserCanClicked += 0.2f;
                    DOTween.Sequence()
                        .AppendInterval(maxDelayForUserCanClicked)
                        .AppendCallback(() => explodeAllEffectedElements(adjacentTnts[0].Tnt, explodeRange, maxDelayForUserCanClicked));
                }
            }
        }
    }


    /*FOR SOME GAMEPLAY AND FEATURES CONTROL*/
    private void setCanUserClick(bool newState = true)
    {
        canUserClick = newState;
        if(canUserClick == false)
        {
            levelStatus.GetComponent<LevelStatus>().decreaseMoveCounter();
        }
        else
        {
            if(levelStatus.GetComponent<LevelStatus>().MoveCount == 0)
            {
                if(boxCounter > 0 || stoneCounter > 0 || vaseCounter > 0)
                {
                    failPanel.transform.localScale = Vector3.zero;
                    failPanel.SetActive(true);
                    failPanel.transform.DOScale(new Vector3(0.1f, 0.1f, 1), 0.5f)
                    .SetEase(Ease.InOutBounce);
                }
            }
        }
    }

    private void SpawnNewCube(int x)
    {
        int y = gridHeight;
        while (grid[x, y] != null)
        {
            y++;
        }
        CreateRandomCube(new Vector2(x, y));
    }

    private void tntCreatableControl()
    {
        bool[,] controlled = new bool[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            if(grid[i, gridHeight] != null)
            {
                int rowIndex = gridHeight - 1;
                while (rowIndex > 0)
                {
                    if(grid[i, rowIndex] != null)
                    {
                        if(grid[i, rowIndex].GetComponent<GridElement>().elementType == GridElement.ElementType.Obstacle && grid[i, rowIndex].GetComponent<GridElement>().isFalldownable == false)
                        {
                            if (grid[i, rowIndex - 1] == null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    rowIndex--;
                }
            }
        }

        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                if (controlled[x, y] == false)
                {
                    if (this.grid[x, y] != null)
                    {
                        ElementType elementType = this.grid[x, y].GetComponent<GridElement>().elementType;
                        if (elementType == GridElement.ElementType.Cube)
                        {

                            List<(GameObject cube, int distance)> adjacentCubes = getAdjacentCubesWithDistances(new Vector2(x, y));
                            foreach (var (cubeObj,_) in adjacentCubes)
                            {
                                Vector2 position = cubeObj.GetComponent<GridElement>().gridPosition;
                                controlled[(int)position.x, (int)position.y] = true;
                            }

                            if (adjacentCubes.Count < 5)
                            {
                                for (int i = 0; i < adjacentCubes.Count; i++)
                                {
                                    if(adjacentCubes[i].cube.GetComponent<Cube>().isExploding == false)
                                    {
                                        adjacentCubes[i].cube.GetComponent<Cube>().setTntCreatable(false);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < adjacentCubes.Count; i++)
                                {
                                    if (adjacentCubes[i].cube.GetComponent<Cube>().isExploding == false)
                                    {
                                        adjacentCubes[i].cube.GetComponent<Cube>().setTntCreatable(true);
                                    }
                                    
                                }
                            }
                        }

                        if(elementType == GridElement.ElementType.TNT)
                        {
                            List<(GameObject tnt, int distance)> adjacentTnts = getAdjacentTntWithDistances(new Vector2(x, y));
                            foreach (var (tntObj, _) in adjacentTnts)
                            {
                                Vector2 position = tntObj.GetComponent<GridElement>().gridPosition;
                                controlled[(int)position.x, (int)position.y] = true;
                            }

                            if (adjacentTnts.Count > 1)
                            {
                                for (int i = 0; i < adjacentTnts.Count; i++)
                                {
                                    adjacentTnts[i].tnt.GetComponent<TNT>().setCanCombo(true);
                                }
                            }
                            else
                            {
                                adjacentTnts[0].tnt.GetComponent<TNT>().setCanCombo(false);
                            }
                        }
                    }
                }
            }
        }
    }

    private void CollapseGrid(int x)
    {
        for (int y = 1; y < gridHeight*2; y++)
        {
            if(grid[x, y] != null)
            {
                GridElement gridElement = grid[x, y].GetComponent<GridElement>();
                if(gridElement.isExploding != true && gridElement.isFalldownable == true)
                {
                    int targetY = y;
                    while (targetY > 0 && grid[x, targetY - 1] == null)
                    {
                        targetY--;
                    }
                    
                    if (targetY != y)
                    {
                        GameObject element = grid[x, y];
                        grid[x, targetY] = element;
                        grid[x, y] = null;
                        element.GetComponent<GridElement>().gridPosition.Set(x, targetY);
                        element.GetComponent<SpriteRenderer>().sortingOrder = targetY + 1;

                        Vector2 targetPosition = GridPositions[x, targetY];
                        element.GetComponent<GridElement>().fallDown(targetPosition);
                    }
                }
            }
        }
        Invoke(nameof(tntCreatableControl), 0.32f);
    }

    private void explodeAllEffectedElements(GameObject startTnt, int range, float beforeDelay)
    {
        float durationOfThisAnimations = 0;
        List<(Vector2 position, int distance)> effectedList = startTnt.GetComponent<TNT>().getDamagePositionsWithDistance(range);
        for (int i = 0; i < effectedList.Count; i++)
        {
            float delay = effectedList[i].distance * 0.2f;
            GridElement elementComponent = grid[(int)effectedList[i].position.x, (int)effectedList[i].position.y].GetComponent<GridElement>();
            if(elementComponent.elementType == GridElement.ElementType.Obstacle)
            {
                if(elementComponent.GetComponent<Obstacle>().isDamageFromTnt == true)
                {
                    if (delay > durationOfThisAnimations) durationOfThisAnimations = delay;
                    elementComponent.isExploding = true;
                    elementComponent.Invoke("takeDamage", delay);
                }
            }
            else
            {
                if (delay > durationOfThisAnimations) durationOfThisAnimations = delay;
                elementComponent.isExploding = true;
                elementComponent.Invoke("takeDamage", delay);
            }
        }
        durationOfThisAnimations += startTnt.GetComponent<TNT>().onExplodeDuration;
        durationOfThisAnimations += beforeDelay;
        DOTween.Sequence()
                        .AppendInterval(durationOfThisAnimations + 0.40f)
                        .AppendCallback(() => setCanUserClick(true));
    }






    /*ALGORITHMS FOR ADJACENCY*/
    private List<(GameObject cube, int distance)> getAdjacentCubesWithDistances(Vector2 gridPosition)
    {
        List<(GameObject, int)> connectedCubesWithDistances = new List<(GameObject, int)>();
        Cube startCube = grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<Cube>();
        bool[,] visited = new bool[gridWidth, gridHeight];
        FindConnectedCubesWithDistanceRecursive(gridPosition, startCube.cubeColor, visited, connectedCubesWithDistances, 0);
        return connectedCubesWithDistances;
    }

    
    private void FindConnectedCubesWithDistanceRecursive(Vector2 pos, Cube.CubeColor color, bool[,] visited, List<(GameObject, int)> connectedCubesWithDistances, int distance)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight || visited[x, y])
            return;

        if((grid[x, y] == null) || (grid[x, y].GetComponent<GridElement>().elementType != GridElement.ElementType.Cube))
            return;

        visited[x, y] = true;

        Cube currentCube = grid[x, y].GetComponent<Cube>();

        if (currentCube.cubeColor != color)
            return;

        connectedCubesWithDistances.Add((grid[x, y], distance));

        FindConnectedCubesWithDistanceRecursive(new Vector2(x + 1, y), color, visited, connectedCubesWithDistances, distance + 1);  // Sağ
        FindConnectedCubesWithDistanceRecursive(new Vector2(x - 1, y), color, visited, connectedCubesWithDistances, distance + 1);  // Sol
        FindConnectedCubesWithDistanceRecursive(new Vector2(x, y + 1), color, visited, connectedCubesWithDistances, distance + 1);  // Yukarı
        FindConnectedCubesWithDistanceRecursive(new Vector2(x, y - 1), color, visited, connectedCubesWithDistances, distance + 1);  // Aşağı
    }

    private List<(GameObject obstacle, int distance)> getAdjacentObstacles(List<(GameObject cube, int distance)> connectedCubes)
    {
        List<(GameObject obstacle, int distance)> adjacentObstacles = new List<(GameObject, int)>();
        bool[,] visited = new bool[gridWidth, gridHeight];

        foreach (var (cube, distance) in connectedCubes)
        {
            Vector2 cubePosition = cube.GetComponent<Cube>().gridPosition;

            foreach (Vector2 neighborPos in GetNeighbors(cubePosition))
            {
                int x = (int)neighborPos.x;
                int y = (int)neighborPos.y;

                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && !visited[x, y])
                {
                    if (grid[x, y] != null)
                    {
                        if (grid[x, y].GetComponent<GridElement>().elementType == GridElement.ElementType.Obstacle)
                        {
                            adjacentObstacles.Add((grid[x, y], distance+1));
                            visited[x, y] = true;
                        } 
                    }
                }
            }
        }

        return adjacentObstacles;
    }

    private List<(GameObject Tnt, int distance)> getAdjacentTntWithDistances(Vector2 gridPosition)
    {
        List<(GameObject, int)> connectedTntsWithDistances = new List<(GameObject, int)>();
        bool[,] visited = new bool[gridWidth, gridHeight];
        FindConnectedTntsWithDistanceRecursive(gridPosition, visited, connectedTntsWithDistances, 0);
        return connectedTntsWithDistances;
    }
    private void FindConnectedTntsWithDistanceRecursive(Vector2 pos, bool[,] visited, List<(GameObject, int)> connectedTntsWithDistances, int distance)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight || visited[x, y])
            return;

        if ((grid[x, y] == null) || (grid[x, y].GetComponent<GridElement>().elementType != GridElement.ElementType.TNT))
            return;

        visited[x, y] = true;

        connectedTntsWithDistances.Add((grid[x, y], distance));
        FindConnectedTntsWithDistanceRecursive(new Vector2(x + 1, y), visited, connectedTntsWithDistances, distance + 1);  // Sağ
        FindConnectedTntsWithDistanceRecursive(new Vector2(x - 1, y), visited, connectedTntsWithDistances, distance + 1);  // Sol
        FindConnectedTntsWithDistanceRecursive(new Vector2(x, y + 1), visited, connectedTntsWithDistances, distance + 1);  // Yukarı
        FindConnectedTntsWithDistanceRecursive(new Vector2(x, y - 1), visited, connectedTntsWithDistances, distance + 1);  // Aşağı
    }

    private List<Vector2> GetNeighbors(Vector2 pos)
    {
        return new List<Vector2>
        {
            new Vector2(pos.x + 1, pos.y),  // Sağ
            new Vector2(pos.x - 1, pos.y),  // Sol
            new Vector2(pos.x, pos.y + 1),  // Yukarı
            new Vector2(pos.x, pos.y - 1)   // Aşağı
        };
    }



    /*FOR BUTTON INTERACTION*/
    public void returnMainScene()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void restartLevelScene()
    {
        SceneManager.LoadScene("LevelScene", LoadSceneMode.Single);
    }


    /*GETTERS, USED IN OTHER SCRIPTS TO ACCESS PRIVATE VARIABLES*/
    public GridElement.ElementType getElementType(Vector2 gridPosition)
    {
        if ((int)gridPosition.x < gridWidth && (int)gridPosition.y < gridHeight)
        {
            if (grid[(int)gridPosition.x, (int)gridPosition.y] != null)
            {
                return grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<GridElement>().elementType;
            }
            else
            {
                return GridElement.ElementType.Empty;
            }
        }
        else
        {
            return GridElement.ElementType.Empty;
        }
    }

    public TNT getTntComponent(Vector2 gridPosition)
    {
        return grid[(int)gridPosition.x, (int)gridPosition.y].GetComponent<TNT>();
    }
}

