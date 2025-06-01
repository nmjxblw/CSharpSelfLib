using System.Collections.Generic;
using System.IO;
using Netcode;
using StardewValley.Network;

namespace StardewValley;

public abstract class NetSynchronizer
{
	private const byte MessageTypeVar = 0;

	private const byte MessageTypeBarrier = 1;

	private Dictionary<string, INetObject<INetSerializable>> variables = new Dictionary<string, INetObject<INetSerializable>>();

	private Dictionary<string, HashSet<long>> barriers = new Dictionary<string, HashSet<long>>();

	protected void reset()
	{
		this.variables.Clear();
		this.barriers.Clear();
	}

	private HashSet<long> barrierPlayers(string name)
	{
		if (!this.barriers.TryGetValue(name, out var barrier))
		{
			barrier = (this.barriers[name] = new HashSet<long>());
		}
		return barrier;
	}

	private bool barrierReady(string name)
	{
		HashSet<long> playersReady = this.barrierPlayers(name);
		foreach (long id in Game1.otherFarmers.Keys)
		{
			if (!playersReady.Contains(id))
			{
				return false;
			}
		}
		return true;
	}

	protected bool shouldAbort()
	{
		if (Game1.client != null)
		{
			return Game1.client.timedOut;
		}
		return false;
	}

	public void barrier(string name)
	{
		this.barrierPlayers(name).Add(Game1.player.UniqueMultiplayerID);
		Game1.multiplayer.UpdateLate();
		this.sendMessage((byte)1, name);
		while (!this.barrierReady(name))
		{
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				return;
			}
		}
		Game1.hooks.AfterNewDayBarrier(name);
	}

	public bool isBarrierReady(string name)
	{
		if (!this.barrierReady(name))
		{
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
			return false;
		}
		return true;
	}

	public bool isVarReady(string varName)
	{
		if (!this.variables.ContainsKey(varName))
		{
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
			LocalMultiplayer.IsLocalMultiplayer();
			return false;
		}
		return true;
	}

	public T waitForVar<TField, T>(string varName) where TField : NetFieldBase<T, TField>, new()
	{
		while (!this.variables.ContainsKey(varName))
		{
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
		}
		return (this.variables[varName] as TField).Value;
	}

	public void sendVar<TField, T>(string varName, T value) where TField : NetFieldBase<T, TField>, new()
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);
		NetRoot<TField> root = new NetRoot<TField>(new TField());
		root.Value.Value = value;
		root.WriteFull(writer);
		this.variables[varName] = root.Value;
		stream.Seek(0L, SeekOrigin.Begin);
		this.sendMessage((byte)0, varName, stream.ToArray());
	}

	public bool hasVar(string varName)
	{
		return this.variables.ContainsKey(varName);
	}

	public abstract void processMessages();

	protected abstract void sendMessage(params object[] data);

	public void receiveMessage(IncomingMessage msg)
	{
		switch (msg.Reader.ReadByte())
		{
		case 0:
		{
			string varName = msg.Reader.ReadString();
			NetRoot<INetObject<INetSerializable>> root = new NetRoot<INetObject<INetSerializable>>();
			root.ReadFull(msg.Reader, default(NetVersion));
			this.variables[varName] = root.Value;
			break;
		}
		case 1:
		{
			string barrierName = msg.Reader.ReadString();
			this.barrierPlayers(barrierName).Add(msg.FarmerID);
			break;
		}
		}
	}
}
