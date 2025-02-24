using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtralLayer : MonoBehaviour
{
    [HideInInspector] public int indexLayerBase;
    public List<SpriteRendererLayer> spriteRenderers;
    public List<MeshRendererLayer> meshRenderers;
    public List<ParticlesRendererLayer> rendererParticles;
    public List<CanvasLayer> canvas;
    public List<LineRendererLayer> lineRenderers;

    public void Init(int indexLayerBase)
    {
        this.indexLayerBase = indexLayerBase;

        SetLayer(3, LayerGame.BIRD_FLY);
    }    

    public void SetLayer(int layer, string layerName)
    {
        if(spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                spriteRenderers[i].spriteRenderers.sortingLayerName = layerName;
                spriteRenderers[i].spriteRenderers.sortingOrder = spriteRenderers[i].firstLayer + indexLayerBase + layer + 1;
            }
        }

        if (meshRenderers != null)
        {
            for (int i = 0; i < meshRenderers.Count; i++)
            {
                meshRenderers[i].meshRenderer.sortingLayerName = layerName;
                meshRenderers[i].meshRenderer.sortingOrder = meshRenderers[i].firstLayer + indexLayerBase + layer + 1;
            }
        }

        if (rendererParticles != null)
        {
            for (int i = 0; i < rendererParticles.Count; i++)
            {
                rendererParticles[i].particlesRendererLayer.sortingLayerName = layerName;
                rendererParticles[i].particlesRendererLayer.sortingOrder = rendererParticles[i].firstLayer + indexLayerBase + layer + 1;
            }
        }

        if (canvas != null)
        {
            for (int i = 0; i < canvas.Count; i++)
            {
                canvas[i].canvas.sortingLayerName = layerName;
                canvas[i].canvas.sortingOrder = canvas[i].firstLayer + indexLayerBase + layer + 1;
            }
        }

        if (lineRenderers != null)
        {
            for (int i = 0; i < lineRenderers.Count; i++)
            {
                lineRenderers[i].lineRenderer.sortingLayerName = layerName;
                lineRenderers[i].lineRenderer.sortingOrder = lineRenderers[i].firstLayer + indexLayerBase + layer + 1;
            }
        }
    }    
}

[System.Serializable]
public class SpriteRendererLayer
{
    public int firstLayer;
    public SpriteRenderer spriteRenderers;
}

[System.Serializable]
public class MeshRendererLayer
{
    public int firstLayer;
    public MeshRenderer meshRenderer;
}

[System.Serializable]
public class ParticlesRendererLayer
{
    public int firstLayer;
    public ParticleSystemRenderer particlesRendererLayer;
}

[System.Serializable]
public class CanvasLayer
{
    public int firstLayer;
    public Canvas canvas;
}

[System.Serializable]
public class LineRendererLayer
{
    public int firstLayer;
    public LineRenderer lineRenderer;
}