using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridTile : MonoBehaviour
{
    public EventTrigger eventTrigger;
    private GridItem currentItem;

    internal GridTile connectRight, connectLeft, connectUp, connectDown;

    public Image tileImage;
    public Color tileColor;
    public Transform connector;
    public GameObject connectorLine;
    public Image connectorLineColor;
    public Animator connectorAnimator;
    public Image buttonImage;
    public Image glowImage;

    // Start is called before the first frame update
    void Start()
    {
        connectorLine.SetActive(false);
    }


    public void Setup()
    {

    }

    /*public void Spawn( float delat )
    {

    }

    void SpawnDelayed()
    {

    }*/

    public void Select()
    {
        //if (!TurnManager.Instance.IsLocalPlayersTurn()) return;
        // If there's no item on this tile, return
        if (currentItem == null) return;

        if (!GameManager.instance.currentPlayer.photonView.IsMine) return;

        if (GameManager.instance.currentPlayer.moves <= 0) return;

        if (GameManager.instance.playerIndex != PhotonNetwork.LocalPlayer.ActorNumber) return;        

        // Rules for linking: 1.Must be adjacent to the last tile in the link sequence (up/down/left/right) 2.Must be first in link sequence or same type 3. Must not already exist in the link 4.If we go back to a previous tile in a link, remove it

        PlayerController playerController = GameManager.instance.playerController;

        // Check the last tile in the link sequence. We will use this to check if we are connecting to same type
        GridTile lastTileInLink = null;
        if (playerController.tileLink.Count > 0) lastTileInLink = playerController.tileLink[playerController.tileLink.Count - 1];

        // If this tile was already added to the link, backtrack to it
        if (playerController.tileLink.Contains(this))
        {
            playerController.linkType = GetCurrentItem().type;
            playerController.CheckSelectables();

            // Remove all items in the link after this one
            Vector2Int gridTilePos = playerController.GetIndexInGrid(this);
            playerController.photonView.RPC("LinkRemoveAfterByGrid",RpcTarget.All, gridTilePos.x, gridTilePos.y);

            return;
        }

        // If we're not tapping and holding the tap on the screen, return
        if (!Input.GetMouseButton(0)) return;

        bool goodLink = false;

        // If this is the first tile in the link, add it regardless of type match
        if (playerController.tileLink.Count == 0)
        {
            playerController.linkType = currentItem.type;
            playerController.CheckSelectables();

            connectorLine.SetActive(false);

            Vector2Int gridTilePos = playerController.GetIndexInGrid(this);
            playerController.photonView.RPC("LinkStartByGrid", RpcTarget.All, gridTilePos.x, gridTilePos.y);

            return;
        }

        // Check if the item type matches the last tile in the link
        if (lastTileInLink && (currentItem.type == lastTileInLink.currentItem.type || currentItem.type < 0 || lastTileInLink.currentItem.type < 0))
        {
            if (connectUp && connectUp == lastTileInLink) // Connect FROM the tile above this one
            {
                goodLink = true;

                connector.localEulerAngles = Vector3.forward * 90;
            }
            else if (connectDown && connectDown == lastTileInLink) // Connect FROM the tile below this one
            {
                goodLink = true;

                connector.localEulerAngles = Vector3.forward * 270;
            }
            else if (connectLeft && connectLeft == lastTileInLink) // Connect FROM the tile left of this one
            {
                goodLink = true;

                connector.localEulerAngles = Vector3.forward * 180;
            }
            else if (connectRight && connectRight == lastTileInLink) // Connect FROM the tile right of this one
            {
                goodLink = true;

                connector.localEulerAngles = Vector3.forward * 0;
            }
            else if ( GridManager.instance.GetAllowDiagonal() == true ) // Check diagonal connections
            {
                if (connectUp && connectUp.connectRight && connectUp.connectRight == lastTileInLink) // Connect FROM top right tile
                {
                    goodLink = true;

                    connector.localEulerAngles = Vector3.forward * 45;
                }
                else if (connectUp && connectUp.connectLeft && connectUp.connectLeft == lastTileInLink)  
                {
                    goodLink = true;

                    connector.localEulerAngles = Vector3.forward * 135;
                }
                else if (connectDown && connectDown.connectRight && connectDown.connectRight == lastTileInLink) // Connect FROM bottom right tile
                {
                    goodLink = true;

                    connector.localEulerAngles = Vector3.forward * 315;
                }
                else if (connectDown && connectDown.connectLeft && connectDown.connectLeft == lastTileInLink)
                {
                    goodLink = true;

                    connector.localEulerAngles = Vector3.forward * 225;
                }
            }
        }

        if ( goodLink == true )
        {
            playerController.linkType = currentItem.type;

            playerController.CheckSelectables();

            Vector2Int gridTilePos = playerController.GetIndexInGrid(this);
            playerController.photonView.RPC("LinkAddByGrid", RpcTarget.All, gridTilePos.x, gridTilePos.y);
        }
    }

    public void SetCurrentItem( GridItem setValue )
    {
        currentItem = setValue;
    }

    public GridItem GetCurrentItem()
    {
        return currentItem;
    }

    public void SetConnectorAnimator(bool setValue)
    {
        connectorAnimator.enabled = setValue;
    }

    public void Glow(float delay)
    {
        StartCoroutine(GlowCoroutine(delay));
    }

    public IEnumerator GlowCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        //LeanTween.cancel(glowImage.rectTransform);
        LeanTween.color(glowImage.rectTransform, new Color(1, 1, 1, 0.6f), 0.3f).setOnComplete(()=>
        {
            LeanTween.color(glowImage.rectTransform, Color.clear, 0.2f);
        });
    }

    public void SetClickSize( float setValue )
    {
        buttonImage.transform.localScale = Vector3.one * setValue;
    }

    private void OnDrawGizmosSelected()
    {
        if (connectUp)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, connectUp.transform.position);
        }

        if (connectDown)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, connectDown.transform.position);
        }

        if (connectRight)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, connectRight.transform.position);
        }

        if (connectLeft)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, connectLeft.transform.position);
        }

    }


}


