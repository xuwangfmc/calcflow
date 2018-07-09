﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CalcFlowUI;

public class ButtonReenactableUnpress : ReenactableAction
{

    public override string key
    {
        get
        {
            return "buttonUnpress";
        }
    }

    public override void Reenact(LogInfo info, GameObject subject, PlaybackLogAction2 entry)
    {
        Button button;
        GameObject buttonPresser;
        if (subject != null)
        {
            button = subject.GetComponent<Button>();
            buttonPresser = PlaybackLogAction2.GetObject(info.GetValue<int>("buttonPresser"));
            button.UnpressButton(buttonPresser);
        }
        else
        {
            Debug.Log(entry.timeStamp + " " + entry.subjectKey);
        }
    }

}
