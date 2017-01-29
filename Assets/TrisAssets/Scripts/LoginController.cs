using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Logging;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;

public class LoginController : MonoBehaviour {

	//----------------------------------------------------------
	// Editor public properties
	//----------------------------------------------------------

	[Tooltip("IP address or domain name of the SmartFoxServer 2X instance")]
	public string Host = "127.0.0.1";

	[Tooltip("TCP port listened by the SmartFoxServer 2X instance; used for regular socket connection in all builds except WebGL")]
	public int TcpPort = 9933;

	[Tooltip("WebSocket port listened by the SmartFoxServer 2X instance; used for in WebGL build only")]
	public int WSPort = 8888;

	[Tooltip("Name of the SmartFoxServer 2X Zone to join")]
	public string Zone = "BasicExamples";

    //----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public InputField nameInput;
	public Button loginButton;
	public Text errorText;
    //public Text versionText;


    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private SmartFox sfs;

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Awake() {
       // 

        Application.runInBackground = true;
		
		#if UNITY_WEBPLAYER
		if (!Security.PrefetchSocketPolicy(Host, TcpPort, 500)) {
			Debug.LogError("Security Exception. Policy file loading failed!");
		}
		#endif

		// Enable interface
		enableLoginUI(true);
	}
	
	// Update is called once per frame
	void Update() {
		if (sfs != null)
			sfs.ProcessEvents();
	}

	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------

	public void OnLoginButtonClick() {
		enableLoginUI(false);
		
		// Set connection parameters
		ConfigData cfg = new ConfigData();
		cfg.Host = Host;
		#if !UNITY_WEBGL
		cfg.Port = TcpPort;
		#else
		cfg.Port = WSPort;
		#endif
		cfg.Zone = Zone;
		
		// Initialize SFS2X client and add listeners
		#if !UNITY_WEBGL
		sfs = new SmartFox();
		#else
		sfs = new SmartFox(UseWebSocket.WS);
		#endif
		
		// Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
		sfs.ThreadSafeMode = true;
		
		sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		
		// Connect to SFS2X
		sfs.Connect(cfg);
	}

	//----------------------------------------------------------
	// Private helper methods
	//----------------------------------------------------------
	
	private void enableLoginUI(bool enable) {
		nameInput.interactable = enable;
		loginButton.interactable = enable;
		errorText.text = "";
        //versionText.text = "PULA";
        //GameObject.Find("VersionText").GetComponent<Text>().text = "ffff";
    }
	
	private void reset() {
		// Remove SFS2X listeners
		// This should be called when switching scenes, so events from the server do not trigger code in this scene
		sfs.RemoveAllEventListeners();
		
		// Enable interface
		enableLoginUI(true);
	}

	//----------------------------------------------------------
	// SmartFoxServer event listeners
	//----------------------------------------------------------

	private void OnConnection(BaseEvent evt){
        //VersionText.text = "HERE";// UpdateVersionLabels.version;

        if ((bool)evt.Params["success"])
		{
			// Save reference to SmartFox instance; it will be used in the other scenes
			SmartFoxConnection.Connection = sfs;

			// Login
			sfs.Send(new Sfs2X.Requests.LoginRequest(nameInput.text));
		}
		else
		{
			// Remove SFS2X listeners and re-enable interface
			reset();

			// Show error message
			errorText.text = "Connection failed; is the server running at all?";
		}
	}
	
	private void OnConnectionLost(BaseEvent evt) {
		// Remove SFS2X listeners and re-enable interface
		reset();

		string reason = (string) evt.Params["reason"];

		if (reason != ClientDisconnectionReason.MANUAL) {
			// Show error message
			errorText.text = "Connection was lost; reason is: " + reason;
		}
	}
	
	private void OnLogin(BaseEvent evt) {
		// Remove SFS2X listeners and re-enable interface
		reset();

		// Load lobby scene
		Application.LoadLevel("Lobby");
    }
	
	private void OnLoginError(BaseEvent evt) {
		// Disconnect
		sfs.Disconnect();

		// Remove SFS2X listeners and re-enable interface
		reset();
		
		// Show error message
		errorText.text = "Login failed: " + (string) evt.Params["errorMessage"];
	}
}
