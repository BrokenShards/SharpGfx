﻿////////////////////////////////////////////////////////////////////////////////
// Transform.cs 
////////////////////////////////////////////////////////////////////////////////
//
// MiGfx - A basic graphics library for use with SFML.Net.
// Copyright (C) 2021 Michael Furlong <michaeljfurlong@outlook.com>
//
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free 
// Software Foundation, either version 3 of the License, or (at your option) 
// any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <https://www.gnu.org/licenses/>.
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using SFML.Graphics;
using SFML.System;
using MiCore;

namespace MiGfx
{
	/// <summary>
	///   Interface for objects with a transform.
	/// </summary>
	public interface ITransformable
	{
		/// <summary>
		///   The transform.
		/// </summary>
		Transform Transform { get; }
	}

	/// <summary>
	///   A 2D transformation.
	/// </summary>
	public class Transform : MiComponent, IEquatable<Transform>
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Transform()
		:	base()
		{
			Relative = false;
			Origin   = Allignment.TopLeft;
			Position = new Vector2f();
			Size     = new Vector2f( 1.0f, 1.0f );
			Scale    = new Vector2f( 1.0f, 1.0f );
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="t">
		///   The transform to copy from.
		/// </param>
		public Transform( Transform t )
		:	base( t )
		{
			Relative = t.Relative;
			Origin   = t.Origin;
			Position = t.Position;
			Size     = t.Size;
			Scale    = t.Scale;
		}
		/// <summary>
		///   Constructor assigning values.
		/// </summary>
		/// <param name="pos">
		///   Transform position.
		/// </param>
		/// <param name="size">
		///   Transform size.
		/// </param>
		/// <param name="scl">
		///   Transform scale.
		/// </param>
		/// <param name="org">
		///   Origin point.
		/// </param>
		/// <param name="rel">
		///   If the transform is relative to the view of the window.
		/// </param>
		public Transform( Vector2f pos, Vector2f? size = null, Vector2f? scl = null, Allignment org = 0, bool rel = false )
		:	base()
		{
			Relative = rel;
			Origin   = org;
			Position = pos;
			Size     = size ?? new Vector2f( 1.0f, 1.0f );
			Scale    = scl  ?? new Vector2f( 1.0f, 1.0f );
		}

		/// <summary>
		///   Component type name.
		/// </summary>
		public override string TypeName
		{
			get { return nameof( Transform ); }
		}

		/// <summary>
		///   If the transform is relative to the top left corner of the window view (true) or is
		///   in absolute pixel coordinates (false).
		/// </summary>
		public bool Relative
		{
			get; set;
		}
		/// <summary>
		///   The origin point of the transform.
		/// </summary>
		public Allignment Origin
		{
			get; set;
		}

		/// <summary>
		///   Position.
		/// </summary>
		public Vector2f Position 
		{
			get { return m_pos; }
			set
			{
				if( m_pos.Equals( value ) )
					return;

				Vector2f diff = value - m_pos;
				m_pos = value;

				if( Parent is null || !Parent.HasChildren )
					return;

				MiEntity[] children = Parent.GetChildrenWithComponent<Transform>();

				for( int i = 0; i < children.Length; i++ )
					children[ i ].GetComponent<Transform>().Position += diff;
			}
		}
		/// <summary>
		///   Local size.
		/// </summary>
		public Vector2f Size
		{
			get { return m_size; }
			set
			{
				if( value.X <= 0.0f )
				{
					float x = Math.Abs( value.X );
					value.X = x > 0.0f ? x : 1.0f;
				}
				if( value.Y <= 0.0f )
				{
					float y = Math.Abs( value.Y );
					value.Y = y > 0.0f ? y : 1.0f;
				}

				m_size = value;
			}
		}
		/// <summary>
		///   Scale.
		/// </summary>
		public Vector2f Scale
		{
			get { return m_scale; }
			set
			{
				if( value.X <= 0.0f || value.Y <= 0.0f )
					m_scale = new Vector2f( 1.0f, 1.0f );
				else
					m_scale = value;
			}
		}

		/// <summary>
		///   The absolute position of the transform, taking into account if it is relative or not.
		/// </summary>
		public Vector2f AbsolutePosition
		{
			get
			{
				if( !Relative )
					return Position;

				View view = Parent?.Window?.GetView();

				if( view is null )
					throw new InvalidOperationException( "Transform cannot calculate absolute position without a valid parent and window." );

				return view.Center - ( view.Size / 2.0f ) + Position;
			}
			set
			{
				if( !Relative )
				{
					Position = value;
					return;
				}

				View view = Parent?.Window?.GetView();

				if( view is null )
					throw new InvalidOperationException( "Transform cannot assign absolute position without a valid parent and window." );

				Position = view.Center - ( view.Size / 2.0f ) + value;
			}
		}
		/// <summary>
		///   Scaled size.
		/// </summary>
		public Vector2f ScaledSize
		{
			get { return new Vector2f( m_size.X * m_scale.X, m_size.Y * m_scale.Y ); }
		}

		/// <summary>
		///   Scaled bounds of the transform.
		/// </summary>
		public FloatRect GlobalBounds
		{
			get
			{
				Vector2f pos  = AbsolutePosition,
				         size = ScaledSize;

				switch( Origin )
				{
					case Allignment.Top:
						pos.X -= size.X / 2.0f;
						break;
					case Allignment.TopRight:
						pos.X -= size.X;
						break;

					case Allignment.Left:
						pos.Y -= size.Y / 2.0f;
						break;
					case Allignment.Middle:
						pos -= size / 2.0f;
						break;
					case Allignment.Right:
						pos.X -= size.X;
						pos.Y -= size.Y / 2.0f;
						break;

					case Allignment.BottomLeft:
						pos.Y -= size.Y;
						break;
					case Allignment.Bottom:
						pos.X -= size.X / 2.0f;
						pos.Y -= size.Y;
						break;
					case Allignment.BottomRight:
						pos -= size;
						break;
				}

				return new FloatRect( pos, size ); 
			}
		}

		/// <summary>
		///   Sets if the transform is relative to the window view or not while maintaining absolute
		///   position.
		/// </summary>
		/// <param name="rel">
		///   If the transform should be relative to the view or not.
		/// </param>
		public void SetRelative( bool rel )
		{
			if( Relative == rel )
				return;

			if( Relative )
			{
				Position = AbsolutePosition;
				Relative = false;
			}
			else
			{
				View view = Parent?.Window?.GetView();

				if( view is null )
					throw new InvalidOperationException( "Transform cannot calculate absolute position without a valid parent and window." );

				Position -= ( view.Center - ( view.Size / 2.0f ) );
				Relative = true;
			}
		}
		/// <summary>
		///   Sets the origin point while maintaining position.
		/// </summary>
		/// <param name="org">
		///   The new origin point.
		/// </param>
		public void SetOrigin( Allignment org )
		{
			if( org < 0 || (int)org >= Enum.GetNames( typeof( Allignment ) ).Length || org == Origin )
				return;

			FloatRect bounds = GlobalBounds;
			Vector2f  pos    = new( bounds.Left,  bounds.Top ),
			          size   = new( bounds.Width, bounds.Height );

			switch( org )
			{
				case Allignment.Top:
					pos.X -= size.X / 2.0f;
					break;
				case Allignment.TopRight:
					pos.X -= size.X;
					break;

				case Allignment.Left:
					pos.Y -= size.Y / 2.0f;
					break;
				case Allignment.Middle:
					pos -= size / 2.0f;
					break;
				case Allignment.Right:
					pos.X -= size.X;
					pos.Y -= size.Y / 2.0f;
					break;

				case Allignment.BottomLeft:
					pos.Y -= size.Y;
					break;
				case Allignment.Bottom:
					pos.X -= size.X / 2.0f;
					pos.Y -= size.Y;
					break;
				case Allignment.BottomRight:
					pos -= size;
					break;
			}

			Origin   = org;
			Position = pos;
		}

		/// <summary>
		///   Loads the object from the stream.
		/// </summary>
		/// <param name="br">
		///   The stream reader.
		/// </param>
		/// <returns>
		///   True if the object was successfully loaded from the stream and false otherwise.
		/// </returns>
		public override bool LoadFromStream( BinaryReader br )
		{
			if( !base.LoadFromStream( br ) )
				return false;

			try
			{
				Relative = br.ReadBoolean();
				Origin   = (Allignment)br.ReadInt32();
				Position = new Vector2f( br.ReadSingle(), br.ReadSingle() );
				Size     = new Vector2f( br.ReadSingle(), br.ReadSingle() );
				Scale    = new Vector2f( br.ReadSingle(), br.ReadSingle() );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( $"Failed loading Transform: { e.Message }", false, LogType.Error );
			}

			return true;
		}
		/// <summary>
		///   Writes the object to a stream.
		/// </summary>
		/// <param name="bw">
		///   The stream writer.
		/// </param>
		/// <returns>
		///   True if the object was successfully written to the stream and false otherwise.
		/// </returns>
		public override bool SaveToStream( BinaryWriter bw )
		{
			if( !base.SaveToStream( bw ) )
				return false;

			try
			{
				bw.Write( Relative );
				bw.Write( (int)Origin );
				bw.Write( Position.X ); bw.Write( Position.Y );
				bw.Write( Size.X );     bw.Write( Size.Y );
				bw.Write( Scale.X );    bw.Write( Scale.Y );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( $"Failed saving Transform: { e.Message }", false, LogType.Error );
			}

			return true;
		}
		/// <summary>
		///   Attempts to load the object from the xml element.
		/// </summary>
		/// <param name="element">
		///   The xml element.
		/// </param>
		/// <returns>
		///   True if the object was successfully loaded, otherwise false.
		/// </returns>
		public override bool LoadFromXml( XmlElement element )
		{
			if( !base.LoadFromXml( element ) )
				return false;

			Origin     = 0;
			Scale      = new Vector2f( 1.0f, 1.0f );

			XmlElement position = element[ nameof( Position ) ],
			           size     = element[ nameof( Size ) ],
			           scale    = element[ nameof( Scale ) ];

			Vector2f? pos = Xml.ToVec2f( position ),
					  siz = Xml.ToVec2f( size );

			if( position is null )
				return Logger.LogReturn( "Failed loading Transform: No Position element.", false, LogType.Error );
			if( size is null )
				return Logger.LogReturn( "Failed loading Transform: No Size element.", false, LogType.Error );

			if( !pos.HasValue )
				return Logger.LogReturn( "Failed loading Transform: Unable to parse position.", false, LogType.Error );
			if( !siz.HasValue )
				return Logger.LogReturn( "Failed loading Transform: Unable to parse size.", false, LogType.Error );

			Position = pos.Value;
			Size     = siz.Value;

			if( element.HasAttribute( nameof( Relative ) ) )
			{
				if( !bool.TryParse( element.GetAttribute( nameof( Relative ) ), out bool r ) )
					return Logger.LogReturn( "Failed loading Transform: Unable to parse Relative attribute.", false, LogType.Error );

				Relative = r;
			}
			if( element.HasAttribute( nameof( Origin ) ) )
			{
				if( !Enum.TryParse( element.GetAttribute( nameof( Origin ) ), true, out Allignment a ) )
					return Logger.LogReturn( "Failed loading Transform: Unable to parse Anchor attribute.", false, LogType.Error );

				Origin = a;
			}

			if( scale is not null )
			{
				Vector2f? scl = Xml.ToVec2f( scale );

				if( !scl.HasValue )
					return Logger.LogReturn( "Failed loading Transform: Unable to parse scale.", false, LogType.Error );

				Scale = scl.Value;
			}

			return true;
		}
		/// <summary>
		///   Converts the object to an xml string.
		/// </summary>
		/// <returns>
		///   Returns the object to an xml string.
		/// </returns>
		public override string ToString()
		{
			StringBuilder sb = new();

			sb.Append( '<' ).Append( TypeName ).Append( ' ' )
				.Append( nameof( Enabled ) ).Append( "=\"" ).Append( Enabled ).AppendLine( "\"" )
				.Append( "           " )
				.Append( nameof( Visible ) ).Append( "=\"" ).Append( Visible ).AppendLine( "\"" )
				.Append( "           " )
				.Append( nameof( Relative ) ).Append( "=\"" ).Append( Relative ).AppendLine( "\"" )
				.Append( "           " )
				.Append( nameof( Origin ) ).Append( "=\"" ).Append( Origin ).AppendLine( "\">" )
				
				.AppendLine( Xml.ToString( Position, nameof( Position ), 1 ) )
				.AppendLine( Xml.ToString( Size,     nameof( Size ), 1 ) );

			if( Scale.X != 1.0f || Scale.Y != 1.0f )
				sb.AppendLine( Xml.ToString( Scale, nameof( Scale ), 1 ) );

			return sb.Append( "</" ).Append( TypeName ).Append( '>' ).ToString();
		}

		/// <summary>
		///   If this object has the same values of the other object.
		/// </summary>
		/// <param name="other">
		///   The other object to check against.
		/// </param>
		/// <returns>
		///   True if both objects are concidered equal and false if they are not.
		/// </returns>
		public bool Equals( Transform other )
		{
			return Origin == other.Origin &&
			       Position.Equals( other.Position ) &&
				   Size.Equals( other.Size ) &&
				   Scale.Equals( other.Scale );
		}
		/// <summary>
		///   If this object has the same values of the other object.
		/// </summary>
		/// <param name="obj">
		///   The other object to check against.
		/// </param>
		/// <returns>
		///   True if both objects are concidered equal and false if they are not.
		/// </returns>
		public override bool Equals( object obj )
		{
			return Equals( obj as Transform );
		}

		/// <summary>
		///   Serves as the default hash function.
		/// </summary>
		/// <returns>
		///   A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine( base.GetHashCode(), Origin, Position, Size, Scale );
		}

		/// <summary>
		///   Clones this object.
		/// </summary>
		/// <returns>
		///   A clone of this object.
		/// </returns>
		public override object Clone()
		{
			return new Transform( this );
		}

		/// <summary>
		///   Gets immediate children of the root entity that contain a Transform and are within the
		///   given area.
		/// </summary>
		/// <param name="root">
		///   The root entity containing the entities to check.
		/// </param>
		/// <param name="area">
		///   The area in world pixel coordinates to check.
		/// </param>
		/// <returns>
		///   All immediate children of the root entity that contain a Transform and are within the
		///   given area.
		/// </returns>
		public static MiEntity[] GetEntitiesInArea( MiEntity root, FloatRect area )
		{
			List<MiEntity> ents = new();

			MiEntity[] children = root.GetChildrenWithComponent<Transform>();

			for( int i = 0; i < children.Length; i++ )
			{
				FloatRect b = children[ i ].GetComponent<Transform>().GlobalBounds;

				if( b.Left > area.Left + area.Width ||
					area.Left > b.Left + b.Width ||
					b.Top > area.Top + area.Height ||
					area.Top > b.Top + b.Height )
					continue;

				ents.Add( children[ i ] );
			}

			return ents.ToArray();
		}
		/// <summary>
		///   Recursively gets all children of the root entity that contain a Transform and are
		///   within the given area.
		/// </summary>
		/// <param name="root">
		///   The root entity containing the entities to check.
		/// </param>
		/// <param name="area">
		///   The area in world pixel coordinates to check.
		/// </param>
		/// <returns>
		///   All children of the root entity that contain a Transform and are within the given area.
		/// </returns>
		public static MiEntity[] GetAllEntitiesInArea( MiEntity root, FloatRect area )
		{
			List<MiEntity> ents = new();

			MiEntity[] children = root.GetAllChildrenWithComponent<Transform>();

			for( int i = 0; i < children.Length; i++ )
			{
				FloatRect b = children[ i ].GetComponent<Transform>().GlobalBounds;

				if( b.Left > area.Left + area.Width ||
					area.Left > b.Left + b.Width ||
					b.Top > area.Top + area.Height ||
					area.Top > b.Top + b.Height )
					continue;

				ents.Add( children[ i ] );
			}

			return ents.ToArray();
		}

		private Vector2f m_pos, m_size, m_scale;
	}
}
