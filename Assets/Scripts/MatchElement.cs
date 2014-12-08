using UnityEngine;

class MatchElement: MonoBehaviour
{
    public int kind;

    public bool isExploded = false;

    [SerializeField]
    private iTween.EaseType easeType = iTween.EaseType.easeInBack;

    [SerializeField]
    private GameObject explosionEffect;

    internal void SetPosInstant(Vector3 pos)
    {
        iTween.Stop(gameObject);
        transform.position = pos;
    }

    internal void Explode(float concurTime)
    {
        isExploded = true;
        CreateEffect();
        Destroy(gameObject);
    }

    private void CreateEffect()
    {
        if (explosionEffect == null)
            return;
        var go = Instantiate(explosionEffect, transform.position, Quaternion.identity) as GameObject;
        go.AddComponent<DestroyAfterParticlesEnded>();
    }

    internal MatchElement MoveTo(Vector3 pos, float concurTime)
    {
        iTween.Stop(gameObject);
        iTween.MoveTo(gameObject, iTween.Hash("position", pos, "easetype", easeType, "time", concurTime)); 
        return this;
    }

    internal void CreateCollider(Vector3 size)
    {
        var col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = Mathf.Max(size.x, size.y, size.z) * 0.5f;
    }
}
