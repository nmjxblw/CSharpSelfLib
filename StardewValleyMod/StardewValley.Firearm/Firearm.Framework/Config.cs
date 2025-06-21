namespace Firearm.Framework;

public class Config
{
	public static Config Default { get; } = new Config();

	public int Ak47ShotSpeed { get; set; } = 600;

	public int Ak47Price { get; set; } = 3000;

	public string Ak47ShotAudio { get; set; } = "clubSmash";

	public int M16ShotSpeed { get; set; } = 800;

	public int M16Price { get; set; } = 3000;

	public string M16ShotAudio { get; set; } = "clubhit";

	public int AssaultRifleDamage { get; set; } = 15;

	public int AssaultRiflePrice { get; set; } = 30;
}
