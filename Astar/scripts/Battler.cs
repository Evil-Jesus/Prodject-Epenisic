using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battler : MonoBehaviour {

    public class health
    {
        int hp = 100;
        int maxHp = 100;
    }

    public Stat vitality = new Stat(5, 99);
    public Stat def = new Stat(5, 99);
    public Stat agi = new Stat(5, 99);
    public Stat str = new Stat(5, 99);

    public void attack(Battler attacker)
    {
        //Apply damage to this battler from attacker and apply reflect / block
    }

    public void heal (int amount)
    {
        //Heals the battler, apply overheal.
    }

    public void pureDamage (int amount)
    {
        //deals damage to the battler in pure form. no effects.
    }
}
