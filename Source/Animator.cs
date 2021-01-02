﻿////////////////////////////////////////////////////////////////////////////////
// Animator.cs 
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
using MiCore;

namespace MiGfx
{
	/// <summary>
	///   Runs and manages animations and animation sets.
	/// </summary>
	[Serializable]
	public class Animator : BinarySerializable, IXmlLoadable, IEquatable<Animator>
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Animator()
		{
			Animations = new AnimationSet();
			Playing    = false;
			Loop       = true;
			Multiplier = 1.0f;
			m_selected = string.Empty;
			FrameIndex = 0;
			m_timer    = new Clock();
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="a"></param>
		public Animator( Animator a )
		:	this()
		{
			if( a == null )
				throw new ArgumentNullException();
			 
			Animations = new AnimationSet( a.Animations );
			Playing    = a.Playing;
			Loop       = a.Loop;
			Multiplier = a.Multiplier;
			m_selected = a.m_selected;
			FrameIndex = a.FrameIndex;
			m_timer    = new Clock();
		}

		/// <summary>
		///   The animation set.
		/// </summary>
		public AnimationSet Animations { get; set; }

		/// <summary>
		///   If the animator is playing.
		/// </summary>
		public bool Playing
		{
			get { return m_playing; }
			private set { m_playing = value; }
		}
		/// <summary>
		///   If animations should loop.
		/// </summary>
		public bool Loop
		{
			get; set;
		}

		/// <summary>
		///   Frame length multiplier.
		/// </summary>
		/// <remarks>
		///   The frame length is multiplied by this number, so a value of 2.0 will make each frame take twice the time
		///   and 0.5 will make each frame take half the time. Setting this to 0.0 will have the same effect as pausing
		///   the animation.
		/// </remarks>
		public float Multiplier
		{
			get { return m_multiplier; }
			set { m_multiplier = value < 0.0f ? 0.0f : value; }
		}

		/// <summary>
		///   The selected animation ID.
		/// </summary>
		public string Selected
		{
			get { return m_selected; }
			set
			{
				if( Animations != null && Animations.Contains( value ) )
					m_selected = value.ToLower();

				FrameIndex = 0;
				m_timer.Restart();
			}
		}
		/// <summary>
		///   The index of the current frame.
		/// </summary>
		public uint FrameIndex
		{
			get { return m_frame; }
			private set { m_frame = value; }
		}
		/// <summary>
		///   Gets the current animation.
		/// </summary>
		public Animation CurrentAnimation
		{
			get
			{
				if( Animations.Empty )
					return null;

				if( !Animations.Contains( Selected ) )
				{
					var e = Animations.GetEnumerator();
					e.MoveNext();

					Selected = e.Current.Key;
					FrameIndex = 0;
				}

				return Animations[ Selected ];
			}
		}
		/// <summary>
		///   Gets the current frame of the current animation.
		/// </summary>
		public Frame CurrentFrame
		{
			get
			{
				if( Animations.Empty )
					return null;

				Frame f = null;

				try
				{
					Animation a = CurrentAnimation;

					if( FrameIndex < 0 )
						FrameIndex = 0;
					else if( FrameIndex >= a.Count )
						FrameIndex = (uint)( a.Count == 0 ? 0 : a.Count - 1 );

					f = CurrentAnimation?.Get( FrameIndex );
				}
				catch
				{
					f = null;
				}

				return f;
			}
		}

		/// <summary>
		///   Play the given animation from the beginning, restarting the
		///   current animation if the ID is invalid.
		/// </summary>
		/// <param name="id">
		///   The ID of the animation to play.
		/// </param>
		public void Play( string id = null )
		{
			if( !string.IsNullOrWhiteSpace( id ) )
				Selected = id;

			m_frame   = 0;
			m_playing = true;
			m_timer.Restart();
		}
		/// <summary>
		///   Pauses the animation if it is playing.
		/// </summary>
		public void Pause()
		{
			m_playing = false;
		}
		/// <summary>
		///   Stops the currently playing animation on the first frame.
		/// </summary>
		public void Stop()
		{
			m_frame   = 0;
			m_playing = false;
		}

		/// <summary>
		///   Updates the animator.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		public void Update( float dt )
		{
			if( Animations.Empty )
				return;

			if( Playing && Multiplier > 0.0f )
			{
				Animation anim = CurrentAnimation;

				if( anim.Count > 1 )
				{
					Time len = anim.Get( m_frame ).Length * Multiplier;

					if( m_timer.ElapsedTime >= len )
					{
						m_frame++;
						m_timer.Restart();
					}

					if( m_frame >= anim.Count )
					{
						m_frame   = 0;
						m_playing = Loop;
					}
				}
				else
					m_frame = 0;
			}
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
			if( br == null )
				return Logger.LogReturn( "Unable to load Animator from null stream.", false, LogType.Error );

			try
			{
				Loop       = br.ReadBoolean();
				Multiplier = br.ReadSingle();
				m_selected = br.ReadString();
			}
			catch( Exception e )
			{
				return Logger.LogReturn( "Unable to load Animator from stream: " + e.Message, false, LogType.Error );
			}

			if( !Animations.LoadFromStream( br ) )
				return Logger.LogReturn( "Unable to load Animator from stream: Failed loading AnimationSet.", false, LogType.Error );

			FrameIndex = 0;
			m_timer.Restart();

			return true;
		}
		/// <summary>
		///   Writes the object to the stream.
		/// </summary>
		/// <param name="bw">
		///   The stream writer.
		/// </param>
		/// <returns>
		///   True if the object was successfully written to the stream and false otherwise.
		/// </returns>
		public override bool SaveToStream( BinaryWriter bw )
		{
			if( bw == null )
				return Logger.LogReturn( "Unable to save Animator to null stream.", false, LogType.Error );

			try
			{
				bw.Write( Loop );
				bw.Write( Multiplier );
				bw.Write( m_selected );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( "Unable to save Animator to stream: " + e.Message, false, LogType.Error );
			}

			if( !Animations.SaveToStream( bw ) )
				return Logger.LogReturn( "Unable to save Animator to stream: Failed saving AnimationSet.", false, LogType.Error );

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
		public bool LoadFromXml( XmlElement element )
		{
			if( element == null )
				return Logger.LogReturn( "Cannot load Animator from a null XmlElement.", false, LogType.Error );

			Animations = new AnimationSet();
			Loop       = false;
			Multiplier = 1.0f;
			Selected   = null;

			XmlElement aset = element[ nameof( AnimationSet ) ];

			if( aset == null )
				return Logger.LogReturn( "Failed loading Animator: No AnimationSet xml element.", false, LogType.Error );
			if( !Animations.LoadFromXml( aset ) )
				return Logger.LogReturn( "Failed loading Animator: Loading AnimationSet failed.", false, LogType.Error );

			try
			{
				string loop = element.GetAttribute( nameof( Loop ) ),
					   mult = element.GetAttribute( nameof( Multiplier ) ),
					   sele = element.GetAttribute( nameof( Selected ) );

				if( !string.IsNullOrWhiteSpace( loop ) )
					Loop = bool.Parse( loop );
				if( !string.IsNullOrWhiteSpace( mult ) )
					Multiplier = float.Parse( mult );
				
				Selected = string.IsNullOrWhiteSpace( sele ) ? null : sele;

				if( Selected == null )
				{
					foreach( var s in Animations )
					{
						Selected = s.Key;
						break;
					}
				}
			}
			catch( Exception e )
			{
				return Logger.LogReturn( "Failed loading Animator: " + e.Message, false, LogType.Error );
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
			StringBuilder sb = new StringBuilder();

			sb.Append( "<" );
			sb.Append( nameof( Animator ) );
			sb.Append( " " + nameof( Loop ) + "=\"" );
			sb.Append( Loop );
			sb.AppendLine( "\"" );

			sb.Append( "          " + nameof( Multiplier ) + "=\"" );
			sb.Append( Multiplier );
			sb.AppendLine( "\"" );

			sb.Append( "          " + nameof( Selected ) + "=\"" );
			sb.Append( Selected ?? string.Empty );
			sb.AppendLine( "\">" );

			sb.AppendLine( XmlLoadable.ToString( Animations, 1 ) );

			sb.Append( "</" );
			sb.Append( nameof( Animator ) );
			sb.AppendLine( ">" );

			return sb.ToString();
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
		public bool Equals( Animator other )
		{
			return other      != null && Animations.Equals( other.Animations ) &&
			       Loop       == other.Loop &&
			       Multiplier == other.Multiplier &&
				   m_selected == other.m_selected;
		}

		private Clock  m_timer;
		private bool   m_playing;
		private string m_selected;
		private uint   m_frame;
		private float  m_multiplier;
	}
}
