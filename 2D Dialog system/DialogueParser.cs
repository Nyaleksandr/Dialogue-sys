using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueParser : MonoBehaviour
{

    [SerializeField] private DialogueContainer dialogue;
    [SerializeField] private DialogView dialogueView;

    private string nextTargetNodeGuid = "";

    public DialogueParser(DialogView dialogueView, DialogueContainer dialogue)
    {
        this.dialogueView = dialogueView;  
        this.dialogue = dialogue;  
    }

    public void StartDialogue()
    {
        var narrativeData = dialogue.NodeLinks.First(); //Entrypoint node
        if(narrativeData == null) return;
        
        ProceedToNarrative(narrativeData.TargetNodeGuid);
    }

    public void ProceedToNarrative(string narrativeDataGUID)
    {
        
        var type = dialogue.GetNodeDatas().Find(x => x.Guid == narrativeDataGUID);

        if (type is EndNodeData)
        {
            dialogueView.CloseDialog();
            
            return;
        }
        
        if (type is EventNodeData)
        {
            ScriptableObject scriptableObject =
                dialogue.EventNodeDatas.Find(x => x.Guid == narrativeDataGUID).EventScript;
            
            
            return;
        }
        
        if (type is DialogueNodeData)
        {
            var actorName = dialogue.DialogueNodeDatas.Find(x => x.Guid == narrativeDataGUID).ActorName;
            var text = dialogue.DialogueNodeDatas.Find(x => x.Guid == narrativeDataGUID).DialogueText;
            var avatar = dialogue.DialogueNodeDatas.Find(x => x.Guid == narrativeDataGUID).FaceAvatar;
            dialogueView.PrintText(actorName, text, avatar);
        }

        var choices = dialogue.NodeLinks.Where(x => x.BaseNodeGuid == narrativeDataGUID);
        
        
//        dialogueText.text = ProcessProperties(text);

        var buttons = dialogueView.buttonContainer.GetComponentsInChildren<Button>();
        foreach (var t in buttons)
        {
            Destroy(t.gameObject);
        }

        dialogueView.GenerateKotaeButtons(choices);
    }
    
    /*private string ProcessProperties(string text)
    {
        foreach (var exposedProperty in dialogue.ExposedProperties)
        {
            text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
        }
        return text;
    }*/
}
