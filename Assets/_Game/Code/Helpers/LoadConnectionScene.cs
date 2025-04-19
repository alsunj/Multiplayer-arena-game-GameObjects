#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helpers
{
    public class LoadConnectionSceneMono : MonoBehaviour
    {
        private void Start()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Destroy(gameObject); // Optional: Destroy this loader if it's in the correct scene
                return;
            }

            SceneManager.LoadScene(0);
            Destroy(gameObject);
        }
    }
}

#endif