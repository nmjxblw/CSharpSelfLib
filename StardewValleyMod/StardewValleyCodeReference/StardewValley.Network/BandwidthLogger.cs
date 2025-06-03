using System;
using System.Collections.Generic;

namespace StardewValley.Network;

public class BandwidthLogger
{
	private long bitsDownSinceLastUpdate;

	private long bitsUpSinceLastUpdate;

	private DateTime lastUpdateTime = DateTime.UtcNow;

	private double lastBitsDownPerSecond;

	private double lastBitsUpPerSecond;

	private double avgBitsUpPerSecond;

	private long bitsUpPerSecondCount;

	private double avgBitsDownPerSecond;

	private long bitsDownPerSecondCount;

	private long totalBitsDown;

	private long totalBitsUp;

	private double totalMs;

	private int queueCapacity = 100;

	private Queue<double> bitsUp = new Queue<double>();

	private Queue<double> bitsDown = new Queue<double>();

	public double AvgBitsDownPerSecond => this.avgBitsDownPerSecond;

	public double AvgBitsUpPerSecond => this.avgBitsUpPerSecond;

	public double BitsDownPerSecond => this.lastBitsDownPerSecond;

	public double BitsUpPerSecond => this.lastBitsUpPerSecond;

	public double TotalBitsDown => this.totalBitsDown;

	public double TotalBitsUp => this.totalBitsUp;

	public double TotalMs => this.totalMs;

	public Queue<double> LoggedAvgBitsUp => this.bitsUp;

	public Queue<double> LoggedAvgBitsDown => this.bitsDown;

	public void Update()
	{
		double msElapsed = (DateTime.UtcNow - this.lastUpdateTime).TotalMilliseconds;
		if (msElapsed > 1000.0)
		{
			this.lastBitsDownPerSecond = (double)this.bitsDownSinceLastUpdate / msElapsed * 1000.0;
			this.lastBitsUpPerSecond = (double)this.bitsUpSinceLastUpdate / msElapsed * 1000.0;
			this.avgBitsDownPerSecond = (this.avgBitsDownPerSecond * (double)this.bitsDownPerSecondCount + this.lastBitsDownPerSecond) / (double)(++this.bitsDownPerSecondCount);
			this.avgBitsUpPerSecond = (this.avgBitsUpPerSecond * (double)this.bitsUpPerSecondCount + this.lastBitsUpPerSecond) / (double)(++this.bitsUpPerSecondCount);
			this.lastUpdateTime = DateTime.UtcNow;
			this.bitsDownSinceLastUpdate = 0L;
			this.bitsUpSinceLastUpdate = 0L;
			this.totalMs += msElapsed;
			if (this.bitsUp.Count >= this.queueCapacity)
			{
				this.bitsUp.Dequeue();
			}
			if (this.bitsDown.Count >= this.queueCapacity)
			{
				this.bitsDown.Dequeue();
			}
			this.bitsUp.Enqueue(this.lastBitsUpPerSecond);
			this.bitsDown.Enqueue(this.lastBitsDownPerSecond);
		}
	}

	public void RecordBytesDown(long bytes)
	{
		this.bitsDownSinceLastUpdate += bytes * 8;
		this.totalBitsDown += bytes * 8;
	}

	public void RecordBytesUp(long bytes)
	{
		this.bitsUpSinceLastUpdate += bytes * 8;
		this.totalBitsUp += bytes * 8;
	}
}
