using UnityEngine;

public class RockProperties : MonoBehaviour
{
    public AudioSource rockHit;
    private static float rockLifetime = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyRock", rockLifetime);
    }

    // Update is called once per frame
    void Update() { }

    private void OnDestroy()
    {
        CancelInvoke("DestroyRock");
    }

    public void DestroyRock()
    {
        gameObject.transform.position = new Vector3(0, -10, 0);
        gameObject.SetActive(false);
        Rocks.MakeRockAvailable(gameObject);
        Destroy(this);
    }

    public static float GetRockLifetime()
    {
        return rockLifetime;
    }

    private void OnCollisionEnter(Collision other)
    {
        rockHit.PlayOneShot(rockHit.clip);
    }

}