using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region Options

public class Option
{
    public DialogBox dialogBox = null;
    public string text = "noText";
    //Need a way to trigger wanted events when chosing a option
    public virtual void activate()
    {

    }

    public Option(string Text, DialogBox dBox)
    {
        text = Text;
        dialogBox = dBox;
    }
}

public class O_Exit : Option
{
    public override void activate()
    {
        base.activate();
        dialogBox.nextDialog(true);
    }

    public O_Exit(string Text, DialogBox dBox)
        : base(Text, dBox)
    {
        text = Text;
        dialogBox = dBox;
    }
}

#endregion

[System.Serializable]
public class dialog
{
    public string text = "noText";
    public bool choice = false;
    public List<Option> options;
    public Image portrait;

    public dialog(string Text, Sprite portraitSprite)
    {
        choice = false;
        text = Text;
        if (portraitSprite != null)
            portrait.sprite = portraitSprite;
        return;
    }

    public dialog(string Text, Sprite portraitSprite, List<Option> Options)
    {
        choice = true;
        text = Text;
        options = Options;
        if (portraitSprite != null)
            portrait.sprite = portraitSprite;
        return;
    }
}

public class DialogBox : MonoBehaviour
{

    public bool active = false;
    public Image dialogBackground = null;
    public Text dialogText = null;
    public Image dialogPortrait = null;
    int dialogIndex = -1;
    public List<dialog> dialogList = new List<dialog>();
    public dialogHolder dialogHolder;

    // Use this for initialization
    void Start()
    {
        CDB = this;
    }

    float letterSpeed = 25;
    float curLetterIndex = 0;
    // Update is called once per frame
    void Update()
    {
        dialogBackground.enabled = active;
        dialogText.enabled = active;
        dialogPortrait.enabled = active;
        if (active) {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) {
                print("Pressed enter");
                nextDialog(false);
            }

            if (dialogIndex >= 0 && dialogIndex < dialogList.Count) {
                if (curLetterIndex < dialogList[dialogIndex].text.Length) {
                    curLetterIndex += letterSpeed * Time.deltaTime;
                }
                dialogText.text = dialogList[dialogIndex].text.Substring(0, Mathf.FloorToInt(curLetterIndex));
            }
        }
    }

    void changeSelection(bool up)
    {
        //Move selection cursor
    }

    void activateSelection()
    {
        //Activate the currently selected option
    }

    public void nextDialog(bool force)
    {
        //Display the next dialog in the dialog list;
        dialogIndex += 1;
        if (dialogList.Count > dialogIndex) {
            curLetterIndex = 0;
        } else {
            endDialog();
        }
    }

    void endDialog()
    {
        dialogIndex = -1;
        active = false;
        PlayerController.allowMovement = true;
        dialogHolder.endDialog();
    }

    public static DialogBox CDB = null;
    public void startDialog(List<dialog> DialogList, dialogHolder DialogHolder)
    {
        if (active)
            return;
        dialogHolder = DialogHolder;
        active = true;
        dialogList = DialogList;
        dialogIndex = -1;
        PlayerController.allowMovement = false;
    }
}
