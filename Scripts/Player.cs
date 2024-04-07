using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float InitialSpeed = 5.0f;
	public const float InitialJumpVelocity = 4.0f;
	public static float Speed {get; set;} = InitialSpeed;
	public static float JumpVelocity {get; set;} = InitialJumpVelocity;

	public static bool Fly {get; set;} = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	//private float hSensitivity = 0.005f;
	//private float vSensitivity = 0.01f;

	public static float HSensitivity {get; set;} = 0.005f;
	public static float VSensitivity {get; set;} = 0.01f;

	public static float HSensitivityController {get=>HSensitivity*10;}
	public static float VSensitivityController {get=>VSensitivity*10;} 

	Camera3D camera3D;

	Joystick joystick;

	GameManager gameManager;

	public override void _Ready()
	{
		camera3D=GetNode<Camera3D>("Camera3D");
		joystick = GetNode<Joystick>("%Joystick");

		gameManager = GetNode<GameManager>("/root/GameManager");


		if(!GameManager.IsMobile)
		{
			joystick.QueueFree();
			return;
		}
		
		//gameManager.HUDVisibility += (visible) => joystick.Visible = visible;
		gameManager.Connect(GameManager.SignalName.HUDVisibility, Callable.From<bool>((visible) => joystick.Visible = visible));
	}

 	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if(!Fly)
		{
			if (!IsOnFloor())
				velocity.Y -= gravity * (float)delta;

			// Handle Jump.
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
				velocity.Y = JumpVelocity;

		}


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir;
		
		if(!GameManager.IsMobile)
		{
			inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		}
		else
		{
			inputDir = joystick.GetDirection();
		}

/* 		GD.Print("Global basis", camera3D.GlobalBasis);
		GD.Print("vector así nomás ", new Vector3(inputDir.X, 0, inputDir.Y));
		GD.Print("multiplicación: ", camera3D.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y)); */

		//Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // OG
		//Vector3 direction = (camera3D.GlobalBasis * new Vector3(inputDir.X, Convert.ToInt32(Fly)*inputDir.Y, inputDir.Y)).Normalized();
		//Vector3 direction = (camera3D.GlobalBasis * new Vector3(inputDir.X, inputDir.Y, inputDir.Y)).Normalized();

		//Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // OG

/* 		float x = direction.X, z = direction.Z;

		direction *= camera3D.GlobalBasis;
		direction.X = x;
		direction.Z = z;

		GD.Print(direction); */

		Vector3 direction = (camera3D.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			if(Fly)
				velocity.Y = direction.Y * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			
			if(Fly)
				velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}


		Velocity = velocity;

		MoveAndSlide();
		MoveCameraController();
	} 



	public void MoveCameraController()
	{
		Vector2 inputDir = Input.GetVector("look_left", "look_right", "look_up", "look_down");

		//GD.Print("Input dir: ", inputDir);
		RotateY(- inputDir.X * HSensitivityController);
		//camera3D.RotateX(- inputDir.Y * VSensitivityController);

		float xAngle = - inputDir.Y * VSensitivityController;
		camera3D.Rotation = new Vector3( Mathf.Clamp(camera3D.Rotation.X + xAngle, -1, 1), camera3D.Rotation.Y, camera3D.Rotation.Z);

	}


	//private void _on_input_event(Camera3D camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseMotion mouseMotion && !GameManager.IsMobile)
		{
			RotateY(- mouseMotion.Relative.X * HSensitivity);
			//RotateX(- mouseMotion.Relative.Y * vSensitivity);
			//arriba -1 abajo 1
			//GD.Print("XAngle: ", camera3D.Rotation.X);
			//camera3D.RotateX(xAngle);

			//GD.Print("Movimiento relativo: ", mouseMotion.Relative);

			float xAngle = - mouseMotion.Relative.Y * VSensitivity;
			camera3D.Rotation = new Vector3( Mathf.Clamp(camera3D.Rotation.X + xAngle, -1, 1), camera3D.Rotation.Y, camera3D.Rotation.Z);
		}

		if(@event is InputEventScreenDrag screenDrag && GameManager.IsMobile && (screenDrag.Index!=joystick.FingerIndex || joystick.GetDirection()==Vector2.Zero))//joystick.GetDirection()==Vector2.Zero)
		{
			RotateY(- screenDrag.Relative.X * HSensitivity);
			//RotateX(- mouseMotion.Relative.Y * vSensitivity);
			//arriba -1 abajo 1
			//GD.Print("XAngle: ", camera3D.Rotation.X);
			//camera3D.RotateX(xAngle);

			//GD.Print("Movimiento relativo: ", mouseMotion.Relative);

			float xAngle = - screenDrag.Relative.Y * VSensitivity;
			camera3D.Rotation = new Vector3( Mathf.Clamp(camera3D.Rotation.X + xAngle, -1, 1), camera3D.Rotation.Y, camera3D.Rotation.Z);
		}

	}

	
}
