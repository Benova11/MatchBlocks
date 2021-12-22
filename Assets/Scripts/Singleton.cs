using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

  static T instance;

  public static T Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindObjectOfType<T>();
        if (instance)
        {
          GameObject singleton = new GameObject(typeof(T).Name);
          instance = singleton.AddComponent<T>();
        }
      }
      return instance;
    }
  }


  public virtual void Awake()
  {
    if (instance == null)
    {
      instance = this as T;
      transform.parent = null;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }
}
