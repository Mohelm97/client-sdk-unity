using UnityEngine;
using LiveKit.Proto;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;

namespace LiveKit
{
    public class TextureVideoSource : RtcVideoSource
    {
        TextureFormat _textureFormat;

        public Texture Texture { get; }

        public override int GetWidth()
        {
            return Texture.width;
        }

        public override int GetHeight()
        {
            return Texture.height;
        }

        public TextureVideoSource(Texture texture, VideoRotation rotation = VideoRotation._0, VideoBufferType bufferType = VideoBufferType.Rgba) : base(VideoStreamSource.Texture, bufferType)
        {
            Texture = texture;
            this.rotation = rotation;
            base.Init();
        }

        ~TextureVideoSource()
        {
            Dispose(); 
        }

        // Read the texture data into a native array asynchronously
        protected override void ReadBuffer()
        {
            if (_reading)
                return;
            _reading = true;
            if (!SystemInfo.IsFormatSupported(Texture.graphicsFormat, FormatUsage.ReadPixels))
            {
                if (_dest == null || _dest.width != GetWidth() || _dest.height != GetHeight())
                {
                    var compatibleFormat = SystemInfo.GetCompatibleFormat(Texture.graphicsFormat, FormatUsage.ReadPixels);
                    _textureFormat = GraphicsFormatUtility.GetTextureFormat(compatibleFormat);
                    _bufferType = GetVideoBufferType(_textureFormat);
                    _data = new NativeArray<byte>(GetWidth() * GetHeight() * GetStrideForBuffer(_bufferType), Allocator.Persistent);
                    _dest = new Texture2D(GetWidth(), GetHeight(), _textureFormat, false);
                }
                Graphics.CopyTexture(Texture, _dest);
            }
            else
            {
                _dest = Texture;
                _textureFormat = GraphicsFormatUtility.GetTextureFormat(Texture.graphicsFormat);
                _bufferType = GetVideoBufferType(_textureFormat);
            }
            
            AsyncGPUReadback.RequestIntoNativeArray(ref _data, _dest, 0, _textureFormat, OnReadback);
        }
    }
}

