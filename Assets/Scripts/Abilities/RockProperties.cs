using UnityEngine;

public class RockProperties : MonoBehaviour
{
    private float rockLifetime = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, rockLifetime);
    }

}
