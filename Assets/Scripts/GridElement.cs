using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UIElements;

public abstract class GridElement : MonoBehaviour
{
    public enum ElementType { Empty, Cube, Obstacle, TNT }
    public ElementType elementType;
    public Vector2 gridPosition;
    public bool isFalldownable = true;
    public bool isExploding = false;
    public float onExplodeDuration = 0.5f;
    protected GridManager gridManager;

    public Sprite[] particleSprites;
    protected ParticleSystem particleComponent;

    private void Start()
    {
        // Particle System'in Texture Sheet Animation modülüne erişiyoruz
        particleComponent = gameObject.GetComponent<ParticleSystem>();
        var textureSheetAnimation = particleComponent.textureSheetAnimation;

        // Texture Sheet Animation'ın sprite listesi temizleniyor
        textureSheetAnimation.RemoveSprite(0);  // İlk sprite'ı kaldırıyoruz (varsa)

        // Yeni sprite'ları ekliyoruz
        for (int i = 0; i < particleSprites.Length; i++)
        {
            textureSheetAnimation.AddSprite(particleSprites[i]);
        }
    }


    public void setGridManager(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    protected virtual void DestroyElement()
    {
        gridManager.removeElementCallback(this.gridPosition);
    }

    //Parent Class for all grid objects
    public virtual void OnExplode(){}
    protected virtual void takeDamage(){}

    public virtual void fallDown(Vector2 targetPosition)
    {
        transform.DOLocalMove(targetPosition, 0.3f).SetEase(Ease.OutBounce);
    }
}
