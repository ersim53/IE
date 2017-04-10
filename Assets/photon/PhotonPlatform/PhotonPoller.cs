//#define TEST_FAIL

using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using ExitGames.Client.Photon.LoadBalancing;
using UdpKit;
using System.Collections;
using Bolt;

public class PhotonPoller : Bolt.GlobalEventListener
{

	enum ConnectState
	{
		Idle = 0,
		JoinRoomPending = 1,
		DirectPending = 2,
		DirectFailed = 3,
		DirectSuccess = 4,
		RelayPending = 5,
		RelayFailed = 6,
		RelaySuccess = 7
	}

	class PhotonSession : UdpSession
	{
		internal Guid _id;
		internal Guid _socketPeerId;
		internal Int32 _playerCount;
		internal Int32 _playerLimit;
		internal String _roomName;

		public override Int32 ConnectionsCurrent {
			get { return _playerCount; }
		}

		public override Int32 ConnectionsMax {
			get { return _playerLimit; }
		}

		public override Boolean HasLan {
			get { return false; }
		}

		public override Boolean HasWan {
			get { return true; }
		}

		public override String HostName {
			get { return _roomName; }
		}

		public override Guid Id {
			get { return _id; }
		}

		public override Boolean IsDedicatedServer {
			get { return false; }
		}

		public override UdpEndPoint LanEndPoint {
			get { return default(UdpEndPoint); }
		}

		public override UdpSessionSource Source {
			get { return UdpSessionSource.Photon; }
		}

		public override UdpEndPoint WanEndPoint {
			get { return default(UdpEndPoint); }
		}

		public override UdpSession Clone ()
		{
			return (UdpSession)MemberwiseClone ();
		}
	}

	class PhotonLoadBalancingClient : LoadBalancingClient
	{
		public override void DebugReturn (DebugLevel level, string message)
		{
			if (level == DebugLevel.ERROR) {
				Debug.LogError (message);
			} else if (level == DebugLevel.WARNING) {
				Debug.LogWarning (message);
			} else if (level == DebugLevel.INFO) {
				Debug.Log (message);
			} else if (level == DebugLevel.ALL) {
				Debug.Log (message);
			}
		}
	}

	class PhotonPacket
	{
		public Byte[] Data;
		public Int32 Remote;

		public PhotonPacket ()
		{

		}

		public PhotonPacket (Int32 size)
		{
			Data = new byte[size];
		}
	}

	class SynchronizedQueue<T>
	{
		Queue<T> queue = new Queue<T> ();

		public void Clear ()
		{
			lock (queue) {
				queue.Clear ();
			}
		}

		public Int32 Count {
			get {
				lock (queue) {
					return queue.Count;
				}
			}
		}

		public void Enqueue (T item)
		{
			lock (queue) {
				queue.Enqueue (item);
			}
		}

		public bool TryDequeue (out T item)
		{
			lock (queue) {
				if (queue.Count > 0) {
					item = queue.Dequeue ();
					return true;
				}

				item = default(T);
				return false;
			}
		}
	}

	static PhotonPoller _instance;

	public static PhotonPoller Instance {
		get {
			return _instance;
		}
	}

	public static void CreatePoller (PhotonPlatformConfig config)
	{
		if (!_instance) {
			var pollers = FindObjectsOfType<PhotonPoller> ();
			if (pollers.Length == 0) {
				_instance = new GameObject (typeof(PhotonPoller).Name).AddComponent<PhotonPoller> ();
			}

			if (pollers.Length == 1) {
				_instance = pollers [0];
			}

			if (pollers.Length >= 2) {
				_instance = pollers [0];

				for (int i = 1; i < pollers.Length; ++i) {
					Destroy (pollers [i].gameObject);
				}
			}

			_instance._config = config;

			DontDestroyOnLoad (_instance);
		}
	}

	const Byte DATA_EVENT_CODE = 1;
	const Single ROOM_UPDATE_RATE = 5f;
	const Single ROOM_CREATE_TIMEOUT = 2f;
	const Single ROOM_JOIN_TIMEOUT = 10f;

	Timer _roomUpdateTimer;
	ClientState _state;
	ConnectState _connectState;
#pragma warning disable 414
    Coroutine _currentConnectRoutine;
#pragma warning restore 414
    PhotonPlatformConfig _config;
	PhotonLoadBalancingClient _lbClient;

	SynchronizedQueue<PhotonPacket> _packetPool = new SynchronizedQueue<PhotonPacket> ();
	SynchronizedQueue<PhotonPacket> _packetSend = new SynchronizedQueue<PhotonPacket> ();
	SynchronizedQueue<PhotonPacket> _packetRecv = new SynchronizedQueue<PhotonPacket> ();

	public LoadBalancingClient LoadBalancerClient {
		get { return _lbClient; }
	}

	public Int32 HostPlayerId {
		get {
			if (_lbClient == null) {
				return -1;
			}

			return _lbClient.CurrentRoom.MasterClientId;
		}
	}

	void Disconnect ()
	{
		if (_lbClient != null) {
			_lbClient.Disconnect ();
			_lbClient = null;
		}
	}

	void OnDestroy ()
	{
		Disconnect ();
	}

	protected new void OnDisable ()
	{
		base.OnDisable ();
		Disconnect ();
	}

	void Start ()
	{
		Disconnect ();

		_lbClient = new PhotonLoadBalancingClient ();
		_lbClient.OnEventAction += OnEventAction;
		_lbClient.OnOpResponseAction += OnOpResponseAction;
		_lbClient.OnStateChangeAction += OnStateChangeAction;

		_lbClient.AutoJoinLobby = true;

		_lbClient.AppId = _config.AppId;
		if (_config.UseOnPremise) {
			_lbClient.Connect (_config.OnPremiseServerIpAddress, _config.AppId, "1.0", "", null);
		} else { 
			_lbClient.ConnectToRegionMaster (_config.RegionMaster);
		}
	}

	void Update ()
	{
		if (_lbClient == null) {
			return;
		}

		// clear send/recv pools when getting connected
		if (_lbClient.State == ClientState.Joined && _lbClient.State != _state) {
			_packetSend.Clear ();
			_packetRecv.Clear ();
		}

		if (_lbClient.State == ClientState.JoinedLobby) {
			if (_roomUpdateTimer.Expired) {
				// update
				BoltNetwork.UpdateSessionList (FetchSessionListFromPhoton ());

				// 
				_roomUpdateTimer = new Timer (ROOM_UPDATE_RATE);
			}
		}

		// poll in/out
		PollIn ();
		PollOut ();

		// store state
		_state = _lbClient.State;
	}

	Map<Guid, UdpSession> FetchSessionListFromPhoton ()
	{
		var map = new Map<Guid, UdpSession> ();

		foreach (var r in LoadBalancerClient.RoomInfoList) {
			if (r.Value.IsOpen) {
				try {
					PhotonSession session = new PhotonSession ();
					session._roomName = r.Key;
					session._id = new Guid ((r.Value.CustomProperties ["UdpSessionId"] as String) ?? "");

					if (_config.UsePunchThrough) {
						try {
							session._socketPeerId = new Guid ((r.Value.CustomProperties ["SocketPeerId"] as String) ?? "");
						}
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            catch (Exception exn) {
							BoltLog.Exception (exn);
						}
#else
            catch { }
#endif
					}

					session._playerCount = r.Value.PlayerCount;
					session._playerLimit = r.Value.MaxPlayers;

					map = map.Add (session.Id, session);
				} catch (Exception exn) {
					BoltLog.Exception (exn);
				}
			}
		}

		return map;
	}

	public override void ConnectFailed (UdpEndPoint endpoint, IProtocolToken token)
	{
		if (_connectState == ConnectState.DirectPending) {
			ChangeState (ConnectState.DirectFailed);
		}

		if (_connectState == ConnectState.RelayPending) {
			ChangeState (ConnectState.RelayFailed);
		}
	}

	public override void Connected (BoltConnection connection)
	{
		if (_connectState == ConnectState.DirectPending) {
			ChangeState (ConnectState.DirectSuccess);
		}

		if (_connectState == ConnectState.RelayPending) {
			ChangeState (ConnectState.RelaySuccess);
		}
	}

	void OnStateChangeAction (ClientState obj)
	{
	}

	void OnOpResponseAction (OperationResponse obj)
	{
	}

	void OnEventAction (EventData obj)
	{

		switch (obj.Code) {
		// AppStats
		case 226:
			break;

		// GameList
		case 230:
			break;

		case 254:
			if (BoltNetwork.server != null) {
				BoltNetwork.server.Disconnect ();
			}
			break;

		case DATA_EVENT_CODE:
			var packetPlayerId = (int)obj.Parameters [ParameterCode.ActorNr];
			var packetContents = (byte[])obj.Parameters [ParameterCode.CustomEventContent];

			_packetRecv.Enqueue (new PhotonPacket {
				Data = packetContents,
				Remote = packetPlayerId
			});
			break;

		//default:
		//  Debug.LogErrorFormat("Unknown event code {0}", obj.Code);
		//  break;

		}
	}

	void PollIn ()
	{
		Boolean success;

		do {
			success = _lbClient.loadBalancingPeer.DispatchIncomingCommands ();
		} while (success);
	}

	void PollOut ()
	{
		PhotonPacket packet;

		while (_packetSend.TryDequeue (out packet)) {
			_lbClient.loadBalancingPeer.OpRaiseEvent (DATA_EVENT_CODE, packet.Data, false, new RaiseEventOptions {
				CachingOption = EventCaching.DoNotCache,
				SequenceChannel = 0,
				TargetActors = new int[1] { packet.Remote }
			});
		}

		_lbClient.loadBalancingPeer.SendOutgoingCommands ();
	}


	Byte[] CloneArray (Byte[] array, Int32 size)
	{
		var clone = new Byte[size];
		Buffer.BlockCopy (array, 0, clone, 0, size);
		return clone;
	}

	public void SetHostInfo (String servername, Boolean dedicated, Bolt.IProtocolToken protocolToken)
	{
		StartCoroutine (SetHostInfoRoutine (servername, dedicated, protocolToken));
	}

	IEnumerator SetHostInfoRoutine (String servername, Boolean dedicated, Bolt.IProtocolToken protocolToken)
	{
		var t = new Timer (ROOM_CREATE_TIMEOUT);

		while (_lbClient.State != ClientState.JoinedLobby && t.Waiting) {
			yield return null;
		}

		if (_lbClient.State != ClientState.JoinedLobby) {
			BoltLog.Error ("Can't call BoltNetwork.SetHostInfo when not in lobby");
			yield break;
		}

		// 
		var maxPlayers = dedicated ? BoltNetwork.maxConnections : BoltNetwork.maxConnections + 1;

		// check for null token and create one
		var token = protocolToken as PhotonHostInfoToken;
		if (token == null) {
			token = new PhotonHostInfoToken ();
		}

		if (token.CustomRoomProperties == null) {
			token.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable ();
		}

		token.CustomRoomProperties ["UdpSessionId"] = Guid.NewGuid ().ToString ();

		if (_config.UsePunchThrough) {
			token.CustomRoomProperties ["SocketPeerId"] = BoltNetwork.UdpSocket.SocketPeerId.ToString ();
		}

		// 
		_lbClient.OpCreateRoom (servername, new RoomOptions () {
			CustomRoomProperties = token.CustomRoomProperties,
			CustomRoomPropertiesForLobby = new string[] {
				"UdpSessionId",
				"SocketPeerId"
			},
			MaxPlayers = (byte)maxPlayers,
		}, null);
	}

	public Boolean JoinSession (UdpSession session, Bolt.IProtocolToken token)
	{
		if (session.Source == UdpSessionSource.Photon) {
			if (_connectState != ConnectState.Idle) {
				BoltLog.Error ("Already attempting connection to a photon room");
				return true;
			}

			if (_lbClient.State != ClientState.JoinedLobby) {
				BoltLog.Error ("Can't call BoltNetwork.Connect when not in lobby");
				return true;
			}

			_currentConnectRoutine = StartCoroutine (JoinSessionRoutine (session, token));
			return true;
		} else {
			return false;
		}
	}

	IEnumerator JoinSessionRoutine (UdpSession session, Bolt.IProtocolToken token)
	{
		Timer timer;

		ChangeState (ConnectState.JoinRoomPending);

		LoadBalancerClient.OpJoinRoom (session.HostName);

		// request new session list from zeus
		Zeus.RequestSessionList ();

		// 
		timer = new Timer (ROOM_JOIN_TIMEOUT);

		while (_lbClient.State != ClientState.Joined && timer.Waiting) {
			yield return null;
		}

		if (_lbClient.State != ClientState.Joined) {
			_currentConnectRoutine = null;
			BoltLog.Error ("Failed to join room");
			ChangeState (ConnectState.Idle);
			yield break;
		}

		// if we have a zeus session available for this
		if (_config.UsePunchThrough) {
			var s = (PhotonSession)session;
			if (s._socketPeerId != Guid.Empty) {
				UdpSession zeusSession;

				if (BoltNetwork.SessionList.TryFind (s._socketPeerId, out zeusSession) && zeusSession.Source == UdpSessionSource.Zeus) {
					ChangeState (ConnectState.DirectPending);

#if TEST_FAIL
          BoltNetwork.Connect(new UdpEndPoint(new UdpIPv4Address(0, 45, 0, 0), unchecked((ushort)-1)));
#else
					BoltNetwork.Connect (zeusSession);
#endif

					while (_connectState == ConnectState.DirectPending) {
						yield return null;
					}

					if (_connectState == ConnectState.DirectSuccess) {
						ChangeState (ConnectState.Idle);
						yield break;
					}
				}
			}
		}

		_currentConnectRoutine = null;

		ChangeState (ConnectState.RelayPending);
		BoltNetwork.Connect (new UdpEndPoint (new UdpIPv4Address ((uint)HostPlayerId), 0), token);

		while (_connectState == ConnectState.RelayPending) {
			yield return null;
		}

		if (_connectState == ConnectState.RelayFailed) {
			BoltLog.Error ("Connecting to photon room '{0}' failed", session.HostName);
		}

		ChangeState (ConnectState.Idle);
	}

	PhotonPacket AllocPacket (Int32 size)
	{
		PhotonPacket packet;

		if (_packetPool.TryDequeue (out packet)) {
			Array.Resize (ref packet.Data, size);
			return packet;
		} else {
			return new PhotonPacket (size);
		}
	}

	void ChangeState (ConnectState state)
	{
		BoltLog.Info ("Changing Connect State: {0} => {1}", _connectState, state);

		// update
		_connectState = state;
	}

	void FreePacket (PhotonPacket packet)
	{
		_packetPool.Enqueue (packet);
	}

	struct Timer
	{
		Single _expire;

		public Timer (Single wait)
		{
			_expire = Time.realtimeSinceStartup + wait;
		}

		public Boolean Expired {
			get {
				return Time.realtimeSinceStartup >= _expire;
			}
		}

		public Boolean Waiting {
			get {
				return Time.realtimeSinceStartup < _expire;
			}
		}
	}

	public Int32 RecvFrom (Byte[] buffer, Int32 bufferSize, ref UdpEndPoint endpoint)
	{
		PhotonPacket packet;

		if (_packetRecv.TryDequeue (out packet)) {
			// copy data
			Buffer.BlockCopy (packet.Data, 0, buffer, 0, packet.Data.Length);

			// set "sender"
			endpoint = new UdpEndPoint (new UdpIPv4Address ((uint)packet.Remote), 0);

			return packet.Data.Length;
		}

		return -1;
	}

	public Boolean RecvPoll ()
	{
		return _packetRecv.Count > 0;
	}

	public Int32 SendTo (Byte[] buffer, Int32 bytesToSend, UdpEndPoint endpoint)
	{
		PhotonPacket packet;
		packet = AllocPacket (bytesToSend);
		packet.Remote = (int)endpoint.Address.Packed;

		Buffer.BlockCopy (buffer, 0, packet.Data, 0, bytesToSend);

		_packetSend.Enqueue (packet);

		return bytesToSend;
	}

}
