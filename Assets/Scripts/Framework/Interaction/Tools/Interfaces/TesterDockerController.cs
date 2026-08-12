using System;
using UnityEngine;

namespace Framework.Interaction.Tools.Interfaces
{
    /// <summary>
    /// Representa el estado de los dos puertos RJ45 del tester.
    /// Los wrappers de Unity llaman a sus metodos publicos y el flujo del
    /// modulo solo necesita escuchar <see cref="BothEndsConnected"/>.
    /// </summary>
    public class TesterDockerController : MonoBehaviour
    {
        private bool masterConnected;
        private bool remoteConnected;
        private bool wereBothConnected;

        /// <summary>Se produce al pasar de uno a dos conectores acoplados.</summary>
        public event Action BothEndsConnected;

        /// <summary>Se produce despues de cualquier cambio de puerto.</summary>
        public event Action<bool, bool> ConnectionStateChanged;

        public bool IsMasterConnected => masterConnected;
        public bool IsRemoteConnected => remoteConnected;
        public bool AreBothConnected => masterConnected && remoteConnected;

        // Asignar desde When Select del wrapper de MasterSocket.
        public void OnMasterConnected() => SetMasterConnected(true);

        // Asignar desde When Unselect del wrapper de MasterSocket.
        public void OnMasterDisconnected() => SetMasterConnected(false);

        // Asignar desde When Select del wrapper de RemoteSocket.
        public void OnRemoteConnected() => SetRemoteConnected(true);

        // Asignar desde When Unselect del wrapper de RemoteSocket.
        public void OnRemoteDisconnected() => SetRemoteConnected(false);

        private void SetMasterConnected(bool connected)
        {
            if (masterConnected == connected)
                return;

            masterConnected = connected;
            NotifyConnectionStateChanged();
        }

        private void SetRemoteConnected(bool connected)
        {
            if (remoteConnected == connected)
                return;

            remoteConnected = connected;
            NotifyConnectionStateChanged();
        }

        private void NotifyConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(masterConnected, remoteConnected);

            bool areBothConnected = AreBothConnected;
            if (areBothConnected && !wereBothConnected)
                BothEndsConnected?.Invoke();

            wereBothConnected = areBothConnected;
        }
    }
}
