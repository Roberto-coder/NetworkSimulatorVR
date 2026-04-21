using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject Settings;
    public GameObject Login;
    public GameObject Registro;
    public GameObject SelectorPartidas;

    void Start()
    {
        ShowCanvas("Login");
    }

    public void ShowCanvas(string canvas)
    {
        MainMenu.SetActive(false);
        Settings.SetActive(false);
        Login.SetActive(false);
        Registro.SetActive(false);
        SelectorPartidas.SetActive(false);

        switch (canvas)
        {
            case "MainMenu":
                MainMenu.SetActive(true);
                break;
            
            case "Login":
                Login.SetActive(true);
                break;

            case "Registro":
                Registro.SetActive(true);
                break;

            case "SelectorPartidas":
                SelectorPartidas.SetActive(true);
                break;
            
            case "Settings":
                Settings.SetActive(true);
                break;
            
            case "Logout":
                Login.SetActive(true);
                break;
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Cerrando juego...");

        Application.Quit();
    }
    
    
}