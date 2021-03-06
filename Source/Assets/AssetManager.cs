////////////////////////////////////////////////////////////////////////////////
// AssetManager.cs
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
using System.Collections;

namespace MiGfx
{
	/// <summary>
	///   Base class for asset managers.
	/// </summary>
	/// <typeparam name="T">
	///   The managed asset type.
	/// </typeparam>
	public abstract class AssetManager<T> : IEnumerable<KeyValuePair<string, T>> where T : class
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public AssetManager()
		{
			m_assets = new Dictionary<string, T>();
		}

		/// <summary>
		///   If the manager contains no assets.
		/// </summary>
		public bool Empty
		{
			get { return Count is 0; }
		}
		/// <summary>
		///   The amount of assets the manager contains.
		/// </summary>
		public int Count
		{
			get { return m_assets.Count; }
		}

		/// <summary>
		///   If an asset been loaded from the path.
		/// </summary>
		/// <param name="path">
		///   The path of the asset.
		/// </param>
		/// <returns>
		///   True if an asset has already been loaded from the path and false otherwise.
		/// </returns>
		public bool IsLoaded( string path )
		{
			string p = MakeAbsolute( path );

			if( string.IsNullOrWhiteSpace( p ) )
				return false;

			return m_assets.ContainsKey( p );
		}
		/// <summary>
		///   Gets the asset loaded from the given path, attempting to load a new one if needed.
		/// </summary>
		/// <remarks>
		///   Please note the given path should be relative to the executable as it will be 
		///   appended to the executable path.
		/// </remarks>
		/// <param name="path">
		///   The path of the asset.
		/// </param>
		/// <param name="reload">
		///   If an already loaded asset ahould be reloaded.
		/// </param>
		/// <returns>
		///   The asset loaded from the given path or null on failure.
		/// </returns>
		public T Get( string path, bool reload = false )
		{
			if( string.IsNullOrWhiteSpace( path ) )
				return null;

			string p = MakeAbsolute( path );

			if( !IsLoaded( p ) )
				if( !Load( p, reload ) )
					return null;

			return m_assets[ p ];
		}

		/// <summary>
		///   Loads an asset from the given path.
		/// </summary>
		/// <param name="path">
		///   The path to load the asset from.
		/// </param>
		/// <param name="reload">
		///   If an already existing asset should be reloaded.
		/// </param>
		/// <returns>
		///   True if the asset was/has been loaded successfully and false otherwise.
		/// </returns>
		protected abstract bool Load( string path, bool reload );
		/// <summary>
		///   Unloads the asset loaded from the given path.
		/// </summary>
		/// <remarks>
		///   Please note the given path should be relative to the executable as it will be 
		///   appended to the executable path.
		/// </remarks>
		/// <param name="path">
		///   The path of the asset.
		/// </param>
		/// <returns>
		///   True if the asset existed and was unloaded and removed successfully, otherwise false.
		/// </returns>
		public virtual bool Unload( string path )
		{
			if( string.IsNullOrWhiteSpace( path ) )
				return false;

			return m_assets.Remove( MakeAbsolute( path ) );
		}
		/// <summary>
		///   Unloads all assets.
		/// </summary>
		public virtual void Clear()
		{
			m_assets.Clear();
		}

		/// <summary>
		///   Gets an enumerator that can be used to iterate through the loaded assets.
		/// </summary>
		/// <returns>
		///   An enumerator that can be used to iterate through the loaded assets.
		/// </returns>
		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return ( (IEnumerable<KeyValuePair<string, T>>)m_assets ).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable)m_assets ).GetEnumerator();
		}

		/// <summary>
		///   Removes file URI from the given path and makes it absolute.
		/// </summary>
		/// <param name="path">
		///   The path to make absolute.
		/// </param>
		/// <returns>
		///   The absolute path on success, otherwise just path.
		/// </returns>
		protected string MakeAbsolute( string path )
		{
			if( string.IsNullOrWhiteSpace( path ) )
				return path;

			string result = path;

			if( result.StartsWith( "file:///" ) || result.StartsWith( "file:\\\\\\" ) )
				result = result[ 8.. ];
			else if( result.StartsWith( "file://" ) || result.StartsWith( "file:\\\\" ) )
				result = result[ 7.. ];

			if( result.Length > 0 )
			{
				if( result.StartsWith( "/" ) || result.StartsWith( "\\" ) )
					result = result[ 1.. ];

				string alpha = "abcdefghijklmnopqrstuvwxyz";

				bool absolute = result.Length > 1 && alpha.Contains( result.ToLower().Substring( 0, 1 ) ) && path[ 1 ] == ':';

				if( !absolute )
					result = FolderPaths.Executable + result;
			}

			return result;
		}

		/// <summary>
		///   Dictionary containing assets indexed by their file paths.
		/// </summary>
		protected Dictionary<string, T> m_assets;
	}

	/// <summary>
	///   Base class for asset managers with disposable assets.
	/// </summary>
	/// <typeparam name="T">
	///   The disposable managed asset type.
	/// </typeparam>
	public abstract class DisposableAssetManager<T> : AssetManager<T>, IDisposable where T : class, IDisposable
	{
		/// <summary>
		///   Constructor.
		/// </summary>
		public DisposableAssetManager()
		:	base()
		{ }

		/// <summary>
		///   Unloads the asset loaded from the given path.
		/// </summary>
		/// <remarks>
		///   Please note the given path should be relative to the executable as it will be 
		///   appended to the executable path.
		/// </remarks>
		/// <param name="path">
		///   The path of the asset.
		/// </param>
		/// <returns>
		///   True if the asset existed and was unloaded and removed successfully, otherwise false.
		/// </returns>
		public override bool Unload( string path )
		{
			if( string.IsNullOrWhiteSpace( path ) )
				return false;

			string p = MakeAbsolute( path );

			if( m_assets.ContainsKey( p ) )
				m_assets[ p ].Dispose();

			return m_assets.Remove( p );
		}
		/// <summary>
		///   Unloads all assets.
		/// </summary>
		public override void Clear()
		{
			foreach( var v in m_assets )
				v.Value?.Dispose();

			m_assets.Clear();
		}

		/// <summary>
		///   Disposes of all assets.
		/// </summary>
		public virtual void Dispose()
		{
			Clear();
			GC.SuppressFinalize( this );
		}
	}
}
