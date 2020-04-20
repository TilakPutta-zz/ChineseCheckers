using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map Instance { set; get; }
    public GameObject hexPrefab;
    public int playerNumber;
    public int players;
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
    private Color selectedPegColor;
    private bool jumped;
    private bool moved;
    private Color highLight1 = Color.HSVToRGB(0.5f, 0.5f, 0.5f);
    private Color highLight2 = Color.HSVToRGB(0.499f, 0.499f, 0.499f);

    private int whoseTurn;
    private Client client;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();
        playerNumber = client.playerNumber;
        players = client.numberOfPlayers;
        GenerateBoard();
        GeneratePlayers(client.numberOfPlayers);
        whoseTurn = 0;
        setCamera(client.numberOfPlayers, client.playerNumber);
    }

    private void setCamera(int n, int p)
    {
        int modifiedN = (n == 5) ? n + 1: n;
        int cameraAngle = (playerNumber - 1) * (360 / modifiedN);
        cameraAngle = (cameraAngle == 90 || cameraAngle == 270) ? (cameraAngle - 30) : cameraAngle;
        Quaternion r = Camera.main.transform.rotation;
        Camera.main.transform.Rotate(new Vector3(r.x, r.y, cameraAngle));
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
                Debug.Log("whoseTrun: "+whoseTurn+", playerNumber: "+playerNumber);
                if (mr.name.StartsWith("Sphere") && ourHitObject.name.StartsWith("Peg"+(whoseTurn + 1)) && playerNumber == whoseTurn + 1)
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
                Hex hex = ourHitObject.GetComponent<Hex>();
                int clearRow = selectedPeg.pegPlacedRow;
                int clearCol = selectedPeg.pegPlacedCol;
                tryMove(hex.row, hex.col, playerNumber, true);
                clearHighLights(clearRow, clearCol);
                highLightMoves(hex.row, hex.col);
            }
        }
    }

    private void mouseOverPeg(GameObject ourHitObject)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedPeg == null)
            {
                selectedPeg = ourHitObject.GetComponent<Peg>();
                selectPeg(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol, playerNumber, true);
                highLightMoves(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol);
            }
            else
            {
                clearHighLights(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol);
                completeMove(selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol, playerNumber, true);
            }
        }
    }

    public void selectPeg(int row, int col, int incomingPlayerNumber, bool pass)
    {
        if (pass || playerNumber != incomingPlayerNumber)
        {
            if (pass)
            {
                string msg = "C_SEL_PEG|" + row + "|" + col + "|" + playerNumber;
                client.Send(msg);
            }
            selectedPeg = pegs[row, col];
            selectedPegColor = selectedPeg.GetComponentInChildren<MeshRenderer>().material.color;
            selectedPeg.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            Vector3 pos = selectedPeg.transform.position;
            selectedPeg.transform.position = new Vector3(pos.x, 0.5f, pos.z);
        }
    }

    public void completeMove(int row, int col, int incomingPlayerNumber, bool pass)
    {
        if (pass || incomingPlayerNumber != playerNumber)
        {
            if (pass)
            {
                string msg = "C_DONE_MOV|" + row + "|" + col + "|" + playerNumber;
                client.Send(msg);
            }
            selectedPeg = pegs[row, col];
            Vector3 pos = selectedPeg.transform.position;
            selectedPeg.transform.position = new Vector3(pos.x, 0.0f, pos.z);
            selectedPeg.GetComponentInChildren<MeshRenderer>().material.color = selectedPegColor;
            selectedPeg = null;
            moved = false;
            jumped = false;
            endTurn();
        }
    }

    private void endTurn()
    {
        whoseTurn = (whoseTurn + 1) % players;
    }

    public void tryMove(int row, int col, int incomingPlayerNumber, bool pass)
    {
        if (pass || incomingPlayerNumber != playerNumber)
        {
            if (pass)
            {
                string msg = "C_MOV_PEG|" + row + "|" + col + "|" + playerNumber;
                client.Send(msg);
            }
            
            GameObject ourHitObject = GameObject.Find("HEX_" + row + "_" + col);
            Vector3 pos = ourHitObject.transform.position;
            Hex hex = ourHitObject.GetComponent<Hex>();
            Debug.Log(hex.row+".."+hex.col);
            Vector3 pegPos = selectedPeg.transform.position;
            selectedPeg.transform.position = new Vector3(pos.x, pegPos.y, pos.z);
            pegs[selectedPeg.pegPlacedRow, selectedPeg.pegPlacedCol] = null;
            selectedPeg.pegPlacedRow = hex.row;
            selectedPeg.pegPlacedCol = hex.col;
            pegs[hex.row, hex.col] = selectedPeg;
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

    private void createPeg(GameObject player, int row, int col, float colPos, float offset)
	{
        GameObject go = Instantiate(player, new Vector3(colPos, 0, row * offset), Quaternion.identity) as GameObject;
        go.transform.SetParent(transform);
        Peg p = go.GetComponent<Peg>();
        //Debug.Log(row + "---" + col);
        p.pegRow = row;
        p.pegCol = col;
        p.pegPlacedRow = row;
        p.pegPlacedCol = col;
        pegs[row, col] = p;
    }

    private void FillFirstTriangle(GameObject player)
	{
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (col <= 6 + (row / 2) && col >= (6 + (row / 2) - row))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }
                    createPeg(player, row, col, colPos, zOffset);
                }
            }
        }
    }

    private void FillSecondTriangle(GameObject player)
	{
        for(int row = 4; row < 8; row++)
		{
            for (int col = 9; col < width; col++)
            {
                int d = row - 4;
                if (col <= 12 - ((d + 1) / 2) && col >= 9 + (d/2))
				{
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }

                    createPeg(player, row, col, colPos, zOffset);
                }
                    
    		}
        }
        
    }


    private void FillThirdTriangle(GameObject player)
    {
        for (int row = 9; row < 13; row++)
        {
            for (int col = 9; col < width; col++)
            {
                int d = 12 - row;
                if (col <= 12 - ( (d+1)/ 2) && col >= 9 + (d/2))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }

                    createPeg(player, row, col, colPos, zOffset);
                }

            }
        }

    }

    private void FillFourthTriangle(GameObject player)
    {
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
                    createPeg(player, row, col, colPos, zOffset);
                }
            }
        }
    }

    private void FillFifthTriangle(GameObject player)
    {
        for (int row = 9; row < 13; row++)
        {
            for (int col = 0; col < 4; col++)
            {
				int d = 12 - row;
				if (col <= 3 - ((d + 1) / 2) && col >= ((d)/2))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }

                    createPeg(player, row, col, colPos, zOffset);
                }

            }
        }

    }

    private void FillSixthTriangle(GameObject player)
    {
        for (int row = 4; row < 8; row++)
        {
            for (int col = 0; col < 4; col++)
            {
				int d = row-4;
				if (col <=  3 - ((d+1) / 2) && col >= ((d)/2))
                {
                    float colPos = col * xOffset;
                    if (row % 2 == 1)
                    {
                        colPos += xOffset / 2f;
                    }

                    createPeg(player, row, col, colPos, zOffset);
                }

            }
        }

    }

    private void GeneratePlayers(int players)
    {
        Debug.Log(players);
        switch (players)
        {
            case 2:
                {
                    Debug.Log("2");
                    // player 1
                    FillFirstTriangle(p1);

                    // player 2
                    FillFourthTriangle(p2);
                    break;
                }
            case 3:
                {
                    Debug.Log("3");
                    FillFirstTriangle(p1);
                    FillThirdTriangle(p2);
                    FillFifthTriangle(p3);
                    break;
                }
            case 4:
                {
                    Debug.Log("4");
                    FillFirstTriangle(p1);
                    FillSecondTriangle(p2);

                    FillFourthTriangle(p3);
                    FillFifthTriangle(p4);
                    break;
                }
            case 5:
                {
                    FillFirstTriangle(p1);
                    FillSecondTriangle(p2);

                    FillThirdTriangle(p3);

                    FillFourthTriangle(p4);
                    FillFifthTriangle(p5);


                    break;
                }
            case 6:
                {
                    FillFirstTriangle(p1);
                    FillSecondTriangle(p2);

                    FillThirdTriangle(p3);
                    FillFourthTriangle(p4);

                    FillFifthTriangle(p5);
                    FillSixthTriangle(p6);
                    break;
                }
            default:
                {
                    // player 1
                    FillFirstTriangle(p1);

                    // player 2
                    FillFourthTriangle(p2);
                    break;
                }
        }
    }

}
