using UnityEngine;

public class Pheomone : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private float lifetime;
    private float currentLifeTime;
    private float alpha;
    Renderer rd;
    Color newColor;

    public void Initialize(Color color, float lifetime, string tag)
    {
        this.color = color;
        this.lifetime = lifetime;
        gameObject.tag = tag;
    }
    
    private void Start()
    {
        rd = GetComponent<Renderer>();
        currentLifeTime = lifetime;
    }

    private void Update()
    {
        alpha = currentLifeTime / lifetime;

        newColor = new Color(color.r, color.g, color.b, alpha);
        
        rd.material.color = newColor;

        if (currentLifeTime > 0 )
        {
            currentLifeTime -= Time.deltaTime;
        }
        else
        {
            // Dead Pheromone
            Destroy(this.gameObject);
        }

    }

    public void SetColor(Color color)
    {
        this.color = color;
    }
    public void SetLifeTime(float lifetime)
    {
        this.lifetime = lifetime;
    }
}
