using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogHolder : MonoBehaviour
{
    [SerializeField]
    List<dialog> dList = new List<dialog>();

    void OnMouseDown()
    {
        foreach (dialog curD in dList) {
            string temp = curD.text;
            curD.text = temp;
        }
        print("Triggerd");
        DialogBox.CDB.startDialog(dList);
    }
}
