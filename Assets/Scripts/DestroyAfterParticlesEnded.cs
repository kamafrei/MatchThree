using System.Collections;
using UnityEngine;

[AddComponentMenu("Utils/DestroyAfterParticlesEnded")]
internal class DestroyAfterParticlesEnded : MonoBehaviour
{
    private float destroyTime;

    private void Awake()
    {
        float particlesTime = GetMaxParticlesTime(gameObject);
        //Debug.Log("Added at " + gameObject.Hierarchy() + " destroy after " + particlesTime + " secs");
        destroyTime = Time.time + particlesTime;
    }

    void Update()
    {
        if (Time.time > destroyTime)
        {
            //Debug.Log("Destroying " + gameObject.Hierarchy(), gameObject);
            Destroy(gameObject);
        }
    }

    public static float GetMaxParticlesTime(GameObject go)
    {
        float destroyTime = 0;
        ParticleSystem[] pss = go.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss)
        {
            if (ps.loop)
            {
                Debug.LogWarning("DestroyAfterParticlesEnded: Looping particle system " + ps.gameObject.name +
                                 " never ends, ignoring!");
                continue;
            }
            destroyTime = Mathf.Max(destroyTime, ps.startDelay + ps.duration + ps.startLifetime);
        }
        return destroyTime;
    }
}