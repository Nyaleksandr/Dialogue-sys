using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTriger : MonoBehaviour
{
    public DialogView dialogueView;
    [SerializeField] private DialogueContainer dialogue;
    private bool isPlayerInRange = false;
    
    void Update() {
        if (isPlayerInRange && !DialogView.isDialogActive) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                dialogueView.ShowDialog(dialogue); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("OnTriggerEnter2D " + collision.gameObject.tag.Equals("Player"));
        if (collision.gameObject.tag.Equals("Player")) {
            isPlayerInRange = true;
        }
        if (collision.gameObject.name.Equals("MaeTrigger")) {

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Debug.Log("OnTriggerEnter2D " + collision.gameObject.tag.Equals("Player"));
        if (collision.gameObject.tag.Equals("Player")) {
            dialogueView.CloseDialog();
            isPlayerInRange = false;
        }
    }
}
