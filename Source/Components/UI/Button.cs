﻿////////////////////////////////////////////////////////////////////////////////
// Button.cs 
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

using SFML.Graphics;
using SFML.System;

using MiCore;

namespace MiGfx
{
	/// <summary>
	///   Contains visual button info.
	/// </summary>
	public class ButtonData : BinarySerializable, IXmlLoadable, IEquatable<ButtonData>
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public ButtonData()
		{
			Color      = Color.White;
			Text       = new TextStyle();
			TextOffset = new Vector2f();
		}
		/// <summary>
		///   Copy constructor.
		/// </summary>
		/// <param name="b">
		///   The object to copy.
		/// </param>
		public ButtonData( ButtonData b )
		{
			if( b is null )
				throw new ArgumentNullException( nameof( b ) );

			Color      = b.Color;
			Text       = new TextStyle( b.Text );
			TextOffset = b.TextOffset;
		}
		/// <summary>
		///   Constructor setting text style, offset and texture color.
		/// </summary>
		/// <param name="txt">
		///   The text style.
		/// </param>
		/// <param name="off">
		///   The text offset.
		/// </param>
		/// <param name="col">
		///   The texture color modifier.
		/// </param>
		public ButtonData( TextStyle txt, Vector2f off = default, Color? col = null )
		{
			Color      = col ?? Color.White;
			Text       = txt ?? new TextStyle();
			TextOffset = off;
		}

		/// <summary>
		///   The texture color modifier.
		/// </summary>
		public Color Color { get; set; }
		/// <summary>
		///   The text style.
		/// </summary>
		public TextStyle Text { get; set; }
		/// <summary>
		///   The text offset.
		/// </summary>
		public Vector2f TextOffset { get; set; }

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
			if( sr is null )
				return Logger.LogReturn( "Cannot load ButtonData from null stream.", false, LogType.Error );

			if( Text is null )
				Text = new TextStyle();

			if( !Text.LoadFromStream( sr ) )
				return Logger.LogReturn( "Failed loading ButtonData's Text from stream.", false, LogType.Error );

			try
			{
				Color      = new Color( sr.ReadByte(), sr.ReadByte(), sr.ReadByte(), sr.ReadByte() );
				TextOffset = new Vector2f( sr.ReadSingle(), sr.ReadSingle() );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( $"Failed loading ButtonData from stream: { e.Message }", false, LogType.Error );
			}

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
			if( sw is null )
				return Logger.LogReturn( "Cannot save ButtonData to null stream.", false, LogType.Error );

			if( Text is null )
				Text = new TextStyle();

			if( !Text.SaveToStream( sw ) )
				return Logger.LogReturn( "Failed saving ButtonData's TextStyle to stream.", false, LogType.Error );

			try
			{
				sw.Write( Color.R ); sw.Write( Color.G );
				sw.Write( Color.B ); sw.Write( Color.A );

				sw.Write( TextOffset.X ); sw.Write( TextOffset.Y );
			}
			catch( Exception e )
			{
				return Logger.LogReturn( $"Failed saving ButtonData to stream: { e.Message }", false, LogType.Error );
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
		public virtual bool LoadFromXml( XmlElement element )
		{
			if( element is null )
				return Logger.LogReturn( "Cannot load ButtonData from a null XmlElement.", false, LogType.Error );

			Text = new TextStyle();

			XmlElement col = element[ nameof( Color ) ],
					   txt = element[ nameof( TextStyle ) ],
					   off = element[ nameof( TextOffset ) ];

			if( txt is null )
				return Logger.LogReturn( "Failed loading ButtonData: No TextStyle xml element.", false, LogType.Error );

			Vector2f?  o = off is not null ? Xml.ToVec2f( off ) : null;
			Color?     c = col is not null ? Xml.ToColor( col ) : null;


			if( col is not null && !c.HasValue )
				return Logger.LogReturn( "Failed loading ButtonData: Unable to parse Color xml element.", false, LogType.Error );
			else if( !c.HasValue )
				c = Color.White;

			if( off is not null && !o.HasValue )
				return Logger.LogReturn( "Failed loading ButtonData: Unable to parse TextOffset xml element.", false, LogType.Error );
			else if( !o.HasValue )
				o = new Vector2f();

			if( !Text.LoadFromXml( txt ) )
				return Logger.LogReturn( "Failed loading ButtonData: Loading TextStyle failed.", false, LogType.Error );

			Color      = c.Value;
			TextOffset = o.Value;			
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
			return new StringBuilder().Append( '<' ).Append( nameof( ButtonData ) ).AppendLine( ">" )
				
				.AppendLine( Xml.ToString( Color, nameof( Color ), 1 ) )
				.AppendLine( XmlLoadable.ToString( Text, 1 ) )
				.AppendLine( Xml.ToString( TextOffset, nameof( TextOffset ), 1 ) )
				
				.Append( "</" ).Append( nameof( ButtonData ) ).AppendLine( ">" ).ToString();
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
		public bool Equals( ButtonData other )
		{
			return other is not null &&
			       Color.Equals( other.Color ) &&
				   Text.Equals( other.Text ) &&
				   TextOffset == other.TextOffset;
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
			return Equals( obj as ButtonData );
		}

		/// <summary>
		///   Serves as the default hash function.
		/// </summary>
		/// <returns>
		///   A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine( Color, Text, TextOffset );
		}
	}

	/// <summary>
	///   Button component.
	/// </summary>
	public class Button : MiComponent, IEquatable<Button>
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public Button()
		:	base()
		{
			Data = new ButtonData[ Enum.GetNames( typeof( ClickableState ) ).Length ];

			for( int i = 0; i < Data.Length; i++ )
				Data[ i ] = new ButtonData();
		}
		/// <summary>
		///  Copy constructor.
		/// </summary>
		/// <param name="b">
		///   The object to copy.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///   Inherited from <see cref="MiComponent(MiComponent)"/>.
		/// </exception>
		public Button( Button b )
		:	base( b )
		{
			Data = new ButtonData[ Enum.GetNames( typeof( ClickableState ) ).Length ];

			for( int i = 0; i < Data.Length; i++ )
				Data[ i ] = new ButtonData( b.Data[ i ] );
		}

		/// <summary>
		///   Type string unique to each UI element type.
		/// </summary>
		public override string TypeName
		{
			get { return nameof( Button ); }
		}

		/// <summary>
		///   Data for each state of the button.
		/// </summary>
		public ButtonData[] Data { get; private set; }

		/// <summary>
		///   Called when the component is added to an entity.
		/// </summary>
		public override void OnAdd()
		{
			Sprite spr = Parent.GetComponent<Sprite>();
			spr.Image = new ImageInfo( Path.Combine( FolderPaths.UI, "Button.png" ) );

			if( spr.Image.IsTextureValid )
			{
				Vector2u size = spr.Image.TextureSize;
				Parent.GetComponent<Transform>().Size = new Vector2f( size.X, size.Y / 3 );
			}

			ButtonData[] bd = Parent.GetComponent<Button>().Data;

			for( int i = 0; i < bd.Length; i++ )
			{
				bd[ i ].Text.FillColor = Color.White;
				bd[ i ].TextOffset = new Vector2f( 0, -8.0f );
			}

			Label lab  = Parent.GetComponent<Label>();
			lab.Allign = Allignment.Middle;
		}

		/// <summary>
		///   Refreshes components' visual elements.
		/// </summary>
		public override void Refresh()
		{
			if( Parent is null )
				return;

			Sprite  spr = Parent.GetComponent<Sprite>();
			Texture tex = spr.Image.Texture;

			if( tex is not null )
			{
				Vector2u size = tex.Size;
				spr.Image.Rect = new FloatRect( 0, 0, size.X, size.Y / 3u );
			}

			spr.Image.Color = Data[ 0 ].Color;

			Label lab = Parent.GetComponent<Label>();
			lab.Text = Data[ 0 ].Text;
			lab.Offset = Data[ 0 ].TextOffset;
		}
		/// <summary>
		///   Updates the elements' logic.
		/// </summary>
		/// <param name="dt">
		///   Delta time.
		/// </param>
		protected override void OnUpdate( float dt )
		{
			if( Parent is null )
				return;

			Sprite  spr = Parent.GetComponent<Sprite>();
			Texture tex = spr.Image.Texture;

			ClickableState state = Parent.GetComponent<Clickable>().ClickState;
			int s = (int)state;

			if( tex is not null )
			{
				Vector2u size = tex.Size;

				if( state is ClickableState.Hover )
					spr.Image.Rect = new FloatRect( 0, size.Y / 3u, size.X, size.Y / 3u );
				else if( state is ClickableState.Click )
					spr.Image.Rect = new FloatRect( 0, size.Y / 3u * 2, size.X, size.Y / 3u );
			}

			if( state != ClickableState.Idle )
			{
				spr.Image.Color = Data[ s ].Color;

				Label lab  = Parent.GetComponent<Label>();
				lab.Text   = Data[ s ].Text;
				lab.Offset = Data[ s ].TextOffset;
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
			return new string[] { nameof( Transform ), nameof( Selectable ), nameof( Clickable ),
			                      nameof( Sprite ),    nameof( Label ) };
		}
		/// <summary>
		///   Gets the type names of components incompatible with this component type.
		/// </summary>
		/// <returns>
		///   The type names of components incompatible with this component type.
		/// </returns>
		protected override string[] GetIncompatibleComponents()
		{
			return new string[] { nameof( CheckBox ), nameof( FillBar ), nameof( TextBox ),
								  nameof( SpriteAnimator ), nameof( SpriteArray ) };
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

			for( int i = 0; i < Data.Length; i++ )
				if( !Data[ i ].LoadFromStream( sr ) )
					return Logger.LogReturn( "Failed loading Button's ButtonData from stream.", false, LogType.Error );

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
			if( !base.SaveToStream( sw ) )
				return false;

			for( int i = 0; i < Data.Length; i++ )
				if( !Data[ i ].SaveToStream( sw ) )
					return Logger.LogReturn( "Failed saving Button's ButtonData to stream.", false, LogType.Error );

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

			XmlNodeList data = element.SelectNodes( nameof( ButtonData ) );

			if( data.Count != Data.Length )
				return Logger.LogReturn( "Failed loading Button: Incorrect amount of ButtonData elements.", false, LogType.Error );

			for( int i = 0; i < Data.Length; i++ )
			{
				Data[ i ] = new ButtonData();
				
				if( !Data[ i ].LoadFromXml( (XmlElement)data[ i ] ) )
					return Logger.LogReturn( "Failed loading Button's ButtonData from xml.", false, LogType.Error );
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
				.Append( "        " )
				.Append( nameof( Visible ) ).Append( "=\"" ).Append( Visible ).AppendLine( "\">" );

			for( int i = 0; i < Data.Length; i++ )
				sb.AppendLine( XmlLoadable.ToString( Data[ i ], 1 ) );

			sb.Append( "</" ).Append( TypeName ).Append( '>' );

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
		public bool Equals( Button other )
		{
			if( !base.Equals( other ) )
				return false;

			for( ClickableState b = 0; (int)b < Enum.GetNames( typeof( ClickableState ) ).Length; b++ )
				if( !Data[ (int)b ].Equals( other.Data[ (int)b ] ) )
					return false;

			return true;
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
			return Equals( obj as Button );
		}

		/// <summary>
		///   Serves as the default hash function.
		/// </summary>
		/// <returns>
		///   A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return HashCode.Combine( base.GetHashCode(), Data );
		}

		/// <summary>
		///   Clones this object.
		/// </summary>
		/// <returns>
		///   A clone of this object.
		/// </returns>
		public override object Clone()
		{
			return new Button( this );
		}

		/// <summary>
		///   Creates an entity and sets it up with a default button and an optional label string.
		/// </summary>
		/// <param name="id">
		///   The entity id.
		/// </param>
		/// <param name="window">
		///   The render window.
		/// </param>
		/// <param name="str">
		///   The label string or null to not add a label.
		/// </param>
		/// <returns>
		///   A valid entity containing the button on success or null on failure.
		/// </returns>
		public static MiEntity Create( string id = null, RenderWindow window = null, string str = null )
		{
			MiEntity ent = new( id, window );

			if( !ent.AddComponent( new Button(), true ) )
			{
				ent.Dispose();
				return Logger.LogReturn<MiEntity>( "Failed creating Button entity: Adding Button failed.", null, LogType.Error );
			}

			Sprite spr = ent.GetComponent<Sprite>();
			spr.Image = new ImageInfo( $"{ FolderPaths.UI }Button.png" );

			if( !spr.Image.IsTextureValid )
			{
				ent.Dispose();
				return Logger.LogReturn<MiEntity>( "Failed creating Button entity: Loading Texture failed.", null, LogType.Error );
			}

			if( window is not null )
			{
				Vector2u size = spr.Image.TextureSize;
				ent.GetComponent<Transform>().Size = new Vector2f( size.X, size.Y / 3 );
			}

			ButtonData[] bd = ent.GetComponent<Button>().Data;

			for ( int i = 0; i < bd.Length; i++ )
			{
				bd[ i ].Text.FillColor = Color.White;
				bd[ i ].TextOffset = new Vector2f( 0, -8.0f );
			}

			Label lab = ent.GetComponent<Label>();

			lab.String = string.IsNullOrEmpty( str ) ? string.Empty : str;
			lab.Allign = Allignment.Middle;

			return ent;
		}
	}
}
