// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace System.Web {
    using System;
    
    
    internal sealed class UplevelHelper {
        
        public static bool IsUplevel(string ua) {
            if ((ua == null)) {
                return false;
            }
            int ualength = ua.Length;
            if ((ualength == 0)) {
                return false;
            }
            bool hasJavaScript = false;
            if (((ualength > 3) 
                        && ((ua[0] == 'M') 
                        && ((ua[1] == 'o') 
                        && ((ua[2] == 'z') 
                        && (ua[3] == 'i')))))) {
                if (UplevelHelper.DetermineUplevel_1_1(ua, out hasJavaScript, ualength)) {
                    return hasJavaScript;
                }
                else {
                    return false;
                }
            }
            if (((ualength > 3) 
                        && ((ua[0] == 'K') 
                        && ((ua[1] == 'o') 
                        && ((ua[2] == 'n') 
                        && (ua[3] == 'q')))))) {
                return true;
            }
            if (((ualength > 3) 
                        && ((ua[0] == 'O') 
                        && ((ua[1] == 'p') 
                        && ((ua[2] == 'e') 
                        && (ua[3] == 'r')))))) {
                return true;
            }
            return false;
        }
        
        private static bool DetermineUplevel_1_1(string ua, out bool hasJavaScript, int ualength) {
            hasJavaScript = true;
            if (((ualength > 10) 
                        && ((ua[7] == '/') 
                        && ((ua[8] == '4') 
                        && ((ua[9] == '.') 
                        && (ua[10] == '0')))))) {
                if (((ualength > 28) 
                            && ((ua[13] == 'A') 
                            && ((ua[14] == 'c') 
                            && ((ua[15] == 't') 
                            && ((ua[16] == 'i') 
                            && ((ua[17] == 'v') 
                            && ((ua[18] == 'e') 
                            && ((ua[19] == 'T') 
                            && ((ua[20] == 'o') 
                            && ((ua[21] == 'u') 
                            && ((ua[22] == 'r') 
                            && ((ua[23] == 'i') 
                            && ((ua[24] == 's') 
                            && ((ua[25] == 't') 
                            && ((ua[26] == 'B') 
                            && ((ua[27] == 'o') 
                            && (ua[28] == 't')))))))))))))))))) {
                    hasJavaScript = false;
                    return true;
                }
                hasJavaScript = true;
                return true;
            }
            if (((ualength > 10) 
                        && ((ua[7] == '/') 
                        && ((ua[8] == '5') 
                        && ((ua[9] == '.') 
                        && (ua[10] == '0')))))) {
                if (((ualength > 28) 
                            && ((ua[13] == 'A') 
                            && ((ua[14] == 'c') 
                            && ((ua[15] == 't') 
                            && ((ua[16] == 'i') 
                            && ((ua[17] == 'v') 
                            && ((ua[18] == 'e') 
                            && ((ua[19] == 'T') 
                            && ((ua[20] == 'o') 
                            && ((ua[21] == 'u') 
                            && ((ua[22] == 'r') 
                            && ((ua[23] == 'i') 
                            && ((ua[24] == 's') 
                            && ((ua[25] == 't') 
                            && ((ua[26] == 'B') 
                            && ((ua[27] == 'o') 
                            && (ua[28] == 't')))))))))))))))))) {
                    hasJavaScript = false;
                    return true;
                }
                hasJavaScript = true;
                return true;
            }
            if (UplevelHelper.ScanForMatch_2_3(ua, out hasJavaScript, ualength)) {
                return true;
            }
            if (UplevelHelper.ScanForMatch_2_4(ua, out hasJavaScript, ualength)) {
                return true;
            }
            if (((ualength > 15) 
                        && ((ua[12] == '(') 
                        && ((ua[13] == 'M') 
                        && ((ua[14] == 'a') 
                        && (ua[15] == 'c')))))) {
                hasJavaScript = true;
                return true;
            }
            if (UplevelHelper.ScanForMatch_2_6(ua, out hasJavaScript, ualength)) {
                return true;
            }
            if (((ualength > 15) 
                        && ((ua[12] == 'G') 
                        && ((ua[13] == 'a') 
                        && ((ua[14] == 'l') 
                        && (ua[15] == 'e')))))) {
                hasJavaScript = true;
                return true;
            }
            if (((ualength > 28) 
                        && ((ua[25] == 'K') 
                        && ((ua[26] == 'o') 
                        && ((ua[27] == 'n') 
                        && (ua[28] == 'q')))))) {
                hasJavaScript = true;
                return true;
            }
            if (((ualength > 12) 
                        && (((ua[9] == '/') 
                        && ((ua[10] == '4') 
                        && (ua[11] == '.'))) 
                        && (ua[12] == '[')))) {
                hasJavaScript = true;
                return true;
            }
            return false;
        }
        
        private static bool ScanForMatch_2_3(string ua, out bool hasJavaScript, int ualength) {
            hasJavaScript = true;
            if ((ualength < 25)) {
                return false;
            }
            int startPosition = 0;
            int endPosition = (startPosition + 7);
            for (int ualeft = ualength; (ualeft >= 8); ualeft = (ualeft - 1)) {
                if ((((ua[(startPosition + 0)] == ')') 
                            && (ua[(endPosition - 0)] == '/')) 
                            && (((ua[(startPosition + 1)] == ' ') 
                            && (ua[(endPosition - 1)] == 'o')) 
                            && (((ua[(startPosition + 2)] == 'G') 
                            && (ua[(endPosition - 2)] == 'k')) 
                            && ((ua[(startPosition + 4)] == 'c') 
                            && (ua[(endPosition - 4)] == 'e')))))) {
                    hasJavaScript = true;
                    return true;
                }
                startPosition = (startPosition + 1);
                endPosition = (endPosition + 1);
            }
            return false;
        }
        
        private static bool ScanForMatch_2_4(string ua, out bool hasJavaScript, int ualength) {
            hasJavaScript = true;
            if ((ualength < 24)) {
                return false;
            }
            int startPosition = 0;
            int endPosition = (startPosition + 6);
            for (int ualeft = ualength; (ualeft >= 7); ualeft = (ualeft - 1)) {
                if ((((ua[(startPosition + 0)] == ')') 
                            && (ua[(endPosition - 0)] == 'a')) 
                            && (((ua[(startPosition + 1)] == ' ') 
                            && (ua[(endPosition - 1)] == 'r')) 
                            && (((ua[(startPosition + 2)] == 'O') 
                            && (ua[(endPosition - 2)] == 'e')) 
                            && (ua[(startPosition + 3)] == 'p'))))) {
                    hasJavaScript = true;
                    return true;
                }
                startPosition = (startPosition + 1);
                endPosition = (endPosition + 1);
            }
            return false;
        }
        
        private static bool ScanForMatch_2_6(string ua, out bool hasJavaScript, int ualength) {
            hasJavaScript = true;
            if ((ualength < 21)) {
                return false;
            }
            int startPosition = 0;
            int endPosition = (startPosition + 5);
            for (int ualeft = ualength; (ualeft >= 6); ualeft = (ualeft - 1)) {
                if ((((ua[(startPosition + 0)] == '(') 
                            && (ua[(endPosition - 0)] == 'L')) 
                            && (((ua[(startPosition + 1)] == 'K') 
                            && (ua[(endPosition - 1)] == 'M')) 
                            && ((ua[(startPosition + 3)] == 'T') 
                            && (ua[(endPosition - 3)] == 'H'))))) {
                    hasJavaScript = true;
                    return true;
                }
                startPosition = (startPosition + 1);
                endPosition = (endPosition + 1);
            }
            return false;
        }
    }
}
