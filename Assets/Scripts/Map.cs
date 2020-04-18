using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject hexPrefab;
    public int players = 2;
    public GameObject p1;
    public GameObject p2;
    public GameObject p3;
    public GameObject p4;
    public GameObject p5;
    public GameObject p6;
    public Peg[,] pegs = new Peg[17, 13];

    int width = 13;
    int height = 17;

    float xOffset = 0.882f;
    float zOffset = 0.764f;

    private Peg selectedPeg;
    private bool jumped;
    private bool moved;
    private Color highLight1 = Color.HSVToRGB(0.5f, 0.5f, 0.5f);
    private Color highLight2 = Color.HSVToRGB(0.499f, 0.499f, 0.499f);
    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
        GeneratePlayers(players);
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject ourHitObject = hitInfo.collider.transform.parent.gameObject;
            //Debug.Log("hit info:" + ourHitObject.name);

            if (Input.GetMouseButtonDown(0))
            {
                MeshRenderer mr = ourHitObject.GetComponentInChildren<MeshRenderer>();
                //Debug.Log(mr.name);
                if (mr.name.StartsWith("Sphere"))
                {
                    mouseOverPeg(ourHitObject);
                }
                else if (mr.name.StartsWith("Hexagon"))
                {
                    mouseOverHex(ourHitObject);
                }
            }
        }
    }

    private bool isHighLighted(GameObject ourHitObject)
    {
        Material mat = ourHitObject.GetComponentInChildren<MeshRenderer>().material;
        return mat.color == highLight1 || mat.color == highLight2;
    }

    private bool isMoved(GameObject ourHitObject)
    {
        Material mat = ourHitObject.GetComponentInChildren<MeshRenderer>().material;
        return mat.color == highLight1;
    }

    private bool isJumped(GameObject ourHitObject)
    {
        Material mat = ourHitObject.GetComponentInChildren<MeshRenderer>().material;
        return mat.color == highLight2;
    }

    private void mouseOverHex(GameObject ourHitObject)
    {
        if (selectedPeg != null && isHighLighted(ourHitObject))
        {
            if (isMoved(ourHitObject))
            {
                moved = true;
            }
            else if (isJumped(ourHitObject))
            {
                jumped = true;
            }
            Vector3 pos = ourHitObject.transform.position;
            Hex hex = ourHitObject.GetComponent<Hex>();
            Vector3 pegPos = selectedPeg.transform.position;
            selectedPeg.transform.position = new Vector3(pos.x, pegPos.y, pos.z);
            clearHighLights(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol);
            pegs[selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol] = null;
            selectedPeg.pegPlacedRow = hex.row;
            selectedPeg.pegPlacedCol = hex.col;
            pegs[hex.row, hex.col] = selectedPeg;
            highLightMoves(hex.row, hex.col);
        }
    }

    private void mouseOverPeg(GameObject ourHitObject)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedPeg == null)
            {
                selectedPeg = ourHitObject.GetComponent<Peg>();
                Vector3 pos = selectedPeg.transform.position;
                selectedPeg.transform.position = new Vector3(pos.x, 0.5f, pos.z);
                highLightMoves(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol);
            }
            else
            {
                Vector3 pos = selectedPeg.transform.position;
                selectedPeg.transform.position = new Vector3(pos.x, 0.0f, pos.z);
                clearHighLights(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol);
                selectedPeg = null;
                moved = false;
                jumped = false;
            }
        }
    }

    private GameObject getUpperLeft(int row, int col)
    {
        if (row % 2 == 0)
        {
            return GameObject.Find("HEX_" + (row + 1) + "_" + (col - 1));
        }
        else
        {
            return GameObject.Find("HEX_" + (row + 1) + "_" + (col));
        }
    }

    private GameObject getUpperRight(int row, int col)
    {
        if (row % 2 == 0)
        {
            return GameObject.Find("HEX_" + (row + 1) + "_" + (col));
        }
        else
        {
            return GameObject.Find("HEX_" + (row + 1) + "_" + (col + 1));
        }
    }

    private GameObject getLowerLeft(int row, int col)
    {
        if (row % 2 == 0)
        {
            return GameObject.Find("HEX_" + (row - 1) + "_" + (col - 1));
        }
        else
        {
            return GameObject.Find("HEX_" + (row - 1) + "_" + (col));
        }
    }

    private GameObject getLowerRight(int row, int col)
    {
        if (row % 2 == 0)
        {
            return GameObject.Find("HEX_" + (row - 1) + "_" + (col));
        }
        else
        {
            return GameObject.Find("HEX_" + (row - 1) + "_" + (col + 1));
        }
    }

    private bool isValidMove(int row, int col)
    {
        return pegs[row, col] == null && !jumped && !moved;
    }

    private bool isValidJump(int row, int col)
    {
        return pegs[row, col] == null && !moved;
    }

    private void highLightMoves(int row, int col)
    {
        //left side
        GameObject left = GameObject.Find("HEX_" + row + "_" + (col - 1));
        if (left != null)
        {
            if (isValidMove(row, col - 1))
            {
                left.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[row, col-1] != null)
            {
                left = GameObject.Find("HEX_" + row + "_" + (col - 2));
                if (left != null)
                {
                    if (isValidJump(row, col - 2))
                    {
                        left.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }

                }
            }
        }

        // right side
        GameObject right = GameObject.Find("HEX_" + (row) + "_" + (col+1));
        if (right != null)
        {
            if (isValidMove(row, col+1))
            {
                right.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[row, col+1] != null)
            {
                right = GameObject.Find("HEX_" + (row) + "_" + (col + 2));
                if (right != null)
                {
                    if (isValidJump(row, col + 2))
                    {
                        right.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }

                }
            }
        }

        // upper-left side
        GameObject upperLeft = getUpperLeft(row, col);
        if (upperLeft != null)
        {
            Hex hex = upperLeft.GetComponent<Hex>();
            if (isValidMove(hex.row, hex.col))
            {
                upperLeft.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[hex.row, hex.col] != null)
            {
                upperLeft = getUpperLeft(hex.row, hex.col);
                if (upperLeft != null)
                {
                    hex = upperLeft.GetComponent<Hex>();
                    if (isValidJump(hex.row, hex.col))
                    {
                        upperLeft.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }
                }
            }
        }

        // upper-right side
        GameObject upperRight = getUpperRight(row, col);
        if (upperRight != null)
        {
            Hex hex = upperRight.GetComponent<Hex>();
            if (isValidMove(hex.row, hex.col))
            {
                upperRight.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[hex.row, hex.col] != null)
            {
                upperRight = getUpperRight(hex.row, hex.col);
                if (upperRight != null)
                {
                    hex = upperRight.GetComponent<Hex>();
                    if (isValidJump(hex.row, hex.col))
                    {
                        upperRight.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }
                }
            }
        }

        // lower-left side
        GameObject lowerLeft = getLowerLeft(row, col);
        if (lowerLeft != null)
        {
            Hex hex = lowerLeft.GetComponent<Hex>();
            if (isValidMove(hex.row, hex.col))
            {
                lowerLeft.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[hex.row, hex.col] != null)
            {
                lowerLeft = getLowerLeft(hex.row, hex.col);
                if (lowerLeft != null)
                {
                    hex = lowerLeft.GetComponent<Hex>();
                    if (isValidJump(hex.row, hex.col))
                    {
                        lowerLeft.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }
                }
            }
        }

        // lower-right side
        GameObject lowerRight = getLowerRight(row, col);
        if (lowerRight != null)
        {
            Hex hex = lowerRight.GetComponent<Hex>();
            if (isValidMove(hex.row, hex.col))
            {
                lowerRight.GetComponentInChildren<MeshRenderer>().material.color = highLight1;
            }
            else if (pegs[hex.row, hex.col] != null)
            {
                lowerRight = getLowerRight(hex.row, hex.col);
                if (lowerRight != null)
                {
                    hex = lowerRight.GetComponent<Hex>();
                    if (isValidJump(hex.row, hex.col))
                    {
                        lowerRight.GetComponentInChildren<MeshRenderer>().material.color = highLight2;
                    }
                }
            }
        }
    }

    private void clearHighLights(int row, int col)
    {
        //left side
        GameObject left = GameObject.Find("HEX_" + row + "_" + (col - 1));
        if (left != null)
        {
            left.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            left = GameObject.Find("HEX_" + row + "_" + (col - 2));
            if (left != null)
            {
                left.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
        // right side
        GameObject right = GameObject.Find("HEX_" + (row) + "_" + (col + 1));
        if (right != null)
        {
            right.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            right = GameObject.Find("HEX_" + (row) + "_" + (col + 2));
            if (right != null)
            {
                right.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }

        // upper-left side
        GameObject upperLeft = getUpperLeft(row, col);
        if (upperLeft != null)
        {
            Hex hex = upperLeft.GetComponent<Hex>();
            upperLeft.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            upperLeft = getUpperLeft(hex.row, hex.col);
            if (upperLeft != null)
            {
                upperLeft.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }

        // upper-right side
        GameObject upperRight = getUpperRight(row, col);
        if (upperRight != null)
        {
            Hex hex = upperRight.GetComponent<Hex>();
            upperRight.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            upperRight = getUpperRight(hex.row, hex.col);
            if (upperRight != null)
            {
                upperRight.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }

        // lower-left side
        GameObject lowerLeft = getLowerLeft(row, col);
        if (lowerLeft != null)
        {
            Hex hex = lowerLeft.GetComponent<Hex>();
            lowerLeft.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            lowerLeft = getLowerLeft(hex.row, hex.col);
            if (lowerLeft != null)
            {
                lowerLeft.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }

        // lower-right side
        GameObject lowerRight = getLowerRight(row, col);
        if (lowerRight != null)
        {
            Hex hex = lowerRight.GetComponent<Hex>();
            lowerRight.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            lowerRight = getLowerRight(hex.row, hex.col);
            if (lowerRight != null)
            {
                lowerRight.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    private void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xPos = x * xOffset;
                if (y % 2 == 1)
                {
                    xPos += xOffset / 2f;
                }
                if (y < 4)
                {
                    if (x <= 6 + (y / 2) && x >= (6 + (y / 2) - y))
                    {
                        GameObject hex_go = (GameObject)Instantiate(hexPrefab, new Vector3(xPos, 0, y * zOffset), Quaternion.identity);
                        hex_go.name = "HEX_" + y + "_" + x;
                        hex_go.GetComponent<Hex>().row = y;
                        hex_go.GetComponent<Hex>().col = x;
                        hex_go.transform.SetParent(transform);
                    }
                }
                else if (y > 12)
                {
                    int mY = 3 - (y - 13);
                    if (x <= 6 + (mY / 2) && x >= (6 + (mY / 2) - mY))
                    {
                        GameObject hex_go = (GameObject)Instantiate(hexPrefab, new Vector3(xPos, 0, y * zOffset), Quaternion.identity);
                        hex_go.name = "HEX_" + y + "_" + x;
                        hex_go.GetComponent<Hex>().row = y;
                        hex_go.GetComponent<Hex>().col = x;
                        hex_go.transform.SetParent(transform);
                    }
                }
                else if (y < 9)
                {
                    int d = y - 4;
                    if (x <= 12 - ((d + 1) / 2) && x >= (d / 2))
                    {
                        GameObject hex_go = (GameObject)Instantiate(hexPrefab, new Vector3(xPos, 0, y * zOffset), Quaternion.identity);
                        hex_go.name = "HEX_" + y + "_" + x;
                        hex_go.GetComponent<Hex>().row = y;
                        hex_go.GetComponent<Hex>().col = x;
                        hex_go.transform.SetParent(transform);
                    }
                }
                else if (y > 8)
                {
                    int d = 12 - y;
                    if (x <= 12 - ((d + 1) / 2) && x >= (d / 2))
                    {
                        GameObject hex_go = (GameObject)Instantiate(hexPrefab, new Vector3(xPos, 0, y * zOffset), Quaternion.identity);
                        hex_go.name = "HEX_" + y + "_" + x;
                        hex_go.GetComponent<Hex>().row = y;
                        hex_go.GetComponent<Hex>().col = x;
                        hex_go.transform.SetParent(transform);
                    }
                }

            }
        }
    }

    private void GeneratePlayers(int players)
    {
        for (int row=0;row < 4; row++ )
        {
            for (int col=0;col < width; col++)
            {
                if (col <= 6 + (row / 2) && col >= (6 + (row / 2) - row))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }
                    GameObject go = Instantiate(p1, new Vector3(colPos, 0, row * zOffset), Quaternion.identity) as GameObject;
                    go.transform.SetParent(transform);
                    Peg p = go.GetComponent<Peg>();
                    p.pegRow = row;
                    p.pegCol = col;
                    p.pegPlacedRow = row;
                    p.pegPlacedCol = col;
                    pegs[row, col] = p;
                }
            }
        }

        for (int row = 13; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int mY = 3 - (row - 13);
                if (col <= 6 + (mY / 2) && col >= (6 + (mY / 2) - mY))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }
                    GameObject go = Instantiate(p2, new Vector3(colPos, 0, row * zOffset), Quaternion.identity) as GameObject;
                    go.transform.SetParent(transform);
                    Peg p = go.GetComponent<Peg>();
                    p.pegRow = row;
                    p.pegCol = col;
                    p.pegPlacedRow = row;
                    p.pegPlacedCol = col;
                    pegs[row, col] = p;
                }
            }
        }
    }
}
