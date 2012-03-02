using UnityEngine;
using System;


public class UITouchableArea : UISprite, ITouchable, IComparable
{
	public event Action<UITouchableArea> onTouchUpInside;
	public event Action<UITouchableArea> onTouchDown;

	public int touchCount;
	public Vector2 initialTouchPosition;

	protected UIEdgeOffsets _normalTouchOffsets;
	protected UIEdgeOffsets _highlightedTouchOffsets;
	protected Rect _highlightedTouchFrame;
	protected Rect _normalTouchFrame;

	protected bool touchFrameIsDirty = true; // Indicates if the touchFrames need to be recalculated

	protected bool _highlighted;
	protected bool _disabled;
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	protected bool _hoveredOver;
#endif

	public UITouchableArea(UIToolkit manager, Rect frame)
		: base(frame, 0, UIUVRect.zero)
	{
		manager.addTouchableSprite(this);
	}


	// constructor for when the need to have a centered UISprite arises (I'm looking at you UIKnob)
	public UITouchableArea(UIToolkit manager, Rect frame, bool gameObjectOriginInCenter)
		: base(frame, 0, UIUVRect.zero, gameObjectOriginInCenter)
	{
		manager.addTouchableSprite(this);
	}


	#region Properties and Getters/Setters

	// Adds or subtracts from the frame of the button to define a hit area
	public UIEdgeOffsets highlightedTouchOffsets
	{
		get { return _highlightedTouchOffsets; }
		set
		{
			_highlightedTouchOffsets = value;
			touchFrameIsDirty = true;
		}
	}


	// Adds or subtracts from the frame of the button to define a hit area
	public UIEdgeOffsets normalTouchOffsets
	{
		get { return _normalTouchOffsets; }
		set
		{
			_normalTouchOffsets = value;
			touchFrameIsDirty = true;
		}
	}


	// Returns a frame to use to see if this element was touched
	public Rect touchFrame
	{
		get
		{
			// if we are disabled, we have no touchFrame to touch
			if( _disabled )
				return UISprite._rectZero;

			// If the frame is dirty, recalculate it
			if( touchFrameIsDirty )
			{
				touchFrameIsDirty = false;

				// grab the normal frame of the sprite then add the offsets to get our touch frames
				// remembering to offset if we have our origin in the center
				var normalFrame = new Rect( clientTransform.position.x, -clientTransform.position.y, width, height );

				if( gameObjectOriginInCenter )
				{
					normalFrame.x -= width / 2;
					normalFrame.y -= height / 2;
				}

				_normalTouchFrame = addOffsetsAndClipToScreen( normalFrame, _normalTouchOffsets );
				_highlightedTouchFrame = addOffsetsAndClipToScreen( normalFrame, _highlightedTouchOffsets );
			}

			// Either return our highlighted or normal touch frame
			return ( _highlighted ) ? _highlightedTouchFrame : _normalTouchFrame;
		}
	}


	private Rect addOffsetsAndClipToScreen( Rect frame, UIEdgeOffsets offsets )
	{
		return Rect.MinMaxRect
		(
			 Mathf.Clamp( frame.x - offsets.left, 0, Screen.width ),
			 Mathf.Clamp( frame.y - offsets.top, 0, Screen.height ),
			 Mathf.Clamp( frame.x + frame.width + offsets.right, 0, Screen.width),
			 Mathf.Clamp( frame.y + + frame.height + offsets.bottom, 0, Screen.height)
		);
	}


	// Override transform() so we can mark the touchFrame as dirty
	public override void updateTransform()
	{
		base.updateTransform();

		touchFrameIsDirty = true;
	}

	#endregion;


	/// <summary>
	/// Tests if a point is inside the current touchFrame
	/// </summary>
	public bool hitTest( Vector2 point )
	{
		return touchFrame.Contains( point );
	}


	// Indicates if there is a finger over this element
	public virtual bool highlighted
	{
		get { return _highlighted; }
		set { _highlighted = value;	}
	}


	// override hidden so we can set the touch frame to dirty when being shown
	public override bool hidden
	{
		get { return ___hidden; }
		set
		{
			base.hidden = value;

			if( value )
				touchFrameIsDirty = true;
		}
	}


	// indicates if the mouse pointer is hovering over this element
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	public virtual bool hoveredOver
	{
		get { return _hoveredOver; }
		set { _hoveredOver = value; }
	}
#endif


	// a disabled UITouchableSprite will have a touchFrame of all zeros
	public virtual bool disabled
	{
		get { return _disabled; }
		set { _disabled = value; }
	}


	// Transforms a point to local coordinates (origin is top left)
	protected Vector2 inverseTranformPoint( Vector2 point )
	{
		return new Vector2( point.x - _normalTouchFrame.xMin, point.y - _normalTouchFrame.yMin );
	}


	public override void centerize()
	{
		touchFrameIsDirty = true;
		base.centerize();
	}


	#region ITouchable

	// Touch handlers.	Subclasses should override these to get their specific behaviour
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	public virtual void onTouchBegan( UIFakeTouch touch, Vector2 touchPos )
#else
	public virtual void onTouchBegan( Touch touch, Vector2 touchPos )
#endif
	{
		highlighted = true;

		initialTouchPosition = touch.position;

		if( onTouchDown != null )
			onTouchDown( this );
	}


#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	public virtual void onTouchMoved( UIFakeTouch touch, Vector2 touchPos )
#else
	public virtual void onTouchMoved( Touch touch, Vector2 touchPos )
#endif
	{

	}


#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	public virtual void onTouchEnded( UIFakeTouch touch, Vector2 touchPos, bool touchWasInsideTouchFrame )
#else
	public virtual void onTouchEnded( Touch touch, Vector2 touchPos, bool touchWasInsideTouchFrame )
#endif
	{
		highlighted = false;

		// If the touch was inside our touchFrame and we have an action, call it
		if( touchWasInsideTouchFrame && onTouchUpInside != null )
			onTouchUpInside( this );
	}

	#endregion;


	// IComparable - sorts based on the z value of the client
	public int CompareTo( object obj )
	{
		if( obj is ITouchable )
		{
			var temp = obj as ITouchable;
			return position.z.CompareTo( temp.position.z );
		}

		return -1;
	}

}

