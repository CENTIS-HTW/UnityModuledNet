using System.Linq;
using UnityEngine;
using UnityEditor;
using CENTIS.UnityModuledNet.Networking;
using CENTIS.UnityModuledNet.Managing;

namespace CENTIS.UnityModuledNet
{
    internal class ModuledNetManagerEditor : EditorWindow
    {
        #region private members

        private static ModuledNetSettings _settings;
        private Editor _settingsEditor;

        private Vector2 _serversViewPos;
        private Vector2 _clientsViewPos;
        private Vector2 _messagesViewPos;
        private Color[] _scrollViewColors = new Color[] { new(0.25f, 0.25f, 0.25f), new(0.23f, 0.23f, 0.23f) };
        
        private bool _isAutoscroll = false;

        private bool    _newServerOptionsIsVisible = true;
        private string  _newServerName = "New Server";

        private Texture2D _texture;
        private Texture2D Texture
        {
            get
            {
                if (_texture == null)
                    _texture = new(1, 1);
                return _texture;
            }
        }

        private GUIStyle _style = new();
        private Vector2 _scrollViewPosition = Vector2.zero;

        private const float ROW_HEIGHT = 20;

        #endregion

        [MenuItem("Window/ModuledNet/ModuledNet Manager")]
        public static void ShowWindow()
        {
            GetWindow(typeof(ModuledNetManagerEditor), false, "ModuledNet Manager");
        }

		public void OnEnable()
		{
            _settings = ModuledNetSettings.GetOrCreateSettings();
            _settingsEditor = Editor.CreateEditor(_settings);
            ModuledNetManager.OnSyncMessageAdded += AddSyncMessage;
            ModuledNetManager.OnConnectedClientListChanged += Repaint;
            ModuledNetManager.OnServerListChanged += Repaint;
        }

		public void CreateGUI()
		{
        }

        private void OnDestroy()
		{
            ModuledNetManager.OnSyncMessageAdded -= AddSyncMessage;
            ModuledNetManager.OnConnectedClientListChanged -= Repaint;
            ModuledNetManager.OnServerListChanged -= Repaint;
        }

        // TODO : add descriptions to labels, was too lazy
        void OnGUI()
        {
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			{
                GUILayout.Label("ModuledNet", EditorStyles.largeLabel);

                // user settings
                _settingsEditor.OnInspectorGUI();

                // module settings
                if (_settings.ModuleSettings.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("Module settings", EditorStyles.boldLabel);
                    foreach (var moduleSettings in _settings.ModuleSettings)
                    {
                        moduleSettings.DrawModuleSettingsFoldout();
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (!ModuledNetManager.IsServerDiscoveryActive)
				{
                    GUILayout.Label($"Server Discovery is inactive!");
                    if (GUILayout.Button(new GUIContent("Restart Server Discovery"), GUILayout.ExpandWidth(false)))
					{
                        ModuledNetManager.ResetServerDiscovery();
                        Repaint();
					}
				}

                if (!ModuledNetManager.IsConnected)
                {
                    // open servers list
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Open Servers: {ModuledNetManager.OpenServers?.Count}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent(_newServerOptionsIsVisible ? "-" : "+", "Create a New Server"),
                        GUILayout.Width(20), GUILayout.Height(20)))
                        _newServerOptionsIsVisible = !_newServerOptionsIsVisible;
                    EditorGUILayout.EndHorizontal();
                    _serversViewPos = EditorGUILayout.BeginScrollView(_serversViewPos,
                        EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(150));
                    {
                        if (ModuledNetManager.OpenServers?.Count == 0)
                            GUILayout.Label($"No Servers found!");

                        for (int i = 0; i < ModuledNetManager.OpenServers?.Count; i++)
                        {
                            OpenServerInformation server = ModuledNetManager.OpenServers[i];
                            EditorGUILayout.BeginHorizontal(GetScrollviewRowStyle(_scrollViewColors[i % 2]));
                            {
                                GUILayout.Label(server.Servername);
                                GUILayout.Label($"#{server.NumberConnectedClients}/{server.MaxNumberConnectedClients}");
                                if (GUILayout.Button(new GUIContent("Connect To Server"), GUILayout.ExpandWidth(false)))
                                    ModuledNetManager.ConnectToServer(server.IP);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    // new server options
                    if (_newServerOptionsIsVisible)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            GUILayout.Label("Create a New Server", EditorStyles.boldLabel);
                            _newServerName = EditorGUILayout.TextField("Servername:", _newServerName);
                            _settings.MaxNumberClients = (byte)EditorGUILayout.IntField("Max Clients:", _settings.MaxNumberClients);
                            if (GUILayout.Button(new GUIContent("Create")))
                                ModuledNetManager.CreateServer(_newServerName);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    // connected clients list
                    GUILayout.Label($"Current Server: {ModuledNetManager.CurrentServer?.Servername}");
                    _clientsViewPos = EditorGUILayout.BeginScrollView(_clientsViewPos, EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(150));
                    {
                        if (ModuledNetManager.ConnectedClients.Count == 0)
						{
                            GUILayout.Label($"There are no Clients in this server!");
						}
                        else
						{
                            Color defaultColor = _style.normal.textColor;
                            _style.alignment = TextAnchor.MiddleLeft;
                            for (int i = 0; i < ModuledNetManager.ConnectedClients?.Count; i++)
                            {
                                ClientInformation client = ModuledNetManager.ConnectedClients.Values.ElementAt(i);
                                EditorGUILayout.BeginHorizontal(GetScrollviewRowStyle(_scrollViewColors[i % 2]));
                                {
                                    _style.normal.textColor = client.Color;
                                    GUILayout.Label(client.ToString(), _style);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            _style.normal.textColor = defaultColor;
						}
                    }
                    EditorGUILayout.EndScrollView();

                    if (GUILayout.Button(new GUIContent(ModuledNetManager.IsHost ? "Close Server" : "Leave Server")))
                        ModuledNetManager.DisconnectFromServer();
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // sync messages
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Network Messages:");
                GUILayout.FlexibleSpace();
                _isAutoscroll = EditorGUILayout.Toggle(new GUIContent(" ", "Is Autoscrolling Messages"), _isAutoscroll);
                EditorGUILayout.EndHorizontal();
                _messagesViewPos = EditorGUILayout.BeginScrollView(_messagesViewPos,
                    EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(200));
				{
                    Color defaultColor = _style.normal.textColor;
                    for (int i = 0; i < ModuledNetManager.ModuledNetMessages.Count; i++)
                    {
                        ModuledNetMessage message = ModuledNetManager.ModuledNetMessages.ElementAt(i);
                        EditorGUILayout.BeginHorizontal(GetScrollviewRowStyle(_scrollViewColors[i % 2]));
                        {
                            switch (message.Severity)
							{
                                case ModuledNetMessageSeverity.Log:
                                    _style.normal.textColor = Color.white;
                                    break;
                                case ModuledNetMessageSeverity.LogWarning:
                                    _style.normal.textColor = Color.yellow;
                                    break;
                                case ModuledNetMessageSeverity.LogError:
                                    _style.normal.textColor = Color.red;
                                    break;
							}
                            GUILayout.Label($"[{message.Timestamp:H:mm:ss}]  {message.Message}", _style);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    _style.normal.textColor = defaultColor;
                }
                EditorGUILayout.EndScrollView();

            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }


        private GUIStyle GetScrollviewRowStyle(Color color)
        {
            Texture.SetPixel(0, 0, color);
            Texture.Apply();
            GUIStyle style = new();
            style.normal.background = Texture;
            style.fixedHeight = ROW_HEIGHT;
            return style;
        }

        private void AddSyncMessage()
		{
            if (_isAutoscroll) 
                _messagesViewPos = new(_messagesViewPos.x, ModuledNetManager.ModuledNetMessages.Count * ROW_HEIGHT);
            Repaint();
        }
    }
}
