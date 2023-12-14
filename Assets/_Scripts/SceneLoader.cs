using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class SceneLoader : PersistentSingleton<SceneLoader>
{
    [SerializeField] private Image progressFill;
    [SerializeField] private CanvasGroup canvasGroup;
    public void Load(string scene){
        StartCoroutine(LoadScene(scene));
    }

    IEnumerator LoadScene(string scene){
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        canvasGroup.blocksRaycasts = true;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f);
        operation.allowSceneActivation = false;
        float _timer = 0;
        while(!operation.isDone){
            while(_timer < 2f){
                progressFill.fillAmount = Mathf.Min(_timer/2f, operation.progress);

                _timer += Time.deltaTime;
                yield return null;
            }
            operation.allowSceneActivation = true;
            progressFill.fillAmount = operation.progress;
            yield return null;
        }
        yield return null;
        LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f);
        canvasGroup.blocksRaycasts = false;
    }
}
