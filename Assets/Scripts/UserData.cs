using System;
using UnityEngine;

[Serializable]
public class UserProfile
{
    public string usuario;
    public string gmail;

    public UserProfile(string user,string email)
    {
        usuario = user;
        gmail = email;
    }
}