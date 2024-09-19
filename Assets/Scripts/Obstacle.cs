using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : GridElement
{
    public enum ObstacleType { Box, Stone, Vase }
    public ObstacleType obstacleType;
    public bool isDamageFromCube;
    public bool isDamageFromTnt;
    public int damageDownCounter;

    protected override void takeDamage()
    {
        damageDownCounter--;
        if (this.damageDownCounter == 0)
        {
            this.OnExplode();
        }
    }
    public override void OnExplode()
    {
        this.isExploding = true;
        particleComponent.Play();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 55;
        gameObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
        gameObject.transform.DOLocalMove(new Vector3(-2.35f, 5, 0), this.onExplodeDuration / 2)
            .SetEase(Ease.InBack)
            .OnComplete(() => jumpGoalPosition());
    }

    private void jumpGoalPosition()
    {
        gameObject.transform.DOScale(Vector3.zero, this.onExplodeDuration / 2)
            .SetEase(Ease.InBack)
            .OnComplete(() => DestroyElement());

    }

    protected override void DestroyElement()
    {
        isExploding = false;
        gridManager.removeElementCallback(this.gridPosition);
        gridManager.obstacleDestroyedCallback(this.obstacleType);
        Destroy(gameObject);
    }
}
