using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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
