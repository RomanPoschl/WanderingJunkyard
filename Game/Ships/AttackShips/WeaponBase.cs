using Godot;

public abstract partial class WeaponBase : ShipBoidBase
{
	public abstract BulletBase Bullet { get; }
	public abstract float RateOfFire { get; }
	public abstract bool CanShoot();
	public abstract void Shoot();
	
	public override void _Ready()
	{
		_bulletScene = GD.Load<PackedScene>(Bullet.ScenePath);
		
		base._Ready();
	}
}
