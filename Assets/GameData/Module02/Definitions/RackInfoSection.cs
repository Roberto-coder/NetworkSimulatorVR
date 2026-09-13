using System;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Data
{
    [Serializable]
    public sealed class RackInfoSection
    {
        public string title;
        [TextArea(3, 8)] public string body;
        public Sprite image;
        public string reference;
    }
}
