using UnityEngine;

namespace Modules.MainMenu.UI
{
    public class SaveSlotsMenu : MonoBehaviour
    {
        public Transform slotParent;
        public GameObject slotPrefab;
        private bool slotsLoaded;

        void Start()
        {
            LoadSlots();
        }

        void LoadSlots()
        {
            if (slotsLoaded || slotParent == null || slotPrefab == null)
                return;

            slotsLoaded = true;
            ClearSlots();

            for (int i = 0; i < 4; i++)
            {
                GameObject obj = Instantiate(slotPrefab, slotParent);

                SaveSlotUI ui = obj.GetComponent<SaveSlotUI>();
                if (ui == null)
                    continue;

                if (SaveManager.Instance != null &&
                    SaveManager.Instance.saveFile != null &&
                    i < SaveManager.Instance.saveFile.slots.Count)
                {
                    //ui.Setup(SaveManager.Instance.saveFile.slots[i]);
                    ui.SetupEmpty(i);
                }
                else
                {
                    ui.SetupEmpty(i);
                }
            }
        }

        void ClearSlots()
        {
            for (int i = slotParent.childCount - 1; i >= 0; i--)
            {
                Destroy(slotParent.GetChild(i).gameObject);
            }
        }
    }
}
