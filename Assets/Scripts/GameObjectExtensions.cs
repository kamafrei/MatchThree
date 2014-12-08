using System;
using System.Text;
using UnityEngine;

public class GameObjectIsNullException : Exception
{
}

public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T> (this GameObject go) where T : Component
	{
#if DEBUG
		if (go == null)
			throw new GameObjectIsNullException ();
#else
        if(go == null)
            return null;
#endif

		var t = go.GetComponent<T> ();
		if (t == null)
			t = go.AddComponent<T> ();

		return t;
	}

    public static T FindChildComponent<T>(this GameObject obj, string childName) where T:Component
    {
        if (obj == null)
            return null;

        Transform child = obj.transform.Find(childName);

        if (child == null)
            return null;

        return child.gameObject.GetComponent<T>();
    }
    
    public static GameObject FindOrNull(this GameObject obj, string childName)
    {
        if(obj == null)
            return null;
        
        Transform child = obj.transform.Find(childName);
        
        if(child == null)
            return null;
        
        return child.gameObject;
    }

	public static void SetLayerRecursively (this GameObject obj, int newLayer)
	{
		if (obj == null)
			return;

		obj.layer = newLayer;
		var transforms = obj.GetComponentsInChildren<Transform> (true);
		foreach (Transform child in transforms) {
			child.gameObject.layer = newLayer;
		}
	}

	public static void SetLayerRecursively (this GameObject obj, string layerName)
	{
		SetLayerRecursively (obj, LayerMask.NameToLayer (layerName));
	}

	public static bool IsChildOf (this GameObject child, GameObject parent)
	{
		if (child == parent)
			return true;

		if (child.transform == parent.transform)
			return true;

		while (child.transform.parent != null) {
			if (child.transform.parent == parent.transform)
				return true;
			child = child.transform.parent.gameObject;
		}
		return false;
	}

    public static GameObject CreateChild(this GameObject parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.layer = parent.layer;
        return go;
    }
    public static GameObject InstantiateAsChild(this GameObject parent, GameObject prefab, bool resetPosition = true, bool resetRotation = true, bool resetScale = false)
    {
        if (prefab == null)
            return null;
        GameObject go = GameObject.Instantiate(prefab) as GameObject;
        go.transform.parent = parent.transform;
        if(resetPosition)
            go.transform.localPosition = Vector3.zero;
        if (resetRotation)
            go.transform.localRotation = Quaternion.identity;
        if (resetScale)
            go.transform.localScale = Vector3.one;
        return go;
    }

    public static T InstantiateAsChild<T>(this GameObject parent) where T:Component
    {
        return parent.InstantiateAsChild<T>(typeof(T).Name);
    }

    public static T InstantiateAsChild<T>(this GameObject parent, string name) where T : Component
    {
        GameObject go = new GameObject(name, typeof(T));
        go.transform.parent = parent.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.layer = parent.layer;
        return go.GetComponent<T>();
    }
	
    public static string Hierarchy (this GameObject go)
	{
		if (go == null)
			return "(null)";

		Transform t = go.transform;

		var sb = new StringBuilder (go.name);

		while (t.parent != null) {
			t = t.parent;
			sb.Insert (0, t.gameObject.name + "/");
		}

		return sb.ToString ();
	}

	public static T GetComponentInParents<T> (this GameObject go) where T : Component
	{
		while (go != null) {
			var t = go.GetComponent<T> ();
			if (t != null)
				return t;
			if (go.transform.parent != null)
				go = go.transform.parent.gameObject;
			else
				return null;
		}
		return null;
	}

    public static void AdjustAspect(this GameObject go)
    {
        float intendedAspect = 480.0f / 320.0f;

        var cached = go.GetComponent<AspectPositionCacher>();
        if (!cached)
        {
            cached = go.AddComponent<AspectPositionCacher>();
            cached.defaultAspect = intendedAspect;
            cached.defaultPosition = go.transform.localPosition;
        }
        

        // try getting screenAspect from screen. (dont always work in editor. bug?)
        float screenAspect = 0;
        if(Screen.height > 0)
        {
            screenAspect = Screen.width / (float)Screen.height;
        }

        if (screenAspect == cached.defaultAspect)
        {
            return;
        }

        if(screenAspect == 0) return;
        float aspect = (screenAspect / intendedAspect);

        go.transform.localPosition = Vector3.Scale(cached.defaultPosition, new Vector3(aspect, 1, 1));


    }

    public static Transform FindOrCreateChild(this GameObject go, string name)
    {
        Transform t = go.transform.FindChild(name);
        if (t == null)
        {
            GameObject ch = new GameObject(name);
            ch.transform.parent = go.transform;
            t = ch.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            ch.layer = go.layer;
        }
        return t;
    }
}

class AspectPositionCacher : MonoBehaviour
{
    public float defaultAspect = 480f / 320f;
    public Vector3 defaultPosition;
}