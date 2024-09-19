using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;
using System;

public class Cube : GridElement
{
    public enum CubeColor { Blue, Red, Green, Yellow }
    public CubeColor cubeColor;
    public bool isClicked = false;
    public bool isTntCreatable = false;
    public Sprite defaultRenderer;
    public Sprite TntCreatableRenderer;
    public GameObject CubeObject;
    private Vector2 destinationPositionForJoining = new Vector2();
    public void setDestinationPositionForJoining(Vector2 destinationPosition)
    {
        destinationPositionForJoining = destinationPosition;
    }

    public void onClick()
    {
        gridManager.clickedElement(this.gridPosition);
    }

    public void setTntCreatable(bool newState)
    {
        this.isTntCreatable = newState;
        CubeObject.GetComponent<SpriteRenderer>().sprite = (this.isTntCreatable ? this.TntCreatableRenderer : this.defaultRenderer); 
    }
    protected override void takeDamage()
    {
        OnExplode();
    }

    public override void OnExplode()
    {
        isExploding = true;
        particleComponent.Play();

        if (isTntCreatable == true && isClicked == false)
        {
            transform.DOScale(gameObject.transform.localScale * 1.5f, this.onExplodeDuration/2)
            .SetEase(Ease.InBack)
            .OnComplete(() => tntCreateJoining());
        }
        else
        {
            transform.DOScale(Vector3.zero, this.onExplodeDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => DestroyCube());
        }
    }
    public void tntCreateJoining()
    {
        particleComponent.Play();
        DestroyElement();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 50;
        DOTween.Sequence()
            .Append(gameObject.transform.DOLocalMove(destinationPositionForJoining, this.onExplodeDuration/2))  // 1 saniyede hedef pozisyona hareket
            .Join(gameObject.transform.DOScale(Vector3.zero, this.onExplodeDuration/2))     // Aynı anda küçülerek yok olma (veya hedefe girme)
            .OnComplete(() => Destroy(gameObject));
    }

    private void DestroyCube()
    {
        isExploding = false;
        DestroyElement();
        Destroy(gameObject);
    }

    protected override void DestroyElement()
    {
        if(isClicked == true && isTntCreatable == true)
        {
            this.gridManager.removeCubeAddTntCallback(this.gridPosition);
        }
        else
        {
            gridManager.removeElementCallback(this.gridPosition);
        }
    }
}
