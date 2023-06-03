﻿#region License Information (GPL v3)

  /*
     Source code provocatively stolen from ShareX: https://github.com/ShareX/ShareX.
     (Seriously, awesome work over there, I used some of the parts to create an easy
     to use .NET package for everyone.)
     Their License:

     ShareX - A program that allows you to take screenshots and share any file type
     Copyright (c) 2007-2017 ShareX Team
     This program is free software; you can redistribute it and/or
     modify it under the terms of the GNU General Public License
     as published by the Free Software Foundation; either version 2
     of the License, or (at your option) any later version.
     This program is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warranty of
     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.
     You should have received a copy of the GNU General Public License
     along with this program; if not, write to the Free Software
     Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
     Optionally you can also view the license at <http://www.gnu.org/licenses/>.
 */

  #endregion License Information (GPL v3)


using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace AnimatedGif {
    /// <summary>
    ///     Quantize using an Octree
    /// </summary>
    public class OctreeQuantizer : Quantizer {
        /// <summary>
        ///     Maximum allowed color depth
        /// </summary>
        private readonly int _maxColors;

        /// <summary>
        ///     Stores the tree
        /// </summary>
        private readonly Octree _octree;

        /// <summary>
        ///     Construct the octree quantizer
        /// </summary>
        /// <remarks>
        ///     The Octree quantizer is a two pass algorithm. The initial pass sets up the octree,
        ///     the second pass quantizes a color based on the nodes in the tree
        /// </remarks>
        /// <param name="maxColors">The maximum number of colors to return</param>
        /// <param name="maxColorBits">The number of significant bits</param>
        public OctreeQuantizer(int maxColors, int maxColorBits)
            : base(false) {
            if (maxColors > 255)
                throw new ArgumentOutOfRangeException(nameof(maxColors), maxColors,
                    "The number of colors should be less than 256");

            if ((maxColorBits < 1) | (maxColorBits > 8))
                throw new ArgumentOutOfRangeException(nameof(maxColorBits), maxColorBits,
                    "This should be between 1 and 8");

            // Construct the octree
            _octree = new Octree(maxColorBits);
            _maxColors = maxColors;
        }

        /// <summary>
        ///     Process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        ///     This function need only be overridden if your quantize algorithm needs two passes,
        ///     such as an Octree quantizer.
        /// </remarks>
        protected override void InitialQuantizePixel(Color32 pixel) {
            // Add the color to the octree
            _octree.AddColor(pixel);
        }

        /// <summary>
        ///     Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected override byte QuantizePixel(Color32 pixel) {
            byte paletteIndex = (byte) _maxColors; // The color at [_maxColors] is set to transparent

            // Get the palette index if this non-transparent
            if (pixel.Alpha > 0)
                paletteIndex = (byte) _octree.GetPaletteIndex(pixel);

            return paletteIndex;
        }

        /// <summary>
        ///     Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="original">Any old palette, this is overrwritten</param>
        /// <returns>The new color palette</returns>
        protected override ColorPalette GetPalette(ColorPalette original) {
            // First off convert the octree to _maxColors colors
            var palette = _octree.Palletize(_maxColors - 1);

            // Then convert the palette based on those colors
            for (int index = 0; index < palette.Count; index++)
                original.Entries[index] = (Color) palette[index];

            // Add the transparent color
            original.Entries[_maxColors] = Color.FromArgb(0, 0, 0, 0);

            return original;
        }

        /// <summary>
        ///     Class which does the actual quantization
        /// </summary>
        private class Octree {
            /// <summary>
            ///     Mask used when getting the appropriate pixels for a given node
            /// </summary>
            private static readonly int[] Mask = {0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01};

            /// <summary>
            ///     Maximum number of significant bits in the image
            /// </summary>
            private readonly int _maxColorBits;

            /// <summary>
            ///     The root of the octree
            /// </summary>
            private readonly OctreeNode _root;

            /// <summary>
            ///     Cache the previous color quantized
            /// </summary>
            private int _previousColor;

            /// <summary>
            ///     Store the last node quantized
            /// </summary>
            private OctreeNode _previousNode;

            /// <summary>
            ///     Construct the octree
            /// </summary>
            /// <param name="maxColorBits">The maximum number of significant bits in the image</param>
            public Octree(int maxColorBits) {
                _maxColorBits = maxColorBits;
                Leaves = 0;
                ReducibleNodes = new OctreeNode[9];
                _root = new OctreeNode(0, _maxColorBits, this);
                _previousColor = 0;
                _previousNode = null;
            }

            private int Leaves { get; set; }

            protected OctreeNode[] ReducibleNodes { get; }

            /// <summary>
            ///     Add a given color value to the octree
            /// </summary>
            /// <param name="pixel"></param>
            public void AddColor(Color32 pixel) {
                // Check if this request is for the same color as the last
                if (_previousColor == pixel.ARGB) {
                    // If so, check if I have a previous node setup. This will only ocurr if the first color in the image
                    // happens to be black, with an alpha component of zero.
                    if (null == _previousNode) {
                        _previousColor = pixel.ARGB;
                        _root.AddColor(pixel, _maxColorBits, 0, this);
                    } else
                        // Just update the previous node
                    {
                        _previousNode.Increment(pixel);
                    }
                } else {
                    _previousColor = pixel.ARGB;
                    _root.AddColor(pixel, _maxColorBits, 0, this);
                }
            }

            /// <summary>
            ///     Reduce the depth of the tree
            /// </summary>
            private void Reduce() {
                int index;

                // Find the deepest level containing at least one reducible node
                for (index = _maxColorBits - 1; index > 0 && null == ReducibleNodes[index]; index--) { }

                // Reduce the node most recently added to the list at level 'index'
                var node = ReducibleNodes[index];
                ReducibleNodes[index] = node.NextReducible;

                // Decrement the leaf count after reducing the node
                Leaves -= node.Reduce();

                // And just in case I've reduced the last color to be added, and the next color to
                // be added is the same, invalidate the previousNode...
                _previousNode = null;
            }

            /// <summary>
            ///     Keep track of the previous node that was quantized
            /// </summary>
            /// <param name="node">The node last quantized</param>
            protected void TrackPrevious(OctreeNode node) {
                _previousNode = node;
            }

            /// <summary>
            ///     Convert the nodes in the octree to a palette with a maximum of colorCount colors
            /// </summary>
            /// <param name="colorCount">The maximum number of colors</param>
            /// <returns>An arraylist with the palettized colors</returns>
            public ArrayList Palletize(int colorCount) {
                while (Leaves > colorCount)
                    Reduce();

                // Now palettize the nodes
                var palette = new ArrayList(Leaves);
                int paletteIndex = 0;
                _root.ConstructPalette(palette, ref paletteIndex);

                // And return the palette
                return palette;
            }

            /// <summary>
            ///     Get the palette index for the passed color
            /// </summary>
            /// <param name="pixel"></param>
            /// <returns></returns>
            public int GetPaletteIndex(Color32 pixel) {
                return _root.GetPaletteIndex(pixel, 0);
            }

            /// <summary>
            ///     Class which encapsulates each node in the tree
            /// </summary>
            protected class OctreeNode {
                /// <summary>
                ///     Blue component
                /// </summary>
                private int _blue;

                /// <summary>
                ///     Green Component
                /// </summary>
                private int _green;

                /// <summary>
                ///     Flag indicating that this is a leaf node
                /// </summary>
                private bool _leaf;

                /// <summary>
                ///     The index of this node in the palette
                /// </summary>
                private int _paletteIndex;

                /// <summary>
                ///     Number of pixels in this node
                /// </summary>
                private int _pixelCount;

                /// <summary>
                ///     Red component
                /// </summary>
                private int _red;

                /// <summary>
                ///     Construct the node
                /// </summary>
                /// <param name="level">The level in the tree = 0 - 7</param>
                /// <param name="colorBits">The number of significant color bits in the image</param>
                /// <param name="octree">The tree to which this node belongs</param>
                public OctreeNode(int level, int colorBits, Octree octree) {
                    // Construct the new node
                    _leaf = level == colorBits;

                    _red = _green = _blue = 0;
                    _pixelCount = 0;

                    // If a leaf, increment the leaf count
                    if (_leaf) {
                        octree.Leaves++;
                        NextReducible = null;
                        Children = null;
                    } else {
                        // Otherwise add this to the reducible nodes
                        NextReducible = octree.ReducibleNodes[level];
                        octree.ReducibleNodes[level] = this;
                        Children = new OctreeNode[8];
                    }
                }

                /// <summary>
                ///     Get/Set the next reducible node
                /// </summary>
                public OctreeNode NextReducible { get; }

                /// <summary>
                ///     Return the child nodes
                /// </summary>
                private OctreeNode[] Children { get; }

                /// <summary>
                ///     Add a color into the tree
                /// </summary>
                /// <param name="pixel">The color</param>
                /// <param name="colorBits">The number of significant color bits</param>
                /// <param name="level">The level in the tree</param>
                /// <param name="octree">The tree to which this node belongs</param>
                public void AddColor(Color32 pixel, int colorBits, int level, Octree octree) {
                    // Update the color information if this is a leaf
                    if (_leaf) {
                        Increment(pixel);
                        // Setup the previous node
                        octree.TrackPrevious(this);
                    } else {
                        // Go to the next level down in the tree
                        int shift = 7 - level;
                        int index = ((pixel.Red & Mask[level]) >> (shift - 2)) |
                                    ((pixel.Green & Mask[level]) >> (shift - 1)) |
                                    ((pixel.Blue & Mask[level]) >> shift);

                        var child = Children[index];

                        if (null == child) {
                            // Create a new child node & store in the array
                            child = new OctreeNode(level + 1, colorBits, octree);
                            Children[index] = child;
                        }

                        // Add the color to the child node
                        child.AddColor(pixel, colorBits, level + 1, octree);
                    }
                }

                /// <summary>
                ///     Reduce this node by removing all of its children
                /// </summary>
                /// <returns>The number of leaves removed</returns>
                public int Reduce() {
                    _red = _green = _blue = 0;
                    int children = 0;

                    // Loop through all children and add their information to this node
                    for (int index = 0; index < 8; index++)
                        if (null != Children[index]) {
                            _red += Children[index]._red;
                            _green += Children[index]._green;
                            _blue += Children[index]._blue;
                            _pixelCount += Children[index]._pixelCount;
                            ++children;
                            Children[index] = null;
                        }

                    // Now change this to a leaf node
                    _leaf = true;

                    // Return the number of nodes to decrement the leaf count by
                    return children - 1;
                }

                /// <summary>
                ///     Traverse the tree, building up the color palette
                /// </summary>
                /// <param name="palette">The palette</param>
                /// <param name="paletteIndex">The current palette index</param>
                public void ConstructPalette(ArrayList palette, ref int paletteIndex) {
                    if (_leaf) {
                        // Consume the next palette index
                        _paletteIndex = paletteIndex++;

                        // And set the color of the palette entry
                        palette.Add(Color.FromArgb(_red / _pixelCount, _green / _pixelCount, _blue / _pixelCount));
                    } else {
                        // Loop through children looking for leaves
                        for (int index = 0; index < 8; index++)
                            if (null != Children[index])
                                Children[index].ConstructPalette(palette, ref paletteIndex);
                    }
                }

                /// <summary>
                ///     Return the palette index for the passed color
                /// </summary>
                public int GetPaletteIndex(Color32 pixel, int level) {
                    int paletteIndex = _paletteIndex;

                    if (!_leaf) {
                        int shift = 7 - level;
                        int index = ((pixel.Red & Mask[level]) >> (shift - 2)) |
                                    ((pixel.Green & Mask[level]) >> (shift - 1)) |
                                    ((pixel.Blue & Mask[level]) >> shift);

                        if (null != Children[index])
                            paletteIndex = Children[index].GetPaletteIndex(pixel, level + 1);
                        else
                            throw new Exception("Didn't expect this!");
                    }

                    return paletteIndex;
                }

                /// <summary>
                ///     Increment the pixel count and add to the color information
                /// </summary>
                public void Increment(Color32 pixel) {
                    _pixelCount++;
                    _red += pixel.Red;
                    _green += pixel.Green;
                    _blue += pixel.Blue;
                }
            }
        }
    }
}
