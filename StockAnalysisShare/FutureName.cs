using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class FutureName : TradingObjectName
    {
        private static Dictionary<string, string> ProductSymbolNameMap = new Dictionary<string, string>()
        {
            { "A","豆一" },
            { "AG","白银" },
            { "AGEFP","AGEFP" },
            { "AGL0","白银连续" },
            { "AGL3","白银连三" },
            { "AGL4","白银连四" },
            { "AGL8","白银主连" },
            { "AGL9","白银指数" },
            { "AL0","豆一连续" },
            { "AL","沪铝" },
            { "AL3","豆一连三" },
            { "AL8","豆一主连" },
            { "AL9","豆一指数" },
            { "ALEFP","ALEFP" },
            { "ALL0","沪铝连续" },
            { "ALL3","沪铝连三" },
            { "ALL4","沪铝连四" },
            { "ALL8","沪铝主连" },
            { "ALL9","沪铝指数" },
            { "AU","黄金" },
            { "AUEFP","AUEFP" },
            { "AUL0","黄金连续" },
            { "AUL3","黄金连三" },
            { "AUL4","黄金连四" },
            { "AUL8","黄金主连" },
            { "AUL9","黄金指数" },
            { "B","豆二" },
            { "BB","胶板" },
            { "BBL8","胶板主连" },
            { "BBL9","胶板指数" },
            { "BL0","豆二连续" },
            { "BL8","豆二主连" },
            { "BL9","豆二指数" },
            { "BU","沥青" },
            { "BUEFP","BUEFP" },
            { "BUL8","沥青主连" },
            { "BUL9","沥青指数" },
            { "C","玉米" },
            { "CF","郑棉" },
            { "CFL0","郑棉连续" },
            { "CFL8","郑棉主连" },
            { "CFL9","郑棉指数" },
            { "CL0","玉米连续" },
            { "CL8","玉米主连" },
            { "CL9","玉米指数" },
            { "CS","淀粉" },
            { "CSL8","淀粉主连" },
            { "CSL9","淀粉指数" },
            { "CU","沪铜" },
            { "CUEFP","CUEFP" },
            { "CUL0","沪铜连续" },
            { "CUL3","沪铜连三" },
            { "CUL4","沪铜连四" },
            { "CUL8","沪铜主连" },
            { "CUL9","沪铜指数" },
            { "FB","纤板" },
            { "FBL8","纤板主连" },
            { "FBL9","纤板指数" },
            { "FG","玻璃" },
            { "FGL0","玻璃连续" },
            { "FGL3","玻璃连三" },
            { "FGL8","玻璃主连" },
            { "FGL9","玻璃指数" },
            { "FU","燃油" },
            { "FUEFP","FUEFP" },
            { "FUL0","燃油连续" },
            { "FUL8","燃油主连" },
            { "FUL9","燃油指数" },
            { "HC","热卷" },
            { "HCEFP","HCEFP" },
            { "HCL8","热卷主连" },
            { "HCL9","热卷指数" },
            { "I","铁矿" },
            { "IC","中证" },
            { "IC500","中证500" },
            { "ICL8","中证主连" },
            { "ICL9","中证加权" },
            { "IF","沪深" },
            { "IF300","沪深300" },
            { "IFL0","沪深当月" },
            { "IFL1","沪深下月" },
            { "IFL2","沪深下季" },
            { "IFL3","沪深隔季" },
            { "IFL8","沪深主连" },
            { "IFL9","沪深加权" },
            { "IH","上证" },
            { "IH50","上证50" },
            { "IHL8","上证主连" },
            { "IHL9","上证加权" },
            { "IL8","铁矿主连" },
            { "IL9","铁矿指数" },
            { "IMCI","IMCI" },
            { "J","焦炭" },
            { "JD","鸡蛋" },
            { "JDL0","鸡蛋连续" },
            { "JDL3","鸡蛋连三" },
            { "JDL4","鸡蛋连四" },
            { "JDL8","鸡蛋主连" },
            { "JDL9","鸡蛋指数" },
            { "JL0","焦炭连续" },
            { "JL3","焦炭连三" },
            { "JL4","焦炭连四" },
            { "JL8","焦炭主连" },
            { "JL9","焦炭指数" },
            { "JM","焦煤" },
            { "JML0","焦煤连续" },
            { "JML3","焦煤连三" },
            { "JML4","焦煤连四" },
            { "JML8","焦煤主连" },
            { "JML9","焦煤指数" },
            { "JR","粳稻" },
            { "JRL8","粳稻主连" },
            { "JRL9","粳稻指数" },
            { "L","乙烯" },
            { "LL0","乙烯连续" },
            { "LL3","乙烯连三" },
            { "LL4","乙烯连四" },
            { "LL8","乙烯主连" },
            { "LL9","乙烯指数" },
            { "LR","晚稻" },
            { "LRL8","晚稻主连" },
            { "LRL9","晚稻指数" },
            { "M","豆粕" },
            { "MA","甲醇" },
            { "MAL8","甲醇主连" },
            { "MAL9","甲醇指数" },
            { "MEL0","甲醇连续" },
            { "MEL3","甲醇连三" },
            { "MEL8","甲醇主力" },
            { "MEL9","甲醇指数" },
            { "ML0","豆粕连续" },
            { "ML3","豆粕连三" },
            { "ML8","豆粕主连" },
            { "ML9","豆粕指数" },
            { "NI","沪镍" },
            { "NIL8","沪镍主连" },
            { "NIL9","沪镍指数" },
            { "OI","菜油" },
            { "OIL0","菜油连续" },
            { "OIL8","菜油主连" },
            { "OIL9","菜油指数" },
            { "P","棕榈" },
            { "PB","沪铅" },
            { "PBEFP","PBEFP" },
            { "PBL0","沪铅连续" },
            { "PBL3","沪铅连三" },
            { "PBL4","沪铅连四" },
            { "PBL8","沪铅主连" },
            { "PBL9","沪铅指数" },
            { "PL0","棕榈连续" },
            { "PL3","棕榈连三" },
            { "PL4","棕榈连四" },
            { "PL8","棕榈主连" },
            { "PL9","棕榈指数" },
            { "PM","普麦" },
            { "PML0","普麦连续" },
            { "PML3","普麦连三" },
            { "PML8","普麦主连" },
            { "PML9","普麦指数" },
            { "PP","丙烯" },
            { "PPL8","丙烯主连" },
            { "PPL9","丙烯指数" },
            { "RB","螺纹" },
            { "RBEFP","RBEFP" },
            { "RBL0","螺纹连续" },
            { "RBL3","螺纹连三" },
            { "RBL4","螺纹连四" },
            { "RBL8","螺纹主连" },
            { "RBL9","螺纹指数" },
            { "RI","早稻" },
            { "RIL0","早稻连续" },
            { "RIL8","早稻主连" },
            { "RIL9","早稻指数" },
            { "RM","菜粕" },
            { "RML0","菜粕连续" },
            { "RML8","菜粕主连" },
            { "RML9","菜粕指数" },
            { "RS","菜籽" },
            { "RSL0","菜籽连续" },
            { "RSL8","菜籽主连" },
            { "RSL9","菜籽指数" },
            { "RU","橡胶" },
            { "RUEFP","RUEFP" },
            { "RUL0","橡胶连续" },
            { "RUL3","橡胶连三" },
            { "RUL4","橡胶连四" },
            { "RUL8","橡胶主连" },
            { "RUL9","橡胶指数" },
            { "SF","硅铁" },
            { "SFL8","硅铁主连" },
            { "SFL9","硅铁指数" },
            { "SM","锰硅" },
            { "SML8","锰硅主连" },
            { "SML9","锰硅指数" },
            { "SN","沪锡" },
            { "SNL8","沪锡主连" },
            { "SNL9","沪锡指数" },
            { "SR","白糖" },
            { "SRL0","白糖连续" },
            { "SRL3","白糖连三" },
            { "SRL8","白糖主连" },
            { "SRL9","白糖指数" },
            { "T","十债" },
            { "TA","PTA" },
            { "TAL0","PTA连续" },
            { "TAL8","PTA主连" },
            { "TAL9","PTA指数" },
            { "TCL8","动煤主力" },
            { "TCL9","动煤指数" },
            { "TF","五债" },
            { "TFL0","五债当季" },
            { "TFL1","五债下季" },
            { "TFL2","五债隔季" },
            { "TFL8","五债主连" },
            { "TFL9","五债加权" },
            { "TL8","十债主连" },
            { "TL9","十债加权" },
            { "V","PVC" },
            { "VL0","PVC连续" },
            { "VL3","PVC连三" },
            { "VL4","PVC连四" },
            { "VL8","PVC主连" },
            { "VL9","PVC指数" },
            { "WH","强麦" },
            { "WHL0","强麦连续" },
            { "WHL3","强麦连三" },
            { "WHL8","强麦主连" },
            { "WHL9","强麦指数" },
            { "WR","线材" },
            { "WREFP","WREFP" },
            { "WRL0","线材连续" },
            { "WRL3","线材连三" },
            { "WRL4","线材连四" },
            { "WRL8","线材主连" },
            { "WRL9","线材指数" },
            { "Y","豆油" },
            { "YL0","豆油连续" },
            { "YL8","豆油主连" },
            { "YL9","豆油指数" },
            { "ZC","动煤" },
            { "ZCL8","动煤主连" },
            { "ZCL9","动煤指数" },
            { "ZN","沪锌" },
            { "ZNEFP","ZNEFP" },
            { "ZNL0","沪锌连续" },
            { "ZNL3","沪锌连三" },
            { "ZNL4","沪锌连四" },
            { "ZNL8","沪锌主连" },
            { "ZNL9","沪锌指数" },
        };

        public FutureName()
        {
        }

        private void SetValues(string rawSymbol)
        {
            RawSymbol = rawSymbol;
            NormalizedSymbol = rawSymbol;
        }

        private FutureName(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentNullException();
            }

            SetValues(symbol);
        }

        public FutureName(string symbol, string name)
            : this(symbol)
        {
            Names = new[] { name };

            if (string.IsNullOrEmpty(name))
            {
                Names[0] = FutureName.GetNameForProductSymbol(NormalizedSymbol);
            }
        }

        public FutureName(string symbol, string[] names)
            : this(symbol)
        {
            Names = names;
        }

        public override string SaveToString()
        {
            return NormalizedSymbol + "|" + String.Join("|", Names);
        }

        public override TradingObjectName ParseFromString(string s)
        {
            return FutureName.Parse(s);
        }

        public static FutureName Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException();
            }

            var fields = s.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields == null || fields.Length == 0)
            {
                throw new FormatException(string.Format("[{0}] is invalid future name", s));
            }

            var name = new FutureName(fields[0], fields.Length > 1 ? fields.Skip(1).ToArray() : new[] { string.Empty });

            return name;
        }

        public static string GetNameForProductSymbol(string productSymbol)
        {
            string originalProductSymbol = productSymbol;

            productSymbol = productSymbol.ToUpperInvariant();

            if (ProductSymbolNameMap.ContainsKey(productSymbol))
            {
                return ProductSymbolNameMap[productSymbol];
            }
            else
            {
                // try to remove numbers at the end.
                string numberString = string.Empty;
                string subProductSymbol = productSymbol;


                for (int i = productSymbol.Length - 1; i >= 0; --i)
                {
                    char ch = productSymbol[i];

                    if (!Char.IsDigit(ch))
                    {
                        numberString = productSymbol.Substring(i + 1, productSymbol.Length - i - 1);
                        subProductSymbol = productSymbol.Substring(0, i + 1);
                        break;
                    }
                }


                if (ProductSymbolNameMap.ContainsKey(subProductSymbol))
                {
                    return ProductSymbolNameMap[subProductSymbol] + numberString;
                }
                else // still can't find 
                {
                    return originalProductSymbol;
                }
            }
        }
    }
}
