using UnityEngine;

namespace Modules.MainMenu.UI
{
    public class SaveSlotsMenu : MonoBehaviour
    {
        public Transform slotParent;
        public GameObject slotPrefab;

        void Start()
        {
            LoadSlots();
        }

        void LoadSlots()
        {
            for(int i = 0; i < 4; i++)
            {
                Debug.Log("Creando slot " + i);
                GameObject obj = Instantiate(slotPrefab, slotParent);

                SaveSlotUI ui = obj.GetComponent<SaveSlotUI>();

                if(i < SaveManager.Instance.saveFile.slots.Count)
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
    }
}