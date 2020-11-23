using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    internal static class ICMPTypeCodeMap
    {                
        private static NestedMap<int, int, string> _entries;


        public static ICMPTypeCodeEntry GetEntry(int type, int code)
        {            
            var entry = _entries[type, code];
            return new ICMPTypeCodeEntry(entry.Item1, 
                                         entry.Item2, 
                                         entry.Item3);
        }


        private static void InitTypeCodeMap()
        {
            _entries = new NestedMap<int, int, string>();

            _entries.Add(0, 0, strings.ICMP_000_00);

            _entries.Add(1, 0, strings.ICMP_001_00);
            _entries.Add(2, 0, strings.ICMP_002_00);
            _entries.Add(3, 0, strings.ICMP_003_00);
            _entries.Add(3, 1, strings.ICMP_003_01);
            _entries.Add(3, 2, strings.ICMP_003_02);
            _entries.Add(3, 3, strings.ICMP_003_03);
            _entries.Add(3, 4, strings.ICMP_003_04);
            _entries.Add(3, 5, strings.ICMP_003_05);
            _entries.Add(3, 6, strings.ICMP_003_06);
            _entries.Add(3, 7, strings.ICMP_003_07);
            _entries.Add(3, 8, strings.ICMP_003_08);
            _entries.Add(3, 8, strings.ICMP_003_08);
            _entries.Add(3, 10, strings.ICMP_003_10);
            _entries.Add(3, 11, strings.ICMP_003_11);
            _entries.Add(3, 12, strings.ICMP_003_12);
            _entries.Add(3, 13, strings.ICMP_003_13);
            _entries.Add(3, 14, strings.ICMP_003_14);
            _entries.Add(3, 15, strings.ICMP_003_15);
            _entries.Add(4, 0, strings.ICMP_004_00);
            _entries.Add(5, 0, strings.ICMP_005_00);
            _entries.Add(5, 1, strings.ICMP_005_01);
            _entries.Add(5, 2, strings.ICMP_005_02);
            _entries.Add(5, 3, strings.ICMP_005_03);
            _entries.Add(6, 0, strings.ICMP_006_00);
            _entries.Add(7, 0, strings.ICMP_007_00);
            _entries.Add(8, 0, strings.ICMP_008_00);
            _entries.Add(9, 0, strings.ICMP_009_00);
            _entries.Add(10, 0, strings.ICMP_010_00);
            _entries.Add(11, 0, strings.ICMP_011_00);
            _entries.Add(11, 1, strings.ICMP_011_01);
            _entries.Add(12, 0, strings.ICMP_012_00);
            _entries.Add(12, 1, strings.ICMP_012_01);
            _entries.Add(12, 2, strings.ICMP_012_02);
            _entries.Add(13, 0, strings.ICMP_013_00);
            _entries.Add(14, 0, strings.ICMP_014_00);
            _entries.Add(15, 0, strings.ICMP_015_00);
            _entries.Add(16, 0, strings.ICMP_016_00);
            _entries.Add(17, 0, strings.ICMP_017_00);
            _entries.Add(18, 0, strings.ICMP_018_00);
            _entries.Add(19, 0, strings.ICMP_019_00);
            _entries.Add(20, 0, strings.ICMP_20To29);
            _entries.Add(21, 0, strings.ICMP_20To29);
            _entries.Add(22, 0, strings.ICMP_20To29);
            _entries.Add(23, 0, strings.ICMP_20To29);
            _entries.Add(24, 0, strings.ICMP_20To29);
            _entries.Add(25, 0, strings.ICMP_20To29);
            _entries.Add(26, 0, strings.ICMP_20To29);
            _entries.Add(27, 0, strings.ICMP_20To29);
            _entries.Add(28, 0, strings.ICMP_20To29);
            _entries.Add(29, 0, strings.ICMP_20To29);
            _entries.Add(30, 0, strings.ICMP_030_00);
            _entries.Add(31, 0, strings.ICMP_031_00);
            _entries.Add(32, 0, strings.ICMP_032_00);
            _entries.Add(33, 0, strings.ICMP_033_00);
            _entries.Add(34, 0, strings.ICMP_034_00);
            _entries.Add(35, 0, strings.ICMP_035_00);
            _entries.Add(36, 0, strings.ICMP_036_00);
            _entries.Add(37, 0, strings.ICMP_037_00);
            _entries.Add(38, 0, strings.ICMP_038_00);
            _entries.Add(39, 0, strings.ICMP_039_00);
            _entries.Add(40, 0, strings.ICMP_040_00);
            _entries.Add(41, 0, strings.ICMP_041_00);
            _entries.Add(42, 0, strings.ICMP_042_00);
            _entries.Add(43, 0, strings.ICMP_043_00);
            _entries.Add(43, 1, strings.ICMP_043_01);
            _entries.Add(43, 2, strings.ICMP_043_02);
            _entries.Add(43, 3, strings.ICMP_043_03);
            _entries.Add(43, 4, strings.ICMP_043_04);
            _entries.Add(253, 0, strings.ICMP_253_00);
            _entries.Add(254, 0, strings.ICMP_254_00);
            _entries.Add(255, 0, strings.ICMP_255_00);

            for (int i=44; i<=252; i++) {
                _entries.Add(i, 0, strings.ICMP_44To252);
            }
        }


        static ICMPTypeCodeMap()
        {
            InitTypeCodeMap();
        }
    }
}