using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs.common
{
    static class BytesConverter
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ConvertToSize(this ulong value, SizeUnits unit)
        {
            return (value / (double)Math.Pow(1024, (ulong)unit)).ToString("0.00");
        }
    }
}
