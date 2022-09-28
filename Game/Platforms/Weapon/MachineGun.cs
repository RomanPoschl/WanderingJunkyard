using Godot;
using System;

public class MachineGun : WeaponBase
{
    BulletBase _bullet = new BasicBullet();
    public override BulletBase Bullet => _bullet;
    public override float RateOfFire => .25f;
    float lastShot = 0;

    public override ComponentAffect ComponentAffect => new ComponentAffect()
    {
        Weight = 100
    };

    public override bool CanShoot()
    {
        var time = Time.GetTicksMsec() / 1000f;

        //GD.Print(time);
        //GD.Print(lastShot);
        //GD.Print(time > lastShot + RateOfFire);

        return time > lastShot + RateOfFire;
    }

    public override void Shoot()
    {
        if(_bulletScene == null)
            GD.PrintErr("MachineGun _bulletScene is null");
        
        lastShot = Time.GetTicksMsec() / 1000f;

        var bullet = (BulletBase)_bulletScene.Instance();
        bullet.Start(GetNode<Node2D>("Muzzle").GlobalPosition, GetParent<Node2D>().GlobalRotation);
        GetParent().GetParent().AddChild(bullet);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }
}
