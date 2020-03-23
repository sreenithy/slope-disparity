using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

public class DelayPCSweepScript : IEpiscanScript
{
    public int DelayMin { set; get; } = -500;
    public int DelayMax { set; get; } = 500;
    public int Shift { set; get; } = 10;
    public int AverageNum { set; get; } = 10;
    public int PCMin { set; get; } = 80;
    public int PCMax { set; get; } = 80;
    public int PCstep { set; get; } = 2;
    public float ExposureMin { set; get; } = 0.1f;
    public float ExposureMax { set; get; } = 0.1f;
    public float Exposurestep { set; get; } = 0.05f;





    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        // generate patterns


        // Switch to synchronized mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        episcan.Screen.BackColor = System.Drawing.Color.White;
        await Task.Delay(100);

        FolderBrowserDialog fbDialog = new FolderBrowserDialog();

        fbDialog.ShowNewFolderButton = true;

        int Expnum = (int)((ExposureMax - ExposureMin) / Exposurestep);
        float exposure = 0;

        if (fbDialog.ShowDialog() == DialogResult.OK)
        {
            var fname_black = fbDialog.SelectedPath + @"/capture_black.png";
            var fname_reg = fbDialog.SelectedPath + @"/capture_reg.png";

            int i = 0;
            for (int e = 0; e <= Expnum; e++)
            {
                exposure = ExposureMin + (float)(e * Exposurestep);

                for (int pc = PCMin; pc <= PCMax; pc += PCstep)
                {
                    var pc_dir = fbDialog.SelectedPath + $@"/pc_{pc:D2}";

                    Debug.WriteLine(pc_dir);

                    Directory.CreateDirectory(pc_dir);

                    //if (Directory.Exists(pc_dir))
                    //{
                    //    throw new System.ArgumentException("Already the directory exists", "pc_dir");
                    //}
                    //else
                    //{
                    //    Directory.CreateDirectory(pc_dir);
                    //}

                    for (int delay = DelayMin; delay <= DelayMax; delay += Shift)
                    {
                        episcan.SetDelayExposurePC(delay, exposure, pc);
                        await Task.Delay(1);

                        var captured = await episcan.CaptureAverage(AverageNum);
                        var filename = pc_dir + $@"/capture_{i:D5}_{(int)delay:D5}_{(int)(exposure * 1000):D5}_{pc:D3}.png";
                        Cv2.ImWrite(filename, captured);
                        i++;
                    }
                    await Task.Delay(100);

                }
            }

            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Global;
            episcan.SetDelayExposure(0, 13.5f);
            episcan.Screen.BackColor = System.Drawing.Color.Black;
            await Task.Delay(100);

            var black = await episcan.CaptureAverage(AverageNum);

            await Task.Delay(100);
            episcan.Screen.BackColor = System.Drawing.Color.White;
            await Task.Delay(100);

            var reg = await episcan.CaptureAverage(AverageNum);

            await Task.Delay(100);
            Cv2.ImWrite(fname_black, black);
            Cv2.ImWrite(fname_reg, reg);


        }
    }
}
