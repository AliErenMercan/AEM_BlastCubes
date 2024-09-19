using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.U2D;

public class Vase : Obstacle
{

    public Sprite defaultRenderer;
    public Sprite damagedRenderer;

    protected override void takeDamage()
    {
        damageDownCounter--;
        bool explodingControl = false;
        if (this.damageDownCounter == 1)
        {
            particleComponent.Play();
            this.gameObject.GetComponent<SpriteRenderer>().sprite = this.damagedRenderer;
        }
        else if(this.damageDownCounter == 0)
        {
            explodingControl = true;
            this.OnExplode();
        }
        isExploding = explodingControl;
    }
}
