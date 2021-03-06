﻿////////////////////////////////////////////////////////////////////////////////
// Clickable.cs 
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
using System.IO;
using System.Text;
using System.Xml;

using SFML.System;
using SFML.Window;

using MiCore;
using MiInput;

using Action = MiInput.Action;

namespace MiGfx
{
	/// <summary>
	///   Possible clickable state.
	/// </summary>
	public enum ClickableState
	{
		/// <summary>
		///   If clickable is not being interacted with.
		/// </summary>
		Idle,
		/// <summary>
		///   If the mouse is hovering over the clickable.
		/// </summary>
		Hover,
		/// <summary>
		///   If the clickable has been clicked.
		/// </summary>
		Click
	}

	/// <summary>
	///   A component that makes entities clickable.
	/// </summary>
	public class Clickable : MiComponent
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Clickable()
		:	base()
		{
			Hovering   = false;
			Clicked    = false;
			ClickState = ClickableState.Idle;
			m_timer    = new Clock();
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="c">
		///   The object to copy.
		/// </param>
		public Clickable( Clickable c )
		:	base( c )
		{
			m_hover    = c.m_hover;
			m_click    = c.m_click;
			ClickState = c.ClickState;
			m_timer    = new Clock();

			m_onhover  = new EventHandler( c.m_onhover );
			m_onclick  = new EventHandler( c.m_onclick );
		}

		/// <summary>
		///   The component type name.
		/// </summary>
		public override string TypeName
		{
			get { return nameof( Clickable ); }
		}

		/// <summary>
		///   Current clickable state.
		/// </summary>
		public ClickableState ClickState
		{
			get; protected set;
		}

		/// <summary>
		///   If the mouse is hovering over the entity.
		/// </summary>
		public bool Hovering
		{
			get { return m_hover; }
			protected set 
			{
				if( !m_hover && value )
					OnHover();

				m_hover = value;
			}
		}
		/// <summary>
		///   If the entity is being clicked.
		/// </summary>
		public bool Clicked
		{
			get { return m_click; }
			protected set
			{
				if( !m_click && value )
					OnClick();

				m_click = value;
			}
		}

		/// <summary>
		///   Occurs when the mouse hovers over the entity.
		/// </summary>
		public event EventHandler Hover
		{
			add
			{
				lock( m_onhoverLock )
				{
					m_onhover += value;
				}
			}
			remove
			{
				lock( m_onhoverLock )
				{
					m_onhover -= value;
				}
			}
		}
		/// <summary>
		///   Occurs when the mouse clicks the entity.
		/// </summary>
		public event EventHandler Click
		{
			add
			{
				lock( m_onclickLock )
				{
					m_onclick += value;
				}
			}
			remove
			{
				lock( m_onclickLock )
				{
					m_onclick -= value;
				}
			}
		}

		/// <summary>
		///   Gets the type names of components required by this component type.
		/// </summary>
		/// <returns>
		///   The type names of components required by this component type.
		/// </returns>
		protected override string[] GetRequiredComponents()
		{
			return new string[] { nameof( Transform ), nameof( Selectable ) };
		}

		/// <summary>
		///   Updates the component logic.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		protected override void OnUpdate( float dt )
		{
			Transform  t = Parent?.GetComponent<Transform>();
			Selectable s = Parent?.GetComponent<Selectable>();

			if( !Enabled || Parent?.Window is null || t is null || s is null )
			{
				ClickState = ClickableState.Idle;
				return;
			}

			if( Input.Manager.LastDevice is InputDevice.Mouse )
			{
				Vector2f mpos = Parent.Window.MapPixelToCoords( MouseManager.GetPosition( Parent.Window ) );
				Hovering = t.GlobalBounds.Contains( mpos.X, mpos.Y );

				if( s.Selector?.Parent is not null )
					s.Selected = Hovering;
			}
			else
				Hovering = s.Selected;

			Action submit = Input.Manager.Actions?.Get( "Submit" );

			bool sub = submit is not null ? submit.JustPressed :
					( Input.Manager.Keyboard.JustPressed( Key.Enter ) ||
					  Input.Manager.Joystick.JustPressed( "A" ) );

			if( Input.Manager.LastDevice is InputDevice.Mouse )
				Clicked = Hovering && Input.Manager.Mouse.JustPressed( Mouse.Button.Left );
			else
				Clicked = s.Selected && sub;
			
			ClickState = Clicked ? ClickableState.Click : ( s.Selected ? ClickableState.Hover : ClickableState.Idle );
		}

		/// <summary>
		///   Attempts to deserialize the object from the stream.
		/// </summary>
		/// <param name="sr">
		///   Stream reader.
		/// </param>
		/// <returns>
		///   True if deserialization succeeded and false otherwise.
		/// </returns>
		public override bool LoadFromStream( BinaryReader sr )
		{
			if( !base.LoadFromStream( sr ) )
				return false;

			ClickState = ClickableState.Idle;
			return true;
		}
		/// <summary>
		///   Attempts to serialize the object to the stream.
		/// </summary>
		/// <param name="sw">
		///   Stream writer.
		/// </param>
		/// <returns>
		///   True if serialization succeeded and false otherwise.
		/// </returns>
		public override bool SaveToStream( BinaryWriter sw )
		{
			return base.SaveToStream( sw );
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

			ClickState = ClickableState.Idle;
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
			return new StringBuilder()
				.Append( '<' ).Append( TypeName ).Append( ' ' )
				.Append( nameof( Enabled ) ).Append( "=\"" ).Append( Enabled ).AppendLine( "\"" )
				.Append( "           " )
				.Append( nameof( Visible ) ).Append( "=\"" ).Append( Visible ).Append( "\"/>" ).ToString();
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
		public bool Equals( Clickable other )
		{
			return base.Equals( other ) &&
				   ClickState == other.ClickState;
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
			return Equals( obj as Clickable );
		}

		/// <summary>
		///   Serves as the default hash function.
		/// </summary>
		/// <returns>
		///   A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine( base.GetHashCode(), ClickState );
		}

		/// <summary>
		///   Clones this object.
		/// </summary>
		/// <returns>
		///   A clone of this object.
		/// </returns>
		public override object Clone()
		{
			return new Clickable( this );
		}

		/// <summary>
		///   Disposes of internal objects.
		/// </summary>
		protected override void OnDispose()
		{
			m_timer?.Dispose();
			m_timer = null;
		}

		private void OnHover()
		{
			EventHandler handler;

			lock( m_onhoverLock )
			{
				handler = m_onhover;
			}

			handler?.Invoke( this, EventArgs.Empty );
		}
		private void OnClick()
		{
			EventHandler handler;

			lock( m_onclickLock )
			{
				handler = m_onclick;
			}

			handler?.Invoke( this, EventArgs.Empty );
		}

		private bool m_hover, 
		             m_click;

		private Clock m_timer;

		private EventHandler m_onhover,
		                     m_onclick;

		readonly object m_onhoverLock = new(),
		                m_onclickLock = new();
	}
}
