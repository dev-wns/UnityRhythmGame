using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
[RequireComponent( typeof( RectTransform ) )]
public abstract class CustomLayoutGroup : MonoBehaviour, ILayoutController
{
    [System.Serializable]
    protected struct RectOffset
    {
        public int left, right, top, bottom;

        public RectOffset( int _left, int _right, int _top, int _bottom )
        {
            left = _left;
            right = _right;
            top = _top;
            bottom = _bottom;
        }
    }
    [SerializeField]
    protected RectOffset padding;
    [SerializeField]
    protected int spacing;
    [SerializeField] 
    protected TextAnchor childAlignment = TextAnchor.MiddleCenter;
    protected Vector2 anchor;

    [SerializeField]
    protected List<RectTransform> rectChildren = new List<RectTransform>();
    public void Initialize()
    {
        SetAlignment();

        var rt = transform as RectTransform;
        for ( int i = 0; i < rt.childCount; i++ )
        {
            var child = rt.GetChild( i ).transform as RectTransform;
            child.anchorMin = anchor;
            child.anchorMax = anchor;

            rectChildren.Add( child );
        }

        SetLayoutHorizontal();
        SetLayoutVertical();
    }

    protected virtual void Start()
    {
        if ( Application.isPlaying )
             Initialize();
    }

    protected virtual void Update()
    {
        // Editor Update
        if ( !Application.isPlaying )
        {
            rectChildren?.Clear();

            SetAlignment();
            Initialize();
        }
    }

    public abstract void SetLayoutHorizontal();
    public abstract void SetLayoutVertical();

    private void SetAlignment()
    {
        switch ( childAlignment )
        {
            case TextAnchor.UpperLeft:
            anchor = new Vector2( 0f, 1f );
            break;

            case TextAnchor.UpperCenter:
            anchor = new Vector2( .5f, 1f );
            break;

            case TextAnchor.UpperRight:
            anchor = new Vector2( 1f, 1f );
            break;

            case TextAnchor.MiddleLeft:
            anchor = new Vector2( 0f, .5f );
            break;

            case TextAnchor.MiddleCenter:
            anchor = new Vector2( .5f, .5f );
            break;

            case TextAnchor.MiddleRight:
            anchor = new Vector2( 1f, .5f );
            break;

            case TextAnchor.LowerLeft:
            anchor = new Vector2( 0f, 0f );
            break;

            case TextAnchor.LowerCenter:
            anchor = new Vector2( .5f, 0f );
            break;

            case TextAnchor.LowerRight:
            anchor = new Vector2( 1f, 0f );
            break;
        }
    }
}
