using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class TNT : GridElement
{
    public GameObject tntCircleParticle;
    public int explosionRange = 5;  // 5x5'lik alan
    public bool canCombo = false;



    private Vector2 destinationPositionForJoining = new Vector2();

    public void setDestinationPositionForJoining(Vector2 destinationPosition)
    {
        destinationPositionForJoining = destinationPosition;
    }

    public void setCanCombo(bool newCanCombo)
    {
        this.canCombo = newCanCombo;
    }
    public void onClick()
    {
        gridManager.clickedElement(this.gridPosition);
    }

    protected override void takeDamage()
    {
        OnExplode();
    }

    public override void OnExplode()
    {
        isExploding = true;
        // Tween ile patlama efekti (küçülme ve yok olma)
        particleComponent.Play();
        transform.DOScale(Vector3.zero, this.onExplodeDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => DestroyTnt());
        GameObject circleParticle = Instantiate(tntCircleParticle, gridManager.gridObject.transform);
        circleParticle.transform.localPosition = gameObject.transform.localPosition;
        Destroy(circleParticle, 1f);
    }

    public void comboJoining()
    {
        particleComponent.Play();
        DestroyElement();
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 50;
        DOTween.Sequence()
            .Append(gameObject.transform.DOLocalMove(destinationPositionForJoining, this.onExplodeDuration))  // 1 saniyede hedef pozisyona hareket
            .Join(gameObject.transform.DOScale(Vector3.zero, this.onExplodeDuration))     // Aynı anda küçülerek yok olma (veya hedefe girme)
            .OnComplete(() => Destroy(gameObject));
    }

    private void DestroyTnt()
    {
        DestroyElement();
        isExploding = false;
        Destroy(gameObject);
    }

    private int[,] getVirtualEffectedGridAsDistances(int range, int addDistance, int[,] virtualGrid)
    {
        int[,] virtualExplosingSpiralDistance = GenerateSpiralDistanceForTnt(range, range);
        List<Vector2> tnts = new List<Vector2>();

        for (int x = 0; x < range; x++)
        {
            for (int y = 0; y < range; y++)
            {
                Vector2 effectedPosition = new Vector2((gridPosition.x - ((range - 1) / 2)) + x, (gridPosition.y - ((range - 1) / 2)) + y);
                if (effectedPosition.x >= 0 && effectedPosition.x < gridManager.gridWidth)
                {
                    if(effectedPosition.y >= 0 && effectedPosition.y < gridManager.gridHeight)
                    {
                        if (virtualGrid[(int)effectedPosition.x, (int)effectedPosition.y] == -1)
                        {
                            if (gridManager.getElementType(effectedPosition) != GridElement.ElementType.Empty)
                            {
                                virtualGrid[(int)effectedPosition.x, (int)effectedPosition.y] = virtualExplosingSpiralDistance[x, y] + addDistance;
                                if (gridManager.getElementType(effectedPosition) == GridElement.ElementType.TNT)
                                {
                                    tnts.Add(effectedPosition);
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (Vector2 position in tnts)
        {
            gridManager.getTntComponent(position).getVirtualEffectedGridAsDistances(5, virtualGrid[(int)position.x, (int)position.y], virtualGrid);
        }

        return virtualGrid;
    }

    public List<(Vector2 position, int distance)> getDamagePositionsWithDistance(int range)
    {

        int[,] virtualGrid = new int[gridManager.gridWidth, gridManager.gridHeight];
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                virtualGrid[x, y] = -1;
            }
        }
        getVirtualEffectedGridAsDistances(range, 0, virtualGrid);

        List<(Vector2 position, int distance)> effectedList = new List<(Vector2, int)>();
        
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                if (virtualGrid[x,y] != -1)
                {
                    effectedList.Add((new Vector2(x, y), virtualGrid[x, y]));
                }
            }
        }

        return effectedList;
    }

    public static int[,] GenerateSpiralDistanceForTnt(int width, int height)
    {
        int[,] matrix = new int[width, height];
        int centerX = width / 2;
        int centerY = height / 2;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Her hücre için Manhattan mesafesi hesaplanıyor
                matrix[x, y] = Mathf.Max(Mathf.Abs(x - centerX), Mathf.Abs(y - centerY));
            }
        }
        return matrix;
    }
}