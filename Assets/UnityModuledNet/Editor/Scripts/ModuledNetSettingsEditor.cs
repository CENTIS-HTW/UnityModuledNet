using UnityEngine;
using UnityEditor;
using CENTIS.UnityModuledNet.Managing;

namespace CENTIS.UnityModuledNet
{
    [CustomEditor(typeof(ModuledNetSettings))]
    [CanEditMultipleObjects]
    internal class ModuledNetSettingsEditor : Editor
    {
        private bool _packetSettingsIsVisible = false;
        private bool _debugSettingsIsVisible = false;

        // TODO : add descriptions to labels, was too lazy
        public override void OnInspectorGUI()
        {
            var settings = ModuledNetSettings.Settings;

            // user settings
            settings.Username = EditorGUILayout.TextField(new GUIContent("Username", "The username of the client."), settings.Username);
            settings.Color = EditorGUILayout.ColorField(new GUIContent("Color", "The color of the client."), settings.Color);
            settings.ReconnectAfterRecompile = EditorGUILayout.Toggle(
                new GUIContent("Reconnect after recompile", " Should the client reconnect after recompile?"), settings.ReconnectAfterRecompile);

            // packet frequency settings
            _packetSettingsIsVisible = EditorGUILayout.Foldout(_packetSettingsIsVisible, "Packet Settings", EditorStyles.foldoutHeader);
            if (_packetSettingsIsVisible)
            {
                EditorGUI.indentLevel++;
                settings.ServerConnectionTimeout = EditorGUILayout.IntField("Connection Timeout:", settings.ServerConnectionTimeout);
                settings.ServerHeartbeatDelay = EditorGUILayout.IntField("Heartbeat Delay:", settings.ServerHeartbeatDelay);
                settings.ServerDiscoveryTimeout = EditorGUILayout.IntField("ServerDiscovery Timeout:", settings.ServerDiscoveryTimeout);
                settings.MaxNumberResendReliablePackets = EditorGUILayout.IntField("Number of Resends of Reliable Packets: ", settings.MaxNumberResendReliablePackets);
                EditorGUI.indentLevel--;
            }

            // debug settings
            _debugSettingsIsVisible = EditorGUILayout.Foldout(_debugSettingsIsVisible, "Debug Settings", EditorStyles.foldoutHeader);
            if (_debugSettingsIsVisible)
            {
                EditorGUI.indentLevel++;
                settings.Debug = EditorGUILayout.Toggle(new GUIContent("Is Debug:", "Allows the display of debug messages."), settings.Debug);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Local IP:", ModuledNetManager.LocalIP);
                if (GUILayout.Button("Update IP Address"))
                    ModuledNetManager.UpdateIPAddress();
                EditorGUILayout.EndHorizontal();
                settings.Port = EditorGUILayout.IntField("Port:", settings.Port);
                settings.DiscoveryPort = EditorGUILayout.IntField("Server Discovery Port:", settings.DiscoveryPort);
                settings.MTU = EditorGUILayout.IntField("MTU:", settings.MTU);
                settings.RTT = EditorGUILayout.IntField("RTT:", settings.RTT);
                if (GUILayout.Button("Reset Server Discovery"))
                    ModuledNetManager.ResetServerDiscovery();
                EditorGUI.indentLevel--;
            }

            EditorUtility.SetDirty(settings);
        }
    }
}
