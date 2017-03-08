using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Zentronic
{
    /// <summary>
    /// Vec3di.
    /// helper class for referencing one position with a astar grid
    /// </summary>
    [Serializable]
    public class Vec3di
    {
        public int x;
        public int y;
        public int z;

        public Vec3di()
        {
            x=0;y=0;z=0;
        }

        public Vec3di(int X, int Y, int Z )
        {
            x=X;
            y=Y;
            z=Z;
        }

        public Vec3di(Vec3di P)
        {
            x=P.x;
            y=P.y;
            z=P.z;
        }
        
        public bool Compare( Vec3di pos )
        {
            return ( x == pos.x && y == pos.y && z == pos.z );
        }

        public bool CompareXY( Vec3di pos )
        {
            return ( x == pos.x && y == pos.y );
        }

        public static implicit operator Vec3df(Vec3di d)
        {
            return new Vec3df(d.x,d.y,d.z);
        }
    }

    [Serializable]
    public class Vec3df
    {
        public float x;
        public float y;
        public float z;

        public Vec3df( )
        {
            x=0;y=0;z=0;
        }

        public Vec3df(Vector3 p)
        {
            x=p.x;y=p.y;z=p.z;
        }

        public static implicit operator Vec3di(Vec3df d)
        {
            return new Vec3di((int)(d.x+0.5f),(int)(d.y+0.5f),(int)(d.z+0.5f));
        }

        public Vec3df(int X, int Y, int Z )
        {
            x=X;
            y=Y;
            z=Z;
        }

        public Vec3df(Vec3df P)
        {
            x=P.x;
            y=P.y;
            z=P.z;
        }

        public bool Compare( Vec3df pos )
        {
            return ( x == pos.x && y == pos.y && z == pos.z );
        }

        public bool CompareXY( Vec3df pos )
        {
            return ( x == pos.x && y == pos.y );
        }
    }

    /// <summary>
    /// helper class.
    /// Represents a 3 dimensional grid of integers. 
    /// With depth set to one it can be used for tilemaps.
    /// With depth bigger one it can be used for multi level tilemaps, like dungeons
    /// </summary>
    public class Grid3di
    {
        public Grid3di()
        {
            _data = null;
            _width=0;
            _height=0;
            _depth =0;
        }

        public Grid3di(Grid3di rhs)
        {
            _width=0;
            _height=0;
            _depth=0;
            _data = null;
            CopyFrom(rhs);
        }

        public Grid3di(int w, int h, int d=1)
        {
            _data = null;
            _width=0;
            _height=0;
            _depth=0;
            Resize(w,h,d);
        }
 
        public int Width  
        {
            get
            {
                return  _width;

            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public int Depth
        {
            get
            {
                return _depth;
            }
        }

        /// <summary>
        /// the code we use for borders or walls in grid
        /// This code will be checked against by the pathfinding logic
        /// </summary>
        /// <value>The border code.</value>
        public int BorderCode 
        {
            get
            {
                return _borderCode;
            }
            set
            {
                _borderCode=value;
            }
        }

        /// <summary>
        /// access read/write with x and y coordinate
        /// Left/Top is 0/0
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public int this[int x,int y, int z=0]
        {
            get
            {
                int idx = x+y*_width + z*_width*_height;

                if( IsIndexInRange(idx))
                {
                    return _data[idx];
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                int idx = x+y*_width+ z*_width*_height;

                if( IsIndexInRange(idx))
                {
                    _data[idx] = value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public int this[int idx]
        {
            get
            {
                if( IsIndexInRange(idx))
                {
                    return _data[idx];
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                if( IsIndexInRange(idx))
                {
                    _data[idx]=value;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public void Clear()
        {
            if (_width > 0 && _height > 0 && _depth > 0 && _data != null)
            {
                for (int z = 0; z < _depth; z++)
                {
                    int idx0 = z * _width * _height;

                    for (int y = 0; y < _height; y++)
                    {
                        int idx1 = y * _width;
                        for (int x = 0; x < _width; x++)
                        {
                            _data[x+ idx0 + idx1] = 0;
                        }
                    }
                }
            }
        }

        public void Resize(int width, int height, int depth=1)
        {
             
            if( width>0 && height>0 && depth>0)
            {
                _width = width;
                _height = height;
                _depth = depth;

                _data = new int[width*height*depth];

                for (int z=0;z<_depth;z++)
                {
                    int idx0 = z * _width * _height;

                    for (int y=0;y<_height;y++)
                    {
                        int idx1 = y * _width;

                        for ( int x = 0; x< _width; x++)
                        {
                            _data[x+ idx0 + idx1]=0;
                        }
                    }
                }
            }
            else
            {
                //Debug::log(Debug::ERROR,"resizing tilemap to 0x0 may lead to errors!");
                _data = null;
            }

            _width = width;
            _height = height;
            _depth = depth;
        }

        /// <summary>
        /// Pad the map with a given border code. In XY direction only
        /// </summary>
        /// <param name="border_code">Border code.</param>
        public void PadXY( int border_code)
        {
            if( _width > 0 && _width > 0 && _data!=null)
            {
                int w = _width  + 2;
                int h = _height + 2;

                int [] mpDataNew = new int[w*h*_depth];

                for (int y=0;y<_height;y++)
                {
                    for ( int x = 0; x< _width; x++)
                    {
                        int idx = x+y*_width;
                        int idxn = x + (y+1)*w + 1;
                        mpDataNew[idxn]= _data[idx];
                    }
                }

                for ( int x = 0; x< w; x++)
                {
                    mpDataNew[x]        =border_code;
                    mpDataNew[x+w*(h-1)]=border_code;
                }

                for (int y=0;y<h;y++)
                {
                    mpDataNew[w*y]      =border_code;
                    mpDataNew[w*y+(w-1)]=border_code;
                }

                _width = w;
                _height = h;
                _data = null;
                _data = mpDataNew;
            }
        }

        /// <summary>
        /// Parses the layer from string. 
        /// Allows to have maps as nicley formated strings in your app
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="valueLookup">Value lookup.</param>
        /// <param name="z">The z coordinate.</param>
        public void ParseLayerFromString( string data, Dictionary<char,int> valueLookup, int z )
        {
            int idx0=z*_width*_height;

            for (int y=0;y<_height;y++)
            {
                int idx1 = y * _width;
                for ( int x = 0; x< _width; x++)
                {
                    int idx  = x + idx1;
                    int idxd = x + idx1 + idx0;

                    if (idx < data.Length)
                    {
                        char val = data[idx];

                        if( valueLookup.ContainsKey(val))
                        {
                            _data[idxd]= valueLookup[val];

                        }
                    }
                   
                }
            }
        }
 
        /// <summary>
        /// draws a random position in the grid with a given code
        /// </summary>
        /// <returns><c>true</c>, if position with code was randomed, <c>false</c> otherwise.</returns>
        /// <param name="code">Code.</param>
        /// <param name="px">Px.</param>
        /// <param name="py">Py.</param>
        public bool RandomPositionWithCode( int code,  ref int px, ref int py ,int maxretries = 1000)
        {
            if( _width>0 && _height>0 && _data!=null)
            {
                bool found = false;
                int NumRetries = 0;

                while(!found && (NumRetries<maxretries))
                {
                    int x = UnityEngine.Random.Range(0,_width);// rand() % map_width;
                    int y = UnityEngine.Random.Range(0,_height); //rand() % map_heigth;

                    // PRO_loge(LL_DEBUG, "testing rand position %d/%d",x,y);
                    int codeAt = this[x,y];// GetAt(x, y);

                    if( codeAt == code)
                    {
                        int idx = x+y*_width;

                        if( IsIndexInRange(idx))
                        {
                            px = x;
                            py = y;
                            return true;
                        }
                    }

                    NumRetries++;
                }
            }

            return false;
        }
 
        public int GetAt(int x, int y, int border_value)
        {
            if( x < 0 ||  x >= _width || y < 0 || y >= _height )
            {
                return border_value;
            }

            return _data[x+y*_width];
        }

        public int GetAt(int x, int y, int z, int border_value)
        {
            if( x < 0 ||  x >= _width || y < 0 || y >= _height || z < 0 || z >= _depth )
            {
                return border_value;
            }

            return _data[x+y*_width+z*_width*_height];
        }
 
        public int GetAt( Vec3di pos )
        {
            if( pos.x < 0 ||  pos.x >= _width || pos.y < 0 || pos.y >= _height|| pos.z < 0 || pos.z >= _depth )
            {
                return _borderCode;
            }

            return _data[pos.x+pos.y*_width+pos.z*_width*_height];
        }

        public void SetAt(  int x, int y, int code )
        {
            int idx = x+y*_width;

            if( IsIndexInRange(idx))
            {
                _data[idx]=code;
            }
        }

        public void SetAt(int x, int y, int z, int code )
        {
            int idx = x+y*_width + z*_width*_height;

            if( IsIndexInRange(idx))
            {
                _data[idx]=code;
            }
        }
        
        public void ShowDebugInfo(Dictionary<int,char> codeLookup)  
        {
            int MW = Width;
            int MH = Height;

            if( MW>0 && MH>0)
            {

                string line;
                string mline;

                Debug.Log( "map: " + MW +"x" + MH);

                mline = " +";

                for (int x=0;x<MW;x++)
                {
                    mline += String.Format("{0:0}",(x%10))[0];
                }

                mline += "+";

                Debug.Log(mline);

                for (int y=0;y<MH;y++)
                {
                    line = String.Format("{0:0}|", (y%10));

                    for (int x=0;x<MW;x++)
                    {
                        /// default cost for empty field
                        int code = this[x,y];// GetAt(x, y);

                        if( codeLookup.ContainsKey(code))
                        {
                            line += codeLookup[code];
                        }
                        else
                        {
                            line += ' ';
                        }
                    }

                    line += "|";

                    Debug.Log(line);

                    line = "";
                }

                Debug.Log(mline);
            }
        }
        
        void CopyFrom(Grid3di rhs)
        {
            _width = rhs._width;
            _height = rhs._height;
            _depth = rhs._depth;

            Resize(_width, _height, _depth);

            if( _width >0 && _height>0 &&_depth>0 && _data!=null)
            {
                Array.Copy(rhs._data,_data,_width*_height*_depth);
            }

            _borderCode = rhs._borderCode;       
        }

        bool IsIndexInRange(int idx)  
        {
            if( _data != null && (idx < _width*_height*_depth))
            {
                return true;
            }

            return false;
        }


       
        int [] _data=null;
        int _borderCode;
        int _width;
        int _height; 
        int _depth;   
    }

   
}  