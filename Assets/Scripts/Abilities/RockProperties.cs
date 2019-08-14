using UnityEngine;

public class RockProperties : MonoBehaviour
{
    private float rockLifetime = 5.0f;
    // Start is called before the first frame update
    void Start ()
    {
        Invoke ("DestroyRock", 5.0f);
    }

    // Update is called once per frame
    void Update ()
    { }

    private void OnDestroy ()
    {
        CancelInvoke ("DestroyRock");
    }

    public void DestroyRock ()
    {
        Destroy (gameObject);
    }

}