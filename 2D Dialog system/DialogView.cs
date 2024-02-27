using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class DialogView : MonoBehaviour
{

    public Button choicePrefab;
    public GameObject buttonContainer;
    
    public Text npc_name;
    public TMP_Text dialogText;
    public Image FaceAvatar;
    public float printSpeed = 0.05f;
    private CanvasGroup canvasGroup;
    
    private DialogueParser dialogueParser;

    public static bool isDialogActive = false;

    private int actveMessage = 0;
    public ArrayList currentMessages = new ArrayList();

    private void Awake() {
        //При пробуждении объекта очищать текст и аватарки
        dialogText.text = "";
    }

    private void Start() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        if (isDialogActive) {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextMessage();
            }
        }
    }

    public void SetAvatarAndNamae(Sprite avatar, string namae) {

    }

    public void SetCharacterSprite(Sprite avatar, string namae) {

    }

    public void ShowDialog(DialogueContainer dialogueContainer)
    {
        isDialogActive = true;
        canvasGroup.alpha = 1;
        
        dialogueParser = new DialogueParser(this, dialogueContainer);
        dialogueParser.StartDialogue();
    }

    public void CloseDialog() {
        isDialogActive = false;
        StopAllCoroutines();
        canvasGroup.alpha = 0;
        currentMessages.Clear();
    }

    private void NextMessage() {
        if(currentMessages.Count > 0)
        {
            dialogueParser.ProceedToNarrative(currentMessages[currentMessages.Count-1].ToString());
            
        } else {
            //Kaiwa owarimasita
        }
    }
    
    public void GenerateKotaeButtons(IEnumerable<NodeLinkData> choices)
    {
        //Генерим кнопку что бы кинуть на неё фокус и не кликая пробел не сделать случайный выбор в диалоге
        var but = Instantiate(choicePrefab, buttonContainer.transform );
        but.GetComponentInChildren<TMP_Text>().text = "";
        but.GetComponent<RawImage>().color = Color.clear;
        but.Select();
        
        foreach (var choice in choices)
        {
            var button = Instantiate(choicePrefab, buttonContainer.transform);
            //button.GetComponentInChildren<Text>().text = ProcessProperties(choice.PortName);
            button.GetComponentInChildren<TMP_Text>().text = choice.PortName;
            button.onClick.AddListener(() => dialogueParser.ProceedToNarrative(choice.TargetNodeGuid));
        }
        
    }

    public void PrintText(string name, string text, Sprite faceAvatar) {
        npc_name.text = name;
        FaceAvatar.sprite = faceAvatar;
        StopAllCoroutines();//проверить не дропает ли вообще все корутины в проекте
        StartCoroutine(PrintTextCoroutine(text));
    }

    private IEnumerator PrintTextCoroutine(string text) {
        dialogText.text = "";
        string originalText = text;
        string displayedText = "";
        int alphaIndex = 0;

        foreach (char c in text.ToCharArray()) {


            alphaIndex++;
            //dialogText.text += c;

            dialogText.text = originalText;
            displayedText = dialogText.text.Insert(alphaIndex, "<color=#00000000>");
            dialogText.text = displayedText;
            //yield return null;//каждый кадр
            yield return new WaitForSecondsRealtime(printSpeed);
        }
    }
}
