using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static bool s_isApplicationQuitting = false;

    public static T Instance
    {
        get
        {
            if (s_isApplicationQuitting)
            {
                return null;
            }

            if (s_instance == null)
            {
                var obj = GameObject.FindObjectOfType<T>();
                if (obj != null)
                {
                    s_instance = obj;
                }
                else
                {
                    var newObj = new GameObject(typeof(T).Name);
                    s_instance = newObj.AddComponent<T>();
                }
            }
            return s_instance;
        }
    }

    protected virtual void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        s_isApplicationQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }
}