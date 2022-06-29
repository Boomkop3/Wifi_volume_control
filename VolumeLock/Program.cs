using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Boomkop3;

namespace VolumeLock
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("starting up...");
            var controller = new CoreAudioController();
            var devicesEnumerable = controller.GetPlaybackDevices();
            (bool muted, double volume, CoreAudioDevice device)[] devices = new (bool muted, double volume, CoreAudioDevice device)[devicesEnumerable.Count()];
            {
                int i = 0;
                foreach (var device in devicesEnumerable)
                {
                    devices[i++] = (device.IsMuted, device.Volume, device);
                }
            }
            int cycleTime = 200;
            int deviceTime = cycleTime / (devices.Length);
            while (true)
            {
                Thread.Sleep(cycleTime);
                Console.WriteLine("cycle");
                Task.Run(() =>
                {
                    devices.WithIndex().ForAll((device) =>
                    {
                        Thread.Sleep(device.index * deviceTime);
                        Console.WriteLine(device.index * deviceTime);
                        device.obj.device.Mute(device.obj.muted);
                        device.obj.device.Volume = device.obj.volume;
                    });
                });
            }
        }
    }
}
