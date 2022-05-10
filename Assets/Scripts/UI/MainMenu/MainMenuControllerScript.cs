using UnityEngine;

namespace Platformer.UI.MainMenu
{
    public class MainMenuControllerScript : MonoBehaviour
    {
        public GameObject MainMenuController;
        public GameObject SettingsController;

        public void Toggle()
        {
            MainMenuController.SetActive(!MainMenuController.activeInHierarchy);
            SettingsController.SetActive(!SettingsController.activeInHierarchy);
        }
    }
}