using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Module03_Diagnostics.Presentation
{
    // Referencias a controles guardados en el prefab: no crea UI durante Play.
    public sealed class NetworkAdministrationView : MonoBehaviour
    {
        public Button open, back, nextPort, nextAddress, nextPrefix, applyAddress, enablePort, disablePort, verify;
        public GameObject panel;
        public TMP_Text details;
        public Button previousPort, previousAddress;
        public TMP_Text portLabel, addressLabel;
        public RectTransform detailsRect;
        private string shownDetails;
        private void LateUpdate()
        {
            if (details == null || detailsRect == null || shownDetails == details.text) return;
            shownDetails = details.text;
            details.ForceMeshUpdate();
            detailsRect.sizeDelta = new Vector2(detailsRect.sizeDelta.x, Mathf.Max(330, details.preferredHeight + 24));
            detailsRect.anchoredPosition = Vector2.zero;
        }
        [System.NonSerialized] public int portIndex, addressIndex, prefixIndex;
    }
}
