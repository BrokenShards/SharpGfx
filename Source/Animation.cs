﻿////////////////////////////////////////////////////////////////////////////////
// Animation.cs 
////////////////////////////////////////////////////////////////////////////////
//
// SharpGfx - A basic graphics library for use with SFML.Net.
// Copyright (C) 2020 Michael Furlong <michaeljfurlong@outlook.com>
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
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

using SharpID;
using SharpSerial;

namespace SharpGfx
{
	/// <summary>
	///   A sprite-based animation frame.
	/// </summary>
	[Serializable]
	public class Frame : BinarySerializable
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Frame()
		{
			Rect   = new FloatRect();
			Length = Time.Zero;
		}
		/// <summary>
		///   Constructor assigning the texture rect and frame length.
		/// </summary>
		/// <param name="rect">
		///   The texture rect to display on the frame.
		/// </param>
		/// <param name="len">
		///   The length of time the frame lasts.
		/// </param>
		public Frame( FloatRect rect, Time len )
		{
			Rect   = rect;
			Length = len;
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="f">
		///   The frame to copy from.
		/// </param>
		public Frame( Frame f )
		{
			if( f == null )
				throw new ArgumentNullException();

			Rect   = f.Rect;
			Length = f.Length;
		}

		/// <summary>
		///   The texture rect to display on the frame.
		/// </summary>
		public FloatRect Rect { get; set; }
		/// <summary>
		///   The length of time the frame is displayed.
		/// </summary>
		public Time Length { get; set; }

		public override bool LoadFromStream( BinaryReader br )
		{
			if( br == null )
				return false;

			try
			{
				Rect   = new FloatRect( br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() );
				Length = Time.FromMicroseconds( br.ReadInt64() ); 
			}
			catch
			{
				return false;
			}

			return true;
		}
		public override bool SaveToStream( BinaryWriter bw )
		{
			if( bw == null )
				return false;

			try
			{
				bw.Write( Rect.Left ); bw.Write( Rect.Top ); bw.Write( Rect.Width ); bw.Write( Rect.Height );
				bw.Write( Length.AsMicroseconds() );
			}
			catch
			{
				return false;
			}

			return true;
		}
	}

	/// <summary>
	///   A sprite-based animation.
	/// </summary>
	[Serializable]
	public class Animation : BinarySerializable, IIdentifiable<string>
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Animation()
		{
			ID = Identifiable.NewStringID( "Animation" );
			m_frames = new List<Frame>();
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="a">
		///   The animation to copy from.
		/// </param>
		public Animation( Animation a )
		{
			if( a == null )
				throw new ArgumentNullException();

			ID = a.ID + "_Copy";
			m_frames = new List<Frame>( a.m_frames.Count );

			foreach( Frame f in a.m_frames )
				Add( f );
		}
		/// <summary>
		///   Constructor that assigns the ID.
		/// </summary>
		/// <param name="id">
		///   The animation ID.
		/// </param>
		public Animation( string id )
		{
			ID       = id;
			m_frames = new List<Frame>();
		}
		/// <summary>
		///   Constructs the animation with the given frame(s).
		/// </summary>
		/// <param name="fs">
		///   The list of frames to construct the animation with.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   If either the list of frames, or an individual frame is null.
		/// </exception>
		public Animation( params Frame[] fs )
		:	this()
		{
			try
			{
				Add( fs );
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		///   Constructs the animation with the given ID and frames.
		/// </summary>
		/// <param name="id">
		///   The animation ID.
		/// </param>
		/// <param name="fs">
		///   The list of frames to construct the animation with.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   If either the list of frames, or an individual frame is null.
		/// </exception>
		public Animation( string id, params Frame[] fs )
		{
			ID = id;
			m_frames = new List<Frame>();

			try
			{
				Add( fs );
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		///   ID accessor.
		/// </summary>
		public string ID { get; private set; }

		/// <summary>
		///   Frame accessor.
		/// </summary>
		/// <param name="index">
		///   The index of the desired frame.
		/// </param>
		/// <returns>
		///   The frame at the given index.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///   See <see cref="Get(uint)"/> and <see cref="Set(uint, Frame)"/>.
		/// </exception>
		public Frame this[ uint index ]
		{
			get { try { return Get( index ); } catch { throw; } }
			set { try { Set( index, value ); } catch { throw; } }
		}

		/// <summary>
		///   If the animation contains no frames.
		/// </summary>
		public bool Empty
		{
			get { return Count == 0; }
		}
		/// <summary>
		///   The amount of frames in the animation.
		/// </summary>
		public int Count
		{
			get { return m_frames.Count; }
		}

		/// <summary>
		///   Gets the frame at the given index.
		/// </summary>
		/// <param name="index">
		///   The index of the desired frame.
		/// </param>
		/// <returns>
		///   The frame at the given index.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///   If the given index is out of range.
		/// </exception>
		public Frame Get( uint index )
		{
			if( index < 0 || index >= Count )
				throw new ArgumentOutOfRangeException();

			return m_frames[ (int)index ];
		}
		/// <summary>
		///   Replaces an existing frame with a new one at the given index.
		/// </summary>
		/// <param name="index">
		///   The desired frame index.
		/// </param>
		/// <param name="f">
		///   The new frame.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		///   If the given index is out of range.
		/// </exception>
		public void Set( uint index, Frame f )
		{
			if( index < 0 || index >= Count )
				throw new ArgumentOutOfRangeException();

			m_frames[ (int)index ] = f;
		}

		/// <summary>
		///   Adds a frame to the end of the animation.
		/// </summary>
		/// <param name="f">
		///   The frame to add.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   If the frame to add is null.
		/// </exception>
		public void Add( Frame f )
		{
			if( f == null )
				throw new ArgumentNullException();

			m_frames.Add( f );
		}
		/// <summary>
		///   Adds multiple frames to the end of the animation.
		/// </summary>
		/// <param name="fs">
		///   List of frames to add.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   If either the list of frames, or an individual frame is null.
		/// </exception>
		public void Add( params Frame[] fs )
		{
			if( fs == null )
				throw new ArgumentNullException();

			try
			{
				foreach( Frame f in fs )
					Add( f );
			}
			catch
			{
				throw;
			}
		}
		/// <summary>
		///   Removes the frame at the given index.
		/// </summary>
		/// <param name="index">
		///   The index of the frame to remove.
		/// </param>
		public void Remove( uint index )
		{
			if( index >= 0 && index < Count )
				m_frames.RemoveAt( (int)index );
		}
		/// <summary>
		///   Removes all frames from the animation.
		/// </summary>
		public void RemoveAll()
		{
			m_frames.Clear();
		}

		public override bool LoadFromStream( BinaryReader br )
		{
			if( br == null )
				return false;

			try
			{
				ID = br.ReadString();
				for( int i = 0; i < Count; i++ )
					if( !m_frames[ i ].LoadFromStream( br ) )
						return false;
			}
			catch
			{
				return false;
			}

			return true;
		}
		public override bool SaveToStream( BinaryWriter bw )
		{
			if( bw == null )
				return false;

			bw.Write( ID );
			for( int i = 0; i < Count; i++ )
				if( !m_frames[ i ].SaveToStream( bw ) )
					return false;

			return true;
		}

		private List<Frame> m_frames;
	}

}