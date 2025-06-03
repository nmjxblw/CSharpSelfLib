using System.IO;

namespace Microsoft.Xna.Framework.Audio;

public class ReverbSettings
{
	private readonly DspParameter[] _parameters = new DspParameter[22];

	public float this[int index]
	{
		get
		{
			return this._parameters[index].Value;
		}
		set
		{
			this._parameters[index].SetValue(value);
		}
	}

	public float ReflectionsDelayMs => this._parameters[0].Value;

	public float ReverbDelayMs => this._parameters[1].Value;

	public float PositionLeft => this._parameters[2].Value;

	public float PositionRight => this._parameters[3].Value;

	public float PositionLeftMatrix => this._parameters[4].Value;

	public float PositionRightMatrix => this._parameters[5].Value;

	public float EarlyDiffusion => this._parameters[6].Value;

	public float LateDiffusion => this._parameters[7].Value;

	public float LowEqGain => this._parameters[8].Value;

	public float LowEqCutoff => this._parameters[9].Value;

	public float HighEqGain => this._parameters[10].Value;

	public float HighEqCutoff => this._parameters[11].Value;

	public float RearDelayMs => this._parameters[12].Value;

	public float RoomFilterFrequencyHz => this._parameters[13].Value;

	public float RoomFilterMainDb => this._parameters[14].Value;

	public float RoomFilterHighFrequencyDb => this._parameters[15].Value;

	public float ReflectionsGainDb => this._parameters[16].Value;

	public float ReverbGainDb => this._parameters[17].Value;

	public float DecayTimeSec => this._parameters[18].Value;

	public float DensityPct => this._parameters[19].Value;

	public float RoomSizeFeet => this._parameters[20].Value;

	public float WetDryMixPct => this._parameters[21].Value;

	public ReverbSettings(BinaryReader reader)
	{
		this._parameters[0] = new DspParameter(reader);
		this._parameters[1] = new DspParameter(reader);
		this._parameters[2] = new DspParameter(reader);
		this._parameters[3] = new DspParameter(reader);
		this._parameters[4] = new DspParameter(reader);
		this._parameters[5] = new DspParameter(reader);
		this._parameters[6] = new DspParameter(reader);
		this._parameters[7] = new DspParameter(reader);
		this._parameters[8] = new DspParameter(reader);
		this._parameters[9] = new DspParameter(reader);
		this._parameters[10] = new DspParameter(reader);
		this._parameters[11] = new DspParameter(reader);
		this._parameters[12] = new DspParameter(reader);
		this._parameters[13] = new DspParameter(reader);
		this._parameters[14] = new DspParameter(reader);
		this._parameters[15] = new DspParameter(reader);
		this._parameters[16] = new DspParameter(reader);
		this._parameters[17] = new DspParameter(reader);
		this._parameters[18] = new DspParameter(reader);
		this._parameters[19] = new DspParameter(reader);
		this._parameters[20] = new DspParameter(reader);
		this._parameters[21] = new DspParameter(reader);
	}
}
