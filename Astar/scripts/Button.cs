using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour 
{
    public  delegate void buttonHitDelegate( Button sender  );

    public event buttonHitDelegate OnButtonDown;
    public event buttonHitDelegate OnButtonUp;
	public event buttonHitDelegate OnButtonHeldBegins;
	public event buttonHitDelegate OnButtonHeldRepeats;
	public event buttonHitDelegate OnButtonHeld;

    public TextMesh label;

    public BoxCollider2D bcollider;
 
	public Camera attachdCamera;

    public Vector3 ScaleWhenHovering = Vector3.one;
    protected Vector3 ScaleWhenIdle;

    bool isTouched = false;
    public float ScaleSpeed = 0.1f;

	public float HoldTimer
	{
		get
		{
			return holdTimer;
		}
	}

	float holdTimer;
	float holdRepeatTimer;

	public float holdTimeEventTimeout = 1.5f;

	public float holdTimeRepeatsEventTimeout = 0.5f;

	public float holdTimeRepeatsEventTimeoutMax = 0.5f;
	public float holdTimeRepeatsEventTimeoutMin = 0.15f;

	public float holdTimeRepeatsEventDecreasePerStep = 0.01f;

	bool touchKeepedEventSent;

	virtual protected void OnStart()
	{
		ScaleWhenIdle = transform.localScale;
	}

    void Start ()
    {
		OnStart();
    }

	virtual protected void OnUpdate()
	{
		Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint( Input.mousePosition );

		if( attachdCamera != null )
		{
			mousePosWorld = attachdCamera.ScreenToWorldPoint( Input.mousePosition );
		}

		if( bcollider != null )
		{
			isTouched = false;

			Collider2D [] hits =  Physics2D.OverlapPointAll(mousePosWorld);

			foreach ( Collider2D collider in hits )
			{
				if( collider == bcollider )
				{
					isTouched = true;
				}
			}
		}
		 
		if( isTouched )
		{
			if( transform.localScale != ScaleWhenHovering )
			{
				transform.localScale = Vector3.MoveTowards(transform.localScale, ScaleWhenHovering, ScaleSpeed * Time.deltaTime );
			}

			holdTimer+= Time.deltaTime;

			if ( holdTimer > holdTimeEventTimeout )
			{
				if( OnButtonHeldBegins != null && !touchKeepedEventSent)
				{
					OnButtonHeldBegins(this);
					touchKeepedEventSent=true;
				}
				else if( OnButtonHeldRepeats != null && touchKeepedEventSent)
				{
					holdRepeatTimer += Time.deltaTime;

					if( holdRepeatTimer > holdTimeRepeatsEventTimeout )
					{
						if( holdTimeRepeatsEventTimeout > holdTimeRepeatsEventTimeoutMin )
						{
							holdTimeRepeatsEventTimeout-= holdTimeRepeatsEventDecreasePerStep;
							if( holdTimeRepeatsEventTimeout < holdTimeRepeatsEventTimeoutMin )
							{
								holdTimeRepeatsEventTimeout = holdTimeRepeatsEventTimeoutMin;
							}
						}

						holdRepeatTimer = 0;

						if( OnButtonHeldRepeats != null )
						{
							OnButtonHeldRepeats(this);
						}
					}
				}
				else if( OnButtonHeld != null )
				{
					OnButtonHeld(this);
				}
			}
		}
		else
		{
			holdTimer=0;
			holdRepeatTimer=0;
			touchKeepedEventSent = false;
			holdTimeRepeatsEventTimeout = holdTimeRepeatsEventTimeoutMax;
				
			if( transform.localScale != ScaleWhenIdle )
			{
				transform.localScale = Vector3.MoveTowards(transform.localScale, ScaleWhenIdle, ScaleSpeed * Time.deltaTime );
			}
		}
	}

    void Update()
    {
		OnUpdate();

    }

    void OnMouseDown ()
    {
        if (OnButtonDown != null)
        {
            OnButtonDown(this);
        }
    }

    void OnMouseUp ()
    {
        if (OnButtonUp != null)
        {
            OnButtonUp(this);
        }
    }
}
