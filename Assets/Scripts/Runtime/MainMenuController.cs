using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Guia3
{
    /// <summary>
    /// Menu principal: entrar al reto o salir de la aplicacion.
    /// Se mantiene deliberadamente corto para que el usuario de prueba no tenga que leer nada
    /// antes de empezar la tarea cronometrada.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Guia3_Apilado";
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(Play);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(Play);
            if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
        }

        public void Play()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
