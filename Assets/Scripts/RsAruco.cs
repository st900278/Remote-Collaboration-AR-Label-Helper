using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Intel.RealSense;

using UnityEngine;
using UnityEngine.Events;
using OpenCVForUnity;
using OpenCVForUnityExample;
[ProcessingBlockDataAttribute(typeof(RsAruco))]
public class RsAruco : RsProcessingBlock
{

    Mat rgbMat;

    void OnDisable()
    {
        rgbMat = null;
    }

    Frame ApplyFilter(VideoFrame color, FrameSource frameSource)
    {
        
        using (var p = color.Profile)
        {
            rgbMat = new Mat(color.Height, color.Width, CvType.CV_8UC3);
            byte[] dat = new byte[color.Height * color.Width * 3];
            Marshal.Copy(color.Data, dat, 0, color.Height * color.Width * 3);

            rgbMat.put(0, 0, dat);

            return color;
        }
        
    }

    public Mat getRgbMat()
    {
        return rgbMat;
    }

    public override Frame Process(Frame frame, FrameSource frameSource)
    {
        if (frame.IsComposite)
        {
            using (var fs = FrameSet.FromFrame(frame))
            using (var color = fs.ColorFrame)
            {
                var v = ApplyFilter(color, frameSource);
                // return v;

                // find and remove the original depth frame
                
                var frames = new List<Frame>();
                foreach (var f in fs)
                {
                    /*
                    using (var p1 = f.Profile)
                        if (p1.Stream == Stream.Depth && p1.Format == Format.Z16)
                        {
                            f.Dispose();
                            continue;
                        }
                    */
                    frames.Add(f);
                }
                //frames.Add(v);
                
                var res = frameSource.AllocateCompositeFrame(frames);
                frames.ForEach(f => f.Dispose());
                using (res)
                    return res.AsFrame();
            }
        }

        if (frame is VideoFrame)
            return ApplyFilter(frame as VideoFrame, frameSource);

        return frame;
    }
}

