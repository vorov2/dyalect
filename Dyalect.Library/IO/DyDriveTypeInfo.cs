using Dyalect.Compiler;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.IO;
using System.Linq;

namespace Dyalect.Library.IO
{
    public sealed class DyDriveTypeInfo : DyForeignTypeInfo<IOModule>
    {
        public override string ReflectedTypeName => "Drive";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyFunction Handle<T>(string name, DyObject self, Func<DriveInfo, T> func)
        {
            var drv = ((DyDrive)self).Value;

            return Func.Auto(name, (ctx, _) =>
            {
                try
                {
                    return TypeConverter.ConvertFrom(func(drv));
                }
                catch (Exception)
                {
                    return ctx.IOFailed();
                }
            });
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch 
            {
                "Name" => Handle(name, self, drv => drv.Name),
                "TotalSize" => Handle(name, self, drv => drv.TotalSize),
                "TotalFreeSize" => Handle(name, self, drv => drv.TotalFreeSpace),
                "AvailableFreeSpace" => Handle(name, self, drv => drv.AvailableFreeSpace),
                "Format" => Handle(name, self, drv => drv.DriveFormat),
                "Root" => Handle(name, self, drv => drv.RootDirectory.FullName),
                "Type" => Handle(name, self, drv => drv.DriveType.ToString()),
                "IsReady" => Handle(name, self, drv => drv.IsReady),
                "Label" => Handle(name, self, drv => drv.VolumeLabel),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject GetDrives(ExecutionContext ctx)
        {
            try
            {
                return new DyArray(DriveInfo.GetDrives().Select(d => new DyDrive(this, d)).ToArray());
            }
            catch (Exception)
            {
                return ctx.IOFailed();
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "GetDrives" => Func.Static(name, GetDrives),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
