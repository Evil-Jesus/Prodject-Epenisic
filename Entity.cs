using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zentronic;

public class Stat
{
    public int lvl = 1;
    public int maxLvl = 99;

    public int totalExp = 0;
    public int exp = 0;

    public int expToNextLvl()
    {
        return Mathf.RoundToInt(10 * lvl * lvl);
    }

    public Stat(int Lvl, int MaxLvl)
    {
        lvl = Lvl;
        maxLvl = MaxLvl;
    }

    public void addExp(int Amount)
    {
        exp += Amount;
        totalExp += Amount;

        if (exp >= expToNextLvl()) {
            exp = 0;
            lvl += 1;
        }
    }
}

public class Entity : AStarAgent
{

    public string race = "noRace";
    public Stat str = new Stat(5, 99);
    public Stat def = new Stat(5, 99);
    public Stat agi = new Stat(5, 99);

    public List<Vec3di> curPath = new List<Vec3di>();
    public Vec3di pathEnd = new Vec3di();
    public bool allowDiagonals = true;
    public string entityType = "noType";

    // Use this for initialization
    void Start()
    {
        map = GameObject.Find("World").GetComponent<World>().navGrid;
        ownPosition = new Vec3di(5, 5, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ownPosition.x, ownPosition.y, 0);
        if (Input.GetKeyDown(KeyCode.Space)) {
            FindPath(pathEnd, curPath, true);
            Follow(curPath);

            str.addExp(50);
            print(str.expToNextLvl());
            print(str.exp);
            print(str.lvl);
        }
    }
}
