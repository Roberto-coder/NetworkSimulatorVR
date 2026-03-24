using System;
using UnityEngine;

[Serializable]
public class UserProfile
{
    public string usuario;
    public string gmail;

    // referencia del save actual
    public string lastSaveDate;

    // slot activo
    public int activeSlot;

    // tiempo total jugado
    public float totalPlayTime;

    public UserProfile(string user,string email)
    {
        usuario = user;
        gmail = email;
        lastSaveDate = "";
        activeSlot = -1;
        totalPlayTime = 0;
    }
}