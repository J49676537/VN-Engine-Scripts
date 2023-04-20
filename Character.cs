using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Character {

    // Root Object: Container for all images related to the character in the scene
    [HideInInspector] public RectTransform root;
    public string characterName;

    // Determines which type of image to render
    public Renderers renderers = new Renderers();
    public bool isMultiLayerCharacter {get{return renderers.renderer == null;}}

    [System.Serializable]
    public class Renderers {
        public RawImage renderer; // Single Layer Character
        public Image bodyRenderer; // Multi Layer Character Body
        public Image expressionRenderer; // Multi Layer Character Expression

        public List<Image> allBodyRenderers = new List<Image>();
        public List<Image> allExpressionRenderers = new List<Image>();
    }

    public Character (string _name, bool enableOnStart = true) {
        CharacterManager cm = CharacterManager.instance;
        GameObject prefab = Resources.Load("Prefabs/Character[" + _name + "]") as GameObject;

        // This creates the character on screen
        GameObject ob = GameObject.Instantiate(prefab, cm.characterPanel);
        root = ob.GetComponent<RectTransform>();
        characterName = _name;

        // Renders the layers, but sets them as invisible
        renderers.renderer = ob.GetComponentInChildren<RawImage>();
        if (isMultiLayerCharacter) {
            renderers.bodyRenderer = ob.transform.Find("BodyLayer").GetComponentInChildren<Image>();
            renderers.expressionRenderer = ob.transform.Find("ExpressionLayer").GetComponentInChildren<Image>();
            renderers.bodyRenderer.color = new Color(1,1,1,0);
            renderers.expressionRenderer.color = new Color(1,1,1,0);
            renderers.allBodyRenderers.Add(renderers.bodyRenderer);
            renderers.allExpressionRenderers.Add(renderers.expressionRenderer);
        }
    }

    // Cached Last Used Sprites
    Sprite lastBodySprite, lastFacialSprite = null;

    public void FadeIn(float speed = 3, bool smooth = false) {
        if (lastBodySprite != null && lastFacialSprite != null) {
            TransitionBody(lastBodySprite, speed, smooth);
            TransitionExpression(lastFacialSprite, speed, smooth);
        }
        else {
            lastBodySprite = renderers.bodyRenderer.sprite;
            lastFacialSprite = renderers.expressionRenderer.sprite;
            TransitionBody(lastBodySprite, speed, smooth);
            TransitionExpression(lastFacialSprite, speed, smooth);
        }
    }

    public void FadeOut(float speed = 3, bool smooth = false) {
        Sprite alphaSprite = Resources.Load<Sprite>("Images/Portraits/null");
        //lastBodySprite = renderers.bodyRenderer.sprite;
        //lastFacialSprite = renderers.expressionRenderer.sprite;
        TransitionBody(alphaSprite, speed, smooth);
        TransitionExpression(alphaSprite, speed, smooth);
    }

//######################################################################################################################################################
    Vector2 targetPosition;
    Coroutine moving;
    bool isMoving{get{return moving != null;}}
    public Vector2 anchorPadding {get{return root.anchorMax - root.anchorMin;}} // Should be (1,1)

    public void MoveTo(Vector2 target, float speed, bool smooth = true) {
        StopMoving();
        moving = CharacterManager.instance.StartCoroutine(Moving(target, speed, smooth));
    }

    public void StopMoving(bool arriveImmediately = false) {
        if (isMoving) {
            CharacterManager.instance.StopCoroutine(moving);
            if (arriveImmediately) {
                SetPosition(targetPosition);
            }
        }
        moving = null;
    }

    public void SetPosition(Vector2 target) {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        Vector2 minAnchorTarget = new Vector2(targetPosition.x, targetPosition.y);
        root.anchorMin = minAnchorTarget;
        root.anchorMax = root.anchorMin + padding;
    }

    IEnumerator Moving(Vector2 target, float speed, bool smooth) {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        Vector2 minAnchorTarget = new Vector2(targetPosition.x, targetPosition.y);
        speed *= Time.deltaTime;

        while(root.anchorMin != minAnchorTarget) {
            root.anchorMin = (!smooth) ? Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed) : Vector2.Lerp(root.anchorMin, minAnchorTarget, speed);
            root.anchorMax = root.anchorMin + padding;
            yield return new WaitForEndOfFrame();
        }

        StopMoving();
    }

//######################################################################################################################################################
    public Sprite GetSprite(string fileName) {
        Sprite sprite = Resources.Load<Sprite> ("Images/Portraits/" + characterName + "/" + fileName);
        return sprite;
    }

    public void SetBodyandExpression(string bodyFile, string expressionFile) {
        renderers.bodyRenderer.sprite = GetSprite(bodyFile);
        renderers.expressionRenderer.sprite = GetSprite(expressionFile);
    }

    // Transition Body Code Block
    bool isTransitioningBody {get{return transitioningBody != null;}}
    Coroutine transitioningBody = null;

    public void TransitionBody(Sprite sprite, float speed, bool smooth) {
        StopTransitioningBody();
        transitioningBody = CharacterManager.instance.StartCoroutine(TransitioningBody(sprite, speed, smooth));
    }

    void StopTransitioningBody() {
        if (isTransitioningBody) {
            CharacterManager.instance.StopCoroutine(transitioningBody);
        }
        transitioningBody = null;
    }

    public IEnumerator TransitioningBody(Sprite sprite, float speed, bool smooth) {
        for (int i = 0; i < renderers.allBodyRenderers.Count; i++) {
            Image image = renderers.allBodyRenderers [i];
            if (image.sprite == sprite) {
                renderers.bodyRenderer = image;
                break;
            }
        }

        if (renderers.bodyRenderer.sprite != sprite) {
            Image image = GameObject.Instantiate(renderers.bodyRenderer.gameObject, renderers.bodyRenderer.transform.parent).GetComponent<Image>();
            renderers.allBodyRenderers.Add(image);
            renderers.bodyRenderer = image;
            image.color = GlobalF.SetAlpha(image.color, 0f);
            image.sprite = sprite;
        }

        while (GlobalF.TransitionImages(ref renderers.bodyRenderer, ref renderers.allBodyRenderers, speed, smooth))
            yield return new WaitForEndOfFrame();

        StopTransitioningBody();
    }

    // Transition Expression Code Block
    bool isTransitioningExpression {get{return transitioningExpression != null;}}
    Coroutine transitioningExpression = null;

    public void TransitionExpression(Sprite sprite, float speed, bool smooth) {
        StopTransitioningExpression();
        transitioningExpression = CharacterManager.instance.StartCoroutine(TransitioningExpression(sprite, speed, smooth));
    }

    void StopTransitioningExpression() {
        if (isTransitioningExpression) {
            CharacterManager.instance.StopCoroutine(transitioningExpression);
        }
        transitioningExpression = null;
    }

    public IEnumerator TransitioningExpression(Sprite sprite, float speed, bool smooth) {
        for (int i = 0; i < renderers.allExpressionRenderers.Count; i++) {
            Image image = renderers.allExpressionRenderers [i];
            if (image.sprite == sprite) {
                renderers.expressionRenderer = image;
                break;
            }
        }

        if (renderers.expressionRenderer.sprite != sprite) {
            Image image = GameObject.Instantiate(renderers.expressionRenderer.gameObject, renderers.expressionRenderer.transform.parent).GetComponent<Image>();
            renderers.allExpressionRenderers.Add(image);
            renderers.expressionRenderer = image;
            image.color = GlobalF.SetAlpha(image.color, 0f);
            image.sprite = sprite;
        }

        while (GlobalF.TransitionImages(ref renderers.expressionRenderer, ref renderers.allExpressionRenderers, speed, smooth))
            yield return new WaitForEndOfFrame();

        StopTransitioningExpression();
    }

}