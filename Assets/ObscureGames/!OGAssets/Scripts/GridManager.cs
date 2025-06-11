using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GridManager instance;

    [SerializeField] private Canvas gridCanvas;
    [SerializeField] internal Transform gridHolder;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] internal GridTile tilePrefab;

    [SerializeField] private Vector2Int gridSize = new Vector2Int(7, 7);
    internal int cellSize = 0;
    private int gridX = 0;
    private int gridY = 0;

    [SerializeField] internal int gridSeed = -1; // -1 is random seed

    public List<GridTile> tileList = new List<GridTile>();
    public List<GridTile> tileListRandom = new List<GridTile>();
    public int tileListRandomIndex = 0;

    public GridItem itemPrefab;
    public ScriptableGridItem[] itemTypes;
    public GridItem[] itemPowerups;

    [SerializeField] private bool allowDiagonal = true;

    public float itemDropDelay = 0.05f;
    public float itemDropTime = 0.05f;

    [SerializeField] private Animator gridAnimator;

    public Color gridColor;
    public Color gridTileColor1;
    public Color gridTileColor2;

    public ScriptableGridPattern overrideGridPattern;

    private void Awake()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //CreateGrid();

        //FillGrid();

        if (gridSeed == -1) Random.InitState(gridSeed);
        // Create a random tiles list from the current list

        CreateGrid();

        tileListRandom = tileList;
        tileListRandom = tileListRandom.OrderBy(x => Random.value).ToList();
        Debug.Log(Random.value + " Random.value");


        gridCanvas.enabled = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gridSeed);
            stream.SendNext(tileListRandomIndex);
        }
        else
        {
            gridSeed = (int)stream.ReceiveNext();
            tileListRandomIndex = (int)stream.ReceiveNext();
        }
    }

    public void SetGridSize(Vector2Int setValue)
    {
        gridSize = setValue;

        if ( setValue.x > setValue.y )
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = setValue.x;

            cellSize = (270 - setValue.x) / setValue.x;
        }
        else
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = setValue.y;

            cellSize = (270 - setValue.y) / setValue.y;
        }

        gridLayoutGroup.cellSize = new Vector2Int(cellSize, cellSize);
    }

    public Vector2Int GetGridSize()
    {
        return gridSize;
    }

    public void ShowGrid()
    {
        gridCanvas.enabled = true;
        gridAnimator.Play("Intro");
    }

    public void HideGrid()
    {
        gridCanvas.enabled = true;
    }

    public void CreateGrid()
    {
        tileList.Clear();

        for (gridY = 0; gridY < gridSize.y; gridY++)
        {
            for (gridX = 0; gridX < gridSize.x; gridX++)
            {
                GridTile newTile = Instantiate(tilePrefab);

                tileList.Add(newTile);

                if ( (gridX + gridY) % 2 == 0 ) newTile.tileImage.color = newTile.tileColor = gridTileColor1;
                else newTile.tileImage.color = newTile.tileColor = gridTileColor2;

                newTile.transform.SetParent(gridHolder);

                newTile.Setup();

                newTile.transform.localScale = Vector3.zero;

                LeanTween.scale(newTile.gameObject, Vector3.one, 0.5f).setDelay(gridX * gridY * 0.02f).setEaseOutBounce();
            }
        }

        // Go through all tiles and assign connections to other tiles
        for (int tileIndex = 0; tileIndex < tileList.Count; tileIndex++)
        {
            if (tileIndex % gridSize.x == 0)
            {
                // tiles on left edge
                tileList[tileIndex].connectRight = tileList[tileIndex + 1];

                // Middle of edge
                if (tileIndex != 0 && tileIndex != tileList.Count - gridSize.x)
                {
                    tileList[tileIndex].connectDown = tileList[tileIndex + gridSize.x];
                    tileList[tileIndex].connectUp = tileList[tileIndex - gridSize.x];
                }
            }

            if (tileIndex % gridSize.x == gridSize.x - 1)
            {
                // tiles on right edge
                tileList[tileIndex].connectLeft = tileList[tileIndex - 1];

                // Middle of edge
                if (tileIndex != gridSize.x - 1 && tileIndex != tileList.Count - 1)
                {
                    tileList[tileIndex].connectDown = tileList[tileIndex + gridSize.x];
                    tileList[tileIndex].connectUp = tileList[tileIndex - gridSize.x];
                }
            }

            if (tileIndex < gridSize.x)
            {
                // tiles on bottom edge
                tileList[tileIndex].connectDown = tileList[tileIndex + gridSize.x];

                // Middle of edge
                if (tileIndex > 0 && tileIndex < gridSize.x - 1)
                {
                    tileList[tileIndex].connectRight = tileList[tileIndex + 1];
                    tileList[tileIndex].connectLeft = tileList[tileIndex - 1];
                }
            }

            if (tileIndex >= tileList.Count - gridSize.x)
            {
                // tiles on top edge
                tileList[tileIndex].connectUp = tileList[tileIndex - gridSize.x];

                // Middle of edge
                if (tileIndex > tileList.Count - gridSize.x && tileIndex < tileList.Count - 1)
                {
                    tileList[tileIndex].connectRight = tileList[tileIndex + 1];
                    tileList[tileIndex].connectLeft = tileList[tileIndex - 1];
                }
            }

            if (tileIndex % gridSize.x != 0 && tileIndex % gridSize.x != gridSize.x - 1 && tileIndex >= gridSize.x && tileIndex < tileList.Count - gridSize.x)
            {
                // tiles in the middle
                tileList[tileIndex].connectRight = tileList[tileIndex + 1];
                tileList[tileIndex].connectLeft = tileList[tileIndex - 1];
                tileList[tileIndex].connectDown = tileList[tileIndex + gridSize.x];
                tileList[tileIndex].connectUp = tileList[tileIndex - gridSize.x];
            }
        }

    }

    public void SpawnItem( int itemIndex, int gridX, int gridY, int offsetY)
    {
        // Translate from grid index to list/array index
        int listIndex = gridSize.x * gridY + gridX;

        //Network Spawn
        //GridItem newItem = PhotonNetwork.Instantiate(itemPrefab.name, tileList[listIndex].transform.position,Quaternion.identity).GetComponent<GridItem>();
        GridItem newItem = Instantiate(itemPrefab, tileList[listIndex].transform);
        //newItem.transform.SetParent(tileList[listIndex].transform);

        newItem.SetType(itemIndex);

        newItem.Setup(itemTypes[itemIndex]);

        newItem.transform.localScale = Vector3.one;

        tileList[listIndex].SetCurrentItem(newItem);

        if ( offsetY > 0 )
        {
            newItem.GetComponent<RectTransform>().anchoredPosition += Vector2.up * offsetY;

            float dropDelay = itemDropDelay * (gridSize.y - gridY);
            float dropTime = itemDropTime * 2;

            tileList[listIndex].GetCurrentItem().PlayAnimationDelayed("Bounce", dropDelay + dropTime);

            newItem.isSpawning = true;

            LeanTween.move(newItem.gameObject, tileList[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay).setOnComplete(()=>
            {
                newItem.isSpawning = false;
                newItem.transform.position = tileList[listIndex].transform.position;
            });
        }
    }

    public void SpawnItem(GridItem spawnItem, GridTile parentTile, float delay)
    {
        GridItem newItem = Instantiate(spawnItem, parentTile.transform);

        newItem.transform.localScale = Vector3.one;

        if (delay > 0)
        {
            newItem.PlayAnimation("Hidden");
            newItem.PlayAnimationDelayed("IntroPowerup", delay);
        }

        parentTile.SetCurrentItem(newItem);
    }

    public void FillGrid()
    {
        if (gridSeed != -1) Random.InitState(gridSeed);

        int arrayIndex = 0;

        for (gridY = 0; gridY < gridSize.y; gridY++)
        {
            for (gridX = 0; gridX < gridSize.x; gridX++)
            {
                int randomItem = Random.Range(0, itemTypes.Length);

                if (overrideGridPattern)
                {
                    Vector2Int gridIndex = new Vector2Int(arrayIndex % gridSize.x, arrayIndex / gridSize.y);

                    if (overrideGridPattern.items[arrayIndex] < 0)
                    {
                        SpawnItem(itemPowerups[overrideGridPattern.items[arrayIndex] * -1 - 1], tileList[arrayIndex], 0);
                    }
                    else
                    {
                        SpawnItem(overrideGridPattern.items[arrayIndex], gridIndex.x, gridIndex.y, 0);
                    }

                    arrayIndex++;

                    if (arrayIndex >= gridSize.x * gridSize.y) return;
                }
                else
                {
                    SpawnItem(randomItem, gridX, gridY, 0);
                }
            }
        }
    }

    public void CollapseTiles()
    {
        // Check from bottom right to top left (from end of list to start)
        for ( int listIndex = tileList.Count - 1; listIndex >= 0; listIndex-- )
        {
            GridItem gridItem = tileList[listIndex].GetCurrentItem();

            // If this grid tile is empty (has no item), check upwards until we find an item, and take it
            if (gridItem == null)
            {
                int checkIndex = listIndex;

                while (checkIndex >= gridSize.x)
                {
                    checkIndex -= gridSize.x;

                    // If we found an item in this tile, drop it to the tile below
                    if ( tileList[checkIndex].GetCurrentItem() )
                    {
                        tileList[listIndex].SetCurrentItem(tileList[checkIndex].GetCurrentItem());

                        tileList[checkIndex].SetCurrentItem(null);

                        tileList[listIndex].GetCurrentItem().transform.SetParent(tileList[listIndex].transform);

                        float dropDelay = itemDropDelay * (gridSize.x - checkIndex / gridSize.x);
                        float dropTime = itemDropTime;

                        tileList[listIndex].GetCurrentItem().PlayAnimationDelayed("Fall", dropDelay);
                        tileList[listIndex].GetCurrentItem().PlayAnimationDelayed("Bounce", dropDelay + dropTime);

                        LeanTween.move(tileList[listIndex].GetCurrentItem().gameObject, tileList[listIndex].transform.position, dropTime).setEaseInCubic().setDelay(dropDelay);

                        break;
                    }
                }
            }
        }

        Invoke(nameof(FillGridDrop), 0.01f * gridSize.y);
    }

    public void FillGridDrop()
    {
        for (gridY = 0; gridY < gridSize.y; gridY++)
        {
            for (gridX = 0; gridX < gridSize.x; gridX++)
            {
                // Translate from grid index to list/array index
                int listIndex = gridSize.x * gridY + gridX;

                if (tileList[listIndex].GetCurrentItem() == null)
                {
                    int randomItem = Random.Range(0, itemTypes.Length);

                    SpawnItem(randomItem, gridX, gridY, gridSize.y * cellSize);
                }
            }
        }
    }

    public void ClearGrid()
    {
        for (int listIndex = 0; listIndex < tileList.Count; listIndex++)
        {
            GridItem gridItem = tileList[listIndex].GetCurrentItem();

            tileList[listIndex].eventTrigger.enabled = false;

            if (gridItem)
            {
                gridItem.PlayAnimationDelayed("Outro", listIndex * 0.02f);
            }
        }
    }

    public void DisableGrid()
    {
        for (int listIndex = 0; listIndex < tileList.Count; listIndex++)
        {
            GridItem gridItem = tileList[listIndex].GetCurrentItem();

            tileList[listIndex].eventTrigger.enabled = false;

            if (gridItem)
            {
                gridItem.SetAnimatorBool("Selectable", false);
            }
        }
    }

    public void EnableGrid()
    {
        for (int listIndex = 0; listIndex < tileList.Count; listIndex++)
        {
            GridItem gridItem = tileList[listIndex].GetCurrentItem();

            tileList[listIndex].eventTrigger.enabled = true;

            if (gridItem)
            {
                gridItem.SetAnimatorBool("Selectable", true);
            }
        }
    }

    public List<GridTile> GetTileList()
    {
        return tileList;
    }

    public void SetAllowDiagonal(bool setValue)
    {
        allowDiagonal = setValue;
    }

    public bool GetAllowDiagonal()
    {
        return allowDiagonal;
    }

    public void SetItemDropDelay(float setValue)
    {
        itemDropDelay = setValue;
    }

    public float GetItemDropDelay()
    {
        return itemDropDelay;
    }

    public void SetItemDropTime(float setValue)
    {
        itemDropTime = setValue;
    }

    public float GetItemDropTime()
    {
        return itemDropTime;
    }


    public void SetGridSeed(int setValue)
    {
        //gridSeed = setValue;
    }

    public void ShakeBoard()
    {
        gridAnimator.Play("Shake");
    }

    public GridTile GetRandomTile()
    {
        GridTile randomTile = tileListRandom[tileListRandomIndex];

        if (tileListRandomIndex < tileListRandom.Count - 1) tileListRandomIndex++;
        else tileListRandomIndex = 0;

        return randomTile;
    }

    /*public GridTile GetBoosterTile()
    {
        GridTile boosterTile = null;

        for (int tileIndex = 0; tileIndex < tileListRandom.Count; tileIndex++)
        {
            if (tileListRandom[tileIndex].GetCurrentItem() && tileListRandom[tileIndex].GetCurrentItem().type == GameManager.instance.currentPlayer.booster.GetItemType() && GameManager.instance.currentPlayer.booster.isActive == false && tileListRandom[tileIndex].GetCurrentItem().isClearing == false)
            {
                boosterTile = tileListRandom[tileIndex];

                return boosterTile;
            }
        }

        return boosterTile;
    }*/

    public GridTile GetPowerupTile()
    {
        GridTile powerupTile = null;

        for ( int tileIndex = 0; tileIndex < tileListRandom.Count; tileIndex++ )
        {
            if ( tileListRandom[tileIndex].GetCurrentItem() && tileListRandom[tileIndex].GetCurrentItem().type < 0 && tileListRandom[tileIndex].GetCurrentItem().isClearing == false )
            {
                powerupTile = tileListRandom[tileIndex];

                return powerupTile;
            }
        }

        return powerupTile;
    }
}
