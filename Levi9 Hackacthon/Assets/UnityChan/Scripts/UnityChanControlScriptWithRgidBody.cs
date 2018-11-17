using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]

public class UnityChanControlScriptWithRgidBody : MonoBehaviour
{
    public Joystick joystick;
    public FloatingJoystick button;

    public float animSpeed = 1.5f;				
	public float lookSmoother = 3.0f;			// a smoothing setting for camera motion
	public bool useCurves = true;				// Mecanimでカーブ調整を使うか設定する
												// このスイッチが入っていないとカーブは使われない
	public float useCurvesHeight = 0.5f;		// カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）


	public float forwardSpeed = 7.0f;
	public float backwardSpeed = 2.0f;
	public float rotateSpeed = 15f;
	public float jumpPower = 3.0f; 

	private CapsuleCollider col;
	private Rigidbody rb;
	private Vector3 velocity;
	private float orgColHight;
	private Vector3 orgVectColCenter;
	
	private Animator anim;
	private AnimatorStateInfo currentBaseState;

	private GameObject cameraObject;
		

	static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");

    public Transform player;
    public float speed = 5.0f;
    private bool touchStart = false;
    private Vector2 pointA;
    private Vector2 pointB;

    public Transform circle;
    public Transform outerCircle;

    void Start ()
	{
		// Animator
		anim = GetComponent<Animator>();

		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();

		cameraObject = GameObject.FindWithTag("MainCamera");

		orgColHight = col.height;
		orgVectColCenter = col.center;
    }

	void FixedUpdate ()
	{
        Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);


        float v = direction.magnitude;

        anim.SetFloat("Speed", v);							
		anim.SetFloat("Direction", 0); 						
		anim.speed = animSpeed;								
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);	
		rb.useGravity = true;
		
		velocity = new Vector3(0, 0, v);

		velocity = transform.TransformDirection(velocity);

		if (v > 0.1) {
			velocity *= forwardSpeed;
		} else if (v < -0.1) {
			velocity *= backwardSpeed;
		}
		
		if (button.isPressed) {
			if (currentBaseState.nameHash == locoState){
				if(!anim.IsInTransition(0))
				{
					rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
					anim.SetBool("Jump", true);
				}
			}
		}
		
		transform.localPosition += velocity * Time.fixedDeltaTime;

        float step = rotateSpeed * Time.deltaTime;
        
        Vector3 mousedownCameraProjectedRight = Vector3.ProjectOnPlane(cameraObject.transform.right, transform.up).normalized;
        Vector3 mousedownCameraProjectedForward = Vector3.ProjectOnPlane(cameraObject.transform.forward, transform.up).normalized;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, (direction.x * mousedownCameraProjectedRight + direction.y * mousedownCameraProjectedForward), step, 0.0f);
        
        transform.rotation = Quaternion.LookRotation(newDir);

        if (currentBaseState.nameHash == locoState){
			if(useCurves){
				resetCollider();
			}
		}

		else if(currentBaseState.nameHash == jumpState)
		{
			cameraObject.SendMessage("setCameraPositionJumpView");

			if(!anim.IsInTransition(0))
			{
				if(useCurves){
					float jumpHeight = anim.GetFloat("JumpHeight");
					float gravityControl = anim.GetFloat("GravityControl"); 
					if(gravityControl > 0)
						rb.useGravity = false;
										
					Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
					RaycastHit hitInfo = new RaycastHit();

					if (Physics.Raycast(ray, out hitInfo))
					{
						if (hitInfo.distance > useCurvesHeight)
						{
							col.height = orgColHight - jumpHeight;
							float adjCenterY = orgVectColCenter.y + jumpHeight;
							col.center = new Vector3(0, adjCenterY, 0);
						}
						else{
							resetCollider();
						}
					}
				}
				anim.SetBool("Jump", false);
			}
		}
		else if (currentBaseState.nameHash == idleState)
		{
			if(useCurves){
				resetCollider();
			}
			if (Input.GetButtonDown("Jump")) {
				anim.SetBool("Rest", true);
			}
		}
		else if (currentBaseState.nameHash == restState)
		{
			if(!anim.IsInTransition(0))
			{
				anim.SetBool("Rest", false);
			}
		}
	}

	void resetCollider()
	{
		col.height = orgColHight;
		col.center = orgVectColCenter;
	}
}
