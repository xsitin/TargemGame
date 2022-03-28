using UnityEngine;

namespace Platformer.UI
{
    /// <summary>
    ///     A simple controller for switching between UI panels.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        public GameObject[] panels;

        private void OnEnable()
        {
            SetActivePanel(0);
        }

        public void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var g = panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }
    }
}