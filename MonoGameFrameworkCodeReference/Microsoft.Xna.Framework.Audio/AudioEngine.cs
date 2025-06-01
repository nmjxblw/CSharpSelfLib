using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// Class used to create and manipulate code audio objects.
/// </summary> 
public class AudioEngine : IDisposable
{
	private readonly AudioCategory[] _categories;

	private readonly Dictionary<string, int> _categoryLookup = new Dictionary<string, int>();

	private readonly RpcVariable[] _variables;

	private readonly Dictionary<string, int> _variableLookup = new Dictionary<string, int>();

	private readonly RpcVariable[] _cueVariables;

	private readonly Stopwatch _stopwatch;

	private TimeSpan _lastUpdateTime;

	private readonly ReverbSettings _reverbSettings;

	private readonly RpcCurve[] _reverbCurves;

	internal List<Cue> ActiveCues = new List<Cue>();

	internal Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>();

	internal readonly RpcCurve[] RpcCurves;

	internal readonly object UpdateLock = new object();

	/// <summary>
	/// The current content version.
	/// </summary>
	public const int ContentVersion = 39;

	internal AudioCategory[] Categories => this._categories;

	/// <summary>
	/// Is true if the AudioEngine has been disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// This event is triggered when the AudioEngine is disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposing;

	internal RpcVariable[] CreateCueVariables()
	{
		RpcVariable[] clone = new RpcVariable[this._cueVariables.Length];
		Array.Copy(this._cueVariables, clone, this._cueVariables.Length);
		return clone;
	}

	/// <param name="settingsFile">Path to a XACT settings file.</param>
	public AudioEngine(string settingsFile)
		: this(settingsFile, TimeSpan.Zero, "")
	{
	}

	internal static Stream OpenStream(string filePath, bool useMemoryStream = false)
	{
		Stream stream = TitleContainer.OpenStream(filePath);
		if (useMemoryStream)
		{
			MemoryStream memStream = new MemoryStream();
			stream.CopyTo(memStream);
			memStream.Seek(0L, SeekOrigin.Begin);
			stream.Dispose();
			stream = memStream;
		}
		return stream;
	}

	/// <param name="settingsFile">Path to a XACT settings file.</param>
	/// <param name="lookAheadTime">Determines how many milliseconds the engine will look ahead when determing when to transition to another sound.</param>
	/// <param name="rendererId">A string that specifies the audio renderer to use.</param>
	/// <remarks>For the best results, use a lookAheadTime of 250 milliseconds or greater.</remarks>
	public AudioEngine(string settingsFile, TimeSpan lookAheadTime, string rendererId)
	{
		if (string.IsNullOrEmpty(settingsFile))
		{
			throw new ArgumentNullException("settingsFile");
		}
		using (Stream stream = AudioEngine.OpenStream(settingsFile))
		{
			using BinaryReader reader = new BinaryReader(stream);
			if (reader.ReadUInt32() != 1179862872)
			{
				throw new ArgumentException("XGS format not recognized");
			}
			reader.ReadUInt16();
			reader.ReadUInt16();
			_ = 42;
			reader.ReadUInt16();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadByte();
			uint numCats = reader.ReadUInt16();
			uint numVars = reader.ReadUInt16();
			reader.ReadUInt16();
			reader.ReadUInt16();
			uint numRpc = reader.ReadUInt16();
			uint numDspPresets = reader.ReadUInt16();
			uint numDspParams = reader.ReadUInt16();
			uint catsOffset = reader.ReadUInt32();
			uint varsOffset = reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			uint catNamesOffset = reader.ReadUInt32();
			uint varNamesOffset = reader.ReadUInt32();
			uint rpcOffset = reader.ReadUInt32();
			reader.ReadUInt32();
			uint dspParamsOffset = reader.ReadUInt32();
			reader.BaseStream.Seek(catNamesOffset, SeekOrigin.Begin);
			string[] categoryNames = AudioEngine.ReadNullTerminatedStrings(numCats, reader);
			this._categories = new AudioCategory[numCats];
			reader.BaseStream.Seek(catsOffset, SeekOrigin.Begin);
			for (int i = 0; i < numCats; i++)
			{
				this._categories[i] = new AudioCategory(this, categoryNames[i], reader);
				this._categoryLookup.Add(categoryNames[i], i);
			}
			reader.BaseStream.Seek(varNamesOffset, SeekOrigin.Begin);
			string[] varNames = AudioEngine.ReadNullTerminatedStrings(numVars, reader);
			List<RpcVariable> variables = new List<RpcVariable>();
			List<RpcVariable> cueVariables = new List<RpcVariable>();
			List<RpcVariable> globalVariables = new List<RpcVariable>();
			reader.BaseStream.Seek(varsOffset, SeekOrigin.Begin);
			for (int j = 0; j < numVars; j++)
			{
				RpcVariable v = default(RpcVariable);
				v.Name = varNames[j];
				v.Flags = reader.ReadByte();
				v.InitValue = reader.ReadSingle();
				v.MinValue = reader.ReadSingle();
				v.MaxValue = reader.ReadSingle();
				v.Value = v.InitValue;
				variables.Add(v);
				if (!v.IsGlobal)
				{
					cueVariables.Add(v);
					continue;
				}
				globalVariables.Add(v);
				this._variableLookup.Add(v.Name, globalVariables.Count - 1);
			}
			this._cueVariables = cueVariables.ToArray();
			this._variables = globalVariables.ToArray();
			List<RpcCurve> reverbCurves = new List<RpcCurve>();
			this.RpcCurves = new RpcCurve[numRpc];
			if (numRpc != 0)
			{
				reader.BaseStream.Seek(rpcOffset, SeekOrigin.Begin);
				for (int k = 0; k < numRpc; k++)
				{
					RpcCurve curve = new RpcCurve
					{
						FileOffset = (uint)reader.BaseStream.Position
					};
					RpcVariable variable = variables[reader.ReadUInt16()];
					if (variable.IsGlobal)
					{
						curve.IsGlobal = true;
						curve.Variable = globalVariables.FindIndex((RpcVariable e) => e.Name == variable.Name);
					}
					else
					{
						curve.IsGlobal = false;
						curve.Variable = cueVariables.FindIndex((RpcVariable e) => e.Name == variable.Name);
					}
					int pointCount = reader.ReadByte();
					curve.Parameter = (RpcParameter)reader.ReadUInt16();
					curve.Points = new RpcPoint[pointCount];
					for (int j2 = 0; j2 < pointCount; j2++)
					{
						curve.Points[j2].Position = reader.ReadSingle();
						curve.Points[j2].Value = reader.ReadSingle();
						curve.Points[j2].Type = (RpcPointType)reader.ReadByte();
					}
					if (curve.Parameter - 5 >= RpcParameter.Volume && variable.IsGlobal)
					{
						reverbCurves.Add(curve);
					}
					this.RpcCurves[k] = curve;
				}
			}
			this._reverbCurves = reverbCurves.ToArray();
			switch (numDspPresets)
			{
			default:
				throw new Exception("Unexpected number of DSP presets!");
			case 1u:
				if (numDspParams != 22)
				{
					throw new Exception("Unexpected number of DSP parameters!");
				}
				reader.BaseStream.Seek(dspParamsOffset, SeekOrigin.Begin);
				this._reverbSettings = new ReverbSettings(reader);
				break;
			case 0u:
				break;
			}
		}
		this._stopwatch = new Stopwatch();
		this._stopwatch.Start();
	}

	internal int GetRpcIndex(uint fileOffset)
	{
		for (int i = 0; i < this.RpcCurves.Length; i++)
		{
			if (this.RpcCurves[i].FileOffset == fileOffset)
			{
				return i;
			}
		}
		return -1;
	}

	private static string[] ReadNullTerminatedStrings(uint count, BinaryReader reader)
	{
		string[] ret = new string[count];
		for (int i = 0; i < count; i++)
		{
			List<char> s = new List<char>();
			while (reader.PeekChar() != 0)
			{
				s.Add(reader.ReadChar());
			}
			reader.ReadChar();
			ret[i] = new string(s.ToArray());
		}
		return ret;
	}

	/// <summary>
	/// Performs periodic work required by the audio engine.
	/// </summary>
	/// <remarks>Must be called at least once per frame.</remarks>
	public void Update()
	{
		TimeSpan cur = this._stopwatch.Elapsed;
		TimeSpan elapsed = cur - this._lastUpdateTime;
		this._lastUpdateTime = cur;
		float dt = (float)elapsed.TotalSeconds;
		lock (this.UpdateLock)
		{
			int x = 0;
			while (x < this.ActiveCues.Count)
			{
				Cue cue = this.ActiveCues[x];
				cue.Update(dt);
				if (cue.IsStopped || cue.IsDisposed)
				{
					this.ActiveCues.Remove(cue);
				}
				else
				{
					x++;
				}
			}
		}
		if (this._reverbSettings != null)
		{
			for (int i = 0; i < this._reverbCurves.Length; i++)
			{
				RpcCurve curve = this._reverbCurves[i];
				float result = curve.Evaluate(this._variables[curve.Variable].Value);
				int parameter = (int)(curve.Parameter - 5);
				this._reverbSettings[parameter] = result;
			}
			SoundEffect.PlatformSetReverbSettings(this._reverbSettings);
		}
		foreach (SoundEffect effect in SoundEffect.EffectsToRemove)
		{
			if (effect.ShouldBeRemoved())
			{
				effect.Dispose();
			}
		}
		SoundEffect.EffectsToRemove.Clear();
	}

	public ReverbSettings GetReverbSettings()
	{
		return this._reverbSettings;
	}

	public int GetCategoryIndex(string name)
	{
		for (int i = 0; i < this.Categories.Length; i++)
		{
			if (this.Categories[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Returns an audio category by name.</summary>
	/// <param name="name">Friendly name of the category to get.</param>
	/// <returns>The AudioCategory with a matching name. Throws an exception if not found.</returns>
	public AudioCategory GetCategory(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (!this._categoryLookup.TryGetValue(name, out var i))
		{
			throw new InvalidOperationException("This resource could not be created.");
		}
		return this._categories[i];
	}

	/// <summary>Gets the value of a global variable.</summary>
	/// <param name="name">Friendly name of the variable.</param>
	/// <returns>float value of the queried variable.</returns>
	/// <remarks>A global variable has global scope. It can be accessed by all code within a project.</remarks>
	public float GetGlobalVariable(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (!this._variableLookup.TryGetValue(name, out var i) || !this._variables[i].IsPublic)
		{
			throw new IndexOutOfRangeException("The specified variable index is invalid.");
		}
		lock (this.UpdateLock)
		{
			return this._variables[i].Value;
		}
	}

	internal float GetGlobalVariable(int index)
	{
		lock (this.UpdateLock)
		{
			return this._variables[index].Value;
		}
	}

	/// <summary>Sets the value of a global variable.</summary>
	/// <param name="name">Friendly name of the variable.</param>
	/// <param name="value">Value of the global variable.</param>
	public void SetGlobalVariable(string name, float value)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (!this._variableLookup.TryGetValue(name, out var i) || !this._variables[i].IsPublic)
		{
			throw new IndexOutOfRangeException("The specified variable index is invalid.");
		}
		lock (this.UpdateLock)
		{
			this._variables[i].SetValue(value);
		}
	}

	/// <summary>
	/// Disposes the AudioEngine.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~AudioEngine()
	{
		this.Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (!this.IsDisposed)
		{
			this.IsDisposed = true;
			if (disposing)
			{
				EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
			}
		}
	}
}
