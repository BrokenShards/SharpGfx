﻿////////////////////////////////////////////////////////////////////////////////
// AssetTest.cs 
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

using SFML.Audio;
using SFML.Graphics;

using SharpGfx;
using SharpLogger;

namespace SharpGfxTest
{
	/// <summary>
	///  Asset tests.
	/// </summary>
	partial class Test
	{
		static readonly string FontPath    = FilePaths.DefaultFont;
		static readonly string SoundPath   = FolderPaths.Sounds   + "test.wav";
		static readonly string TexturePath = FolderPaths.Textures + "test.png";

		static bool AssetTests()
		{
			bool result = true;

			if( !FontTest() )
				result = false;
			if( !SoundTest() )
				result = false;
			if( !TextureTest() )
				result = false;

			return result;
		}

		static bool FontTest()
		{
			Logger.Log( "Running FontManager Tests..." );

			Font font = Assets.Manager.Get<Font>( FontPath );

			if( font == null )
				return Logger.LogReturn( "Failed: Unable to load font from path.", false );

			Assets.Manager.Font.Unload( FontPath );

			return Logger.LogReturn( "Success!", true );
		}
		static bool SoundTest()
		{
			Logger.Log( "Running SoundManager Tests..." );

			SoundBuffer snd = Assets.Manager.Get<SoundBuffer>( SoundPath );

			if( snd == null )
				return Logger.LogReturn( "Failed: Unable to load sound from path.", false );

			Assets.Manager.Sound.Unload( SoundPath );

			return Logger.LogReturn( "Success!", true );
		}
		static bool TextureTest()
		{
			Logger.Log( "Running TextureManager Tests..." );

			Texture tex = Assets.Manager.Get<Texture>( TexturePath );

			if( tex == null )
				return Logger.LogReturn( "Failed: Unable to load texture from path.", false );

			Assets.Manager.Sound.Unload( TexturePath );

			return Logger.LogReturn( "Success!", true );
		}
	}
}
