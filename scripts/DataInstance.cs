using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInstance : MonoBehaviour
{
    private static DataInstance instance;

    public Vector2 playerPosition;
    private bool hasSavedPosition = false;

    public int currentHearts;
    public int hp;
    public int currentKeys;

    public static DataInstance Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DataInstance");
                instance = go.AddComponent<DataInstance>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetPlayerPosition(Vector2 playerPos)
    {
        playerPosition = playerPos;
        hasSavedPosition = true;

        GameManager gm = FindObjectOfType<GameManager>();

        currentHearts = gm.currentHearts;
        hp = gm.hp;
        currentKeys = gm.currentKeys;
    }

    public Vector2 GetPlayerPosition()
    {
        return playerPosition;
    }

    public bool HasSavedPosition()
    {
        return hasSavedPosition;
    }
}
