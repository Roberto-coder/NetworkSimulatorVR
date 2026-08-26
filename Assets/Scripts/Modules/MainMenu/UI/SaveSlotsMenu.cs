using UnityEngine;

namespace Modules.MainMenu.UI
{
    public class SaveSlotsMenu : MonoBehaviour
    {
        public Transform slotParent;
        public GameObject slotPrefab;
        void OnEnable()
        {
            LoadSlots();
        }

        void LoadSlots()
        {
            if (slotParent == null || slotPrefab == null)
                return;

            ClearSlots();

            for (int i = 0; i < 4; i++)
            {
                GameObject obj = Instantiate(slotPrefab, slotParent);

                SaveSlotUI ui = obj.GetComponent<SaveSlotUI>();
                if (ui == null)
                    continue;

                SaveSlot slot = SaveManager.Instance?.saveFile?.slots.Find(item => item.slotID == i);
                if (slot != null && slot.data != null)
                {
                    ui.Setup(slot);
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
